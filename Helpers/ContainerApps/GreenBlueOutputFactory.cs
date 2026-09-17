using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Helpers.ContainerApps;

/// <summary>
/// Factory for creating GreenBlueDeploymentOutputs.
/// </summary>
public static class GreenBlueOutputFactory
{
    /// <summary>
    /// Creates the GreenBlueDeploymentOutputs object.
    /// </summary>
    public static GreenBlueDeploymentOutputs CreateOutputs(
        InfrastructureSettings settings,
        ManagedEnvironment containerAppsEnvironment,
        SlotOutputs greenSlotOutputs,
        SlotOutputs blueSlotOutputs,
        ContainerApp mainIngress,
        string environmentName)
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

    /// <summary>
    /// Creates a fallback output object for when GreenBlueDeployment is disabled or partially configured.
    /// </summary>
    public static GreenBlueDeploymentOutputs CreateFallbackOutputs(InfrastructureSettings settings)
    {
        return new GreenBlueDeploymentOutputs
        {
            GreenSlot = new SlotOutputs { SlotName = GreenBlueConstants.GreenSlotName },
            BlueSlot = new SlotOutputs { SlotName = GreenBlueConstants.BlueSlotName },
            ActiveSlot = settings.GreenBlueDeployment?.ActiveSlot ?? throw new InvalidOperationException(),
            TargetSlot = settings.GreenBlueDeployment.TargetSlot,
            MainAppUrl = Output.Create(string.Empty),
            StagingUrl = Output.Create(string.Empty),
            EnvironmentName = settings.Environment,
            EnvironmentId = Output.Create(string.Empty),
            IsGreenBlueEnabled = settings.GreenBlueDeployment.Enabled,
            DeploymentStatus = DeploymentStatusType.RequiresDependencies.ToStringValue(),
            LastSlotSwitchTimestamp = null,
            TrafficDistribution = new Dictionary<string, int>
            {
                { DeploymentSlotType.Green.ToStringValue(), settings.GreenBlueDeployment.ActiveSlotTrafficPercentage },
                { DeploymentSlotType.Blue.ToStringValue(), settings.GreenBlueDeployment.TargetSlotTrafficPercentage }
            }
        };
    }

    private static Output<string> GetStagingUrl(InfrastructureSettings settings, SlotOutputs greenSlot, SlotOutputs blueSlot)
    {
        return settings.GreenBlueDeployment?.TargetSlot.ToLowerInvariant() switch
        {
            GreenBlueConstants.GreenSlotName => greenSlot.AppUrl,
            _ => blueSlot.AppUrl
        };
    }
}



