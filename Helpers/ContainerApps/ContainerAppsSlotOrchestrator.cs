using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Helpers.ContainerApps;

/// <summary>
/// Helper class for orchestrating the creation of container app slots
/// </summary>
public static class ContainerAppsSlotOrchestrator
{
    /// <summary>
    /// Creates the green and blue slots concurrently.
    /// </summary>
    public static async Task<(SlotOutputs GreenSlot, SlotOutputs BlueSlot)> CreateSlotsAsync(
        ISlotManagementService slotManagementService,
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        ManagedEnvironment containerAppsEnvironment,
        ContainerRegistryOutputs containerRegistry,
        DatabaseOutputs database,
        CacheOutputs cache,
        MonitoringOutputs monitoring,
        CancellationToken cancellationToken)
    {
        // Create green and blue slots
        // Slots are independent of each other (though dependent on the environment/DB/etc) and are provisioned concurrently using Task.WhenAll.
        cancellationToken.ThrowIfCancellationRequested();

        var greenSlotTask = slotManagementService.CreateSlotContainerAppAsync(DeploymentSlotType.Green.ToStringValue(), settings, resourceGroup, containerAppsEnvironment, containerRegistry, database, cache, monitoring, settings.GreenSlot ?? throw new InvalidOperationException("Green slot cannot be null"));
        var blueSlotTask = slotManagementService.CreateSlotContainerAppAsync(DeploymentSlotType.Blue.ToStringValue(), settings, resourceGroup, containerAppsEnvironment, containerRegistry, database, cache, monitoring, settings.BlueSlot ?? throw new InvalidOperationException("Blue slot cannot be null"));

        await Task.WhenAll(greenSlotTask, blueSlotTask);

        var greenSlotOutputs = await greenSlotTask;
        var blueSlotOutputs = await blueSlotTask;

        return (greenSlotOutputs, blueSlotOutputs);
    }
}



