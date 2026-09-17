using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service for managing container app slots.
/// </summary>
public interface ISlotManagementService
{
    /// <summary>
    /// Creates a slot container app.
    /// </summary>
    Task<SlotOutputs> CreateSlotContainerAppAsync(
        string slotName,
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        ManagedEnvironment environment,
        ContainerRegistryOutputs containerRegistry,
        DatabaseOutputs database,
        CacheOutputs cache,
        MonitoringOutputs monitoring,
        SlotSettings slotSettings);

    /// <summary>
    /// Updates a slot container app.
    /// </summary>
    Task<SlotOutputs> UpdateSlotAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        string slotName,
        string imageTag,
        Dictionary<string, string>? environmentVariables = null,
        CancellationToken cancellationToken = default);
}



