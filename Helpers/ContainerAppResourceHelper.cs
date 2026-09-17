using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Helpers;

/// <summary>
/// Helper class for managing Container Apps resources
/// </summary>
public static class ContainerAppResourceHelper
{
    /// <summary>
    /// Creates a Managed Environment for Container Apps
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
    /// Constructs the GreenBlueDeploymentOutputs object
    /// </summary>
    public static GreenBlueDeploymentOutputs CreateDeploymentOutputs(
        InfrastructureSettings settings,
        string environmentName,
        ManagedEnvironment containerAppsEnvironment,
        SlotOutputs greenSlotOutputs,
        SlotOutputs blueSlotOutputs,
        ContainerApp mainIngress)
    {
        return new GreenBlueDeploymentOutputs
        {
            GreenSlot = greenSlotOutputs,
            BlueSlot = blueSlotOutputs,
            ActiveSlot = settings.GreenBlueDeployment?.ActiveSlot ?? throw new InvalidOperationException(),
            TargetSlot = settings.GreenBlueDeployment.TargetSlot,
            MainAppUrl = mainIngress.Configuration.Apply(c => $"{ContainerAppConstants.HttpsScheme}{c?.Ingress?.Fqdn}"),
            StagingUrl = GetStagingUrl(settings, greenSlotOutputs, blueSlotOutputs),
            EnvironmentName = environmentName,
            EnvironmentId = containerAppsEnvironment.Id,
            IsGreenBlueEnabled = settings.GreenBlueDeployment.Enabled,
            DeploymentStatus = DeploymentStatusType.Stable.ToStringValue(),
            TrafficDistribution = new Dictionary<string, int>
            {
                { DeploymentSlotType.Green.ToStringValue(), settings.GreenSlot!.TrafficPercentage },
                { DeploymentSlotType.Blue.ToStringValue(), settings.BlueSlot!.TrafficPercentage }
            }
        };
    }

    private static Output<string> GetStagingUrl(InfrastructureSettings settings, SlotOutputs greenSlot, SlotOutputs blueSlot) => settings.GreenBlueDeployment?.TargetSlot.ToLowerInvariant() switch
    {
        GreenBlueConstants.GreenSlotName => greenSlot.AppUrl,
        _ => blueSlot.AppUrl
    };
}



