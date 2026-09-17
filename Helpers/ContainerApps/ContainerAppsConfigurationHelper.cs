using DeploymentKit.Constants;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Helpers.ContainerApps;

/// <summary>
/// Helper class for creating container app environment configurations.
/// </summary>
public static class ContainerAppsConfigurationHelper
{
    /// <summary>
    /// Creates the managed environment arguments.
    /// </summary>
    /// <param name="settings">The infrastructure settings.</param>
    /// <param name="resourceGroup">The resource group name.</param>
    /// <param name="monitoring">The monitoring outputs.</param>
    /// <param name="network">The network outputs.</param>
    /// <returns>The managed environment arguments.</returns>
    public static ManagedEnvironmentArgs CreateManagedEnvironmentArgs(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        MonitoringOutputs monitoring,
        NetworkOutputs network)
    {
        var managedEnvArgs = new ManagedEnvironmentArgs
        {
            ResourceGroupName = resourceGroup,
            Location = settings.Location,
            AppLogsConfiguration = new AppLogsConfigurationArgs
            {
                Destination = ServiceConstants.ContainerApps.LogAnalyticsDestination,
                LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                {
                    CustomerId = monitoring.LogAnalyticsWorkspaceId,
                    SharedKey = monitoring.LogAnalyticsWorkspacePrimaryKey
                }
            },
            Tags = ResourceTagHelper.GetStandardTags(settings.Environment, DeploymentConstants.ResourceTags.ContainerAppsEnvironmentType)
        };

        // Only add VNet configuration if ContainerAppsSubnet is configured
        if (!string.IsNullOrEmpty(settings.Network?.ContainerAppsSubnet))
        {
            managedEnvArgs.VnetConfiguration = new VnetConfigurationArgs
            {
                InfrastructureSubnetId = network.ContainerAppsSubnetId,
                Internal = settings.Network.IsInternalEnvironment
            };
        }

        return managedEnvArgs;
    }

    /// <summary>
    /// Creates ingress configuration for a container app.
    /// </summary>
    public static IngressArgs CreateIngressConfiguration(IngressSettings? ingressSettings, ILogger logger) =>
        ContainerAppIngressHelper.CreateIngressConfiguration(ingressSettings, logger);

    /// <summary>
    /// Builds environment variables for container apps.
    /// </summary>
    public static InputList<EnvironmentVarArgs> BuildEnvironmentVariables(
        InfrastructureSettings settings,
        CacheOutputs cache,
        EventHubsOutputs eventHubs,
        MonitoringOutputs monitoring,
        KeyVaultOutputs? keyVault,
        Input<string>? azureFrontDoorId,
        ILogger logger,
        IEnumerable<EnvironmentVarArgs>? additionalEnvironmentVariables = null) =>
        ContainerAppEnvVarHelper.BuildEnvironmentVariables(settings, cache, eventHubs, monitoring, keyVault, azureFrontDoorId, logger, additionalEnvironmentVariables);

    /// <summary>
    /// Creates scale rules for container apps based on settings.
    /// </summary>
    public static ScaleArgs CreateScaleRules(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.Container is null)
        {
            throw new ArgumentException("Container settings are required.", nameof(settings));
        }

        var containerSettings = settings.Container;
        var autoScaling = containerSettings.AutoScaling;

        var rules = new List<ScaleRuleArgs>();

        if (autoScaling.EnableHttpScaling)
        {
            rules.Add(new ScaleRuleArgs
            {
                Name = ServiceConstants.ContainerApps.HttpScalingRuleName,
                Http = new HttpScaleRuleArgs
                {
                    Metadata =
                    {
                        [ServiceConstants.AutoScaling.ConcurrentRequestsKey] = autoScaling.HttpRequestThreshold.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
            });
        }

        if (autoScaling.EnableCpuScaling)
        {
            rules.Add(new ScaleRuleArgs
            {
                Name = ServiceConstants.ContainerApps.CpuScalingRuleName,
                Custom = new CustomScaleRuleArgs
                {
                    Type = ServiceConstants.AutoScaling.CpuType,
                    Metadata =
                    {
                        [ServiceConstants.AutoScaling.TypeKey] = ServiceConstants.AutoScaling.UtilizationType,
                        [ServiceConstants.AutoScaling.ValueKey] = autoScaling.CpuThreshold.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
            });
        }

        if (autoScaling.EnableMemoryScaling)
        {
            rules.Add(new ScaleRuleArgs
            {
                Name = ServiceConstants.ContainerApps.MemoryScalingRuleName,
                Custom = new CustomScaleRuleArgs
                {
                    Type = ServiceConstants.AutoScaling.MemoryType,
                    Metadata =
                    {
                        [ServiceConstants.AutoScaling.TypeKey] = ServiceConstants.AutoScaling.UtilizationType,
                        [ServiceConstants.AutoScaling.ValueKey] = autoScaling.MemoryThreshold.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
            });
        }

        var scaleArgs = new ScaleArgs
        {
            MinReplicas = autoScaling.MinReplicas,
            MaxReplicas = autoScaling.MaxReplicas
        };

        if (rules.Count > 0)
        {
            scaleArgs.Rules = rules.ToArray();
        }

        return scaleArgs;
    }
}




