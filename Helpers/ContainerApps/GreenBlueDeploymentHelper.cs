using DeploymentKit.Constants;
using DeploymentKit.Extensions;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Helpers.ContainerApps;

/// <summary>
/// Helper for Green-Blue deployment operations.
/// </summary>
public static class GreenBlueDeploymentHelper
{
    /// <summary>
    /// Creates the Managed Environment for Container Apps.
    /// </summary>
    public static ManagedEnvironment CreateManagedEnvironment(
        string environmentName,
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        MonitoringOutputs monitoring)
    {
        return new ManagedEnvironment(environmentName, new ManagedEnvironmentArgs
        {
            ResourceGroupName = resourceGroup,
            Location = settings.Location,
            AppLogsConfiguration = new AppLogsConfigurationArgs
            {
                Destination = ContainerAppConstants.LogAnalyticsDestination,
                LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                {
                    CustomerId = monitoring.LogAnalyticsWorkspaceId,
                    SharedKey = monitoring.LogAnalyticsWorkspacePrimaryKey
                }
            },
            Tags = ResourceTagHelper.GetStandardTags(settings.Environment, ContainerAppConstants.ContainerAppsEnvironmentType)
        });
    }

    /// <summary>
    /// Constructs the output object for the Green-Blue deployment.
    /// </summary>
    public static GreenBlueDeploymentOutputs CreateOutputs(
        InfrastructureSettings settings,
        string environmentName,
        Output<string> environmentId,
        SlotOutputs greenSlot,
        SlotOutputs blueSlot,
        ContainerApp mainIngress)
    {
        return new GreenBlueDeploymentOutputs
        {
            GreenSlot = greenSlot,
            BlueSlot = blueSlot,
            ActiveSlot = settings.GreenBlueDeployment?.ActiveSlot ?? throw new InvalidOperationException(),
            TargetSlot = settings.GreenBlueDeployment.TargetSlot,
            MainAppUrl = mainIngress.Configuration.Apply(c => $"{ContainerAppConstants.HttpsScheme}{c?.Ingress?.Fqdn}"),
            StagingUrl = GetStagingUrl(settings, greenSlot, blueSlot),
            EnvironmentName = environmentName,
            EnvironmentId = environmentId,
            IsGreenBlueEnabled = settings.GreenBlueDeployment.Enabled,
            DeploymentStatus = Enums.DeploymentStatusType.Stable.ToStringValue(),
            TrafficDistribution = new Dictionary<string, int>
            {
                { Enums.DeploymentSlotType.Green.ToStringValue(), settings.GreenSlot!.TrafficPercentage },
                { Enums.DeploymentSlotType.Blue.ToStringValue(), settings.BlueSlot!.TrafficPercentage }
            }
        };
    }

    private static Output<string> GetStagingUrl(InfrastructureSettings settings, SlotOutputs greenSlot, SlotOutputs blueSlot) => settings.GreenBlueDeployment?.TargetSlot.ToLowerInvariant() switch
    {
        GreenBlueConstants.GreenSlotName => greenSlot.AppUrl,
        _ => blueSlot.AppUrl
    };
}



