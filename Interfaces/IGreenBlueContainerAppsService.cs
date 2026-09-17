using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for managing Green-Blue deployment strategy for Azure Container Apps
/// Provides zero-downtime deployments with traffic splitting and rollback capabilities
/// </summary>
public interface IGreenBlueContainerAppsService : IInfrastructureService
{
    /// <summary>
    /// Creates Green-Blue Container Apps deployment with traffic management and rollback capabilities
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including deployment strategy and traffic rules</param>
    /// <param name="resourceGroup">Azure resource group name where the container apps will be deployed</param>
    /// <param name="monitoring">Monitoring infrastructure outputs for deployment health tracking</param>
    /// <param name="containerRegistry">Container registry outputs for image versioning and deployment</param>
    /// <param name="database">Database infrastructure outputs for connection configuration</param>
    /// <param name="cache">Cache infrastructure outputs for Redis connection configuration</param>
    /// <param name="network">Network infrastructure outputs for VNet integration</param>
    /// <param name="eventHubs">Event Hubs infrastructure outputs for event streaming</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Green-Blue Container Apps infrastructure outputs with deployment status and traffic configuration</returns>
    Task<GreenBlueDeploymentOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, MonitoringOutputs monitoring, ContainerRegistryOutputs containerRegistry, DatabaseOutputs database, CacheOutputs cache, NetworkOutputs network, EventHubsOutputs eventHubs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates Green-Blue Container Apps deployment with traffic management and rollback capabilities, accepting tasks for pipeline parallelism
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including deployment strategy and traffic rules</param>
    /// <param name="resourceGroup">Azure resource group name where the container apps will be deployed</param>
    /// <param name="monitoringTask">Task returning Monitoring infrastructure outputs</param>
    /// <param name="containerRegistryTask">Task returning Container registry outputs</param>
    /// <param name="databaseTask">Task returning Database infrastructure outputs</param>
    /// <param name="cacheTask">Task returning Cache infrastructure outputs</param>
    /// <param name="networkTask">Task returning Network infrastructure outputs</param>
    /// <param name="eventHubsTask">Task returning Event Hubs infrastructure outputs</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Green-Blue Container Apps infrastructure outputs with deployment status and traffic configuration</returns>
    Task<GreenBlueDeploymentOutputs> CreateAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        Task<MonitoringOutputs> monitoringTask,
        Task<ContainerRegistryOutputs> containerRegistryTask,
        Task<DatabaseOutputs> databaseTask,
        Task<CacheOutputs> cacheTask,
        Task<NetworkOutputs> networkTask,
        Task<EventHubsOutputs> eventHubsTask,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches traffic between green and blue slots
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="resourceGroup">Resource group name</param>
    /// <param name="targetSlot">Target slot to switch to</param>
    /// <param name="trafficPercentage">Percentage of traffic to route to target slot</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Updated deployment outputs</returns>
    Task<GreenBlueDeploymentOutputs> SwitchTrafficAsync(InfrastructureSettings settings, Input<string> resourceGroup, string targetSlot, int trafficPercentage = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs health checks on a specific slot using resolved URLs.
    /// </summary>
    /// <param name="slotName">Slot name to check.</param>
    /// <param name="healthCheckUrl">Resolved health check URL.</param>
    /// <param name="appUrl">Resolved app URL used as fallback when health URL is missing.</param>
    /// <param name="healthCheckSettings">Health check configuration.</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation.</param>
    /// <returns>Health check result.</returns>
    Task<bool> PerformHealthCheckAsync(
        string slotName,
        string? healthCheckUrl,
        string? appUrl,
        GreenBlueDeploymentSettings healthCheckSettings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a specific slot with new container image
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="resourceGroup">Resource group name</param>
    /// <param name="slotName">Slot name to update</param>
    /// <param name="imageTag">New image tag</param>
    /// <param name="environmentVariables">Environment variables to update</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Updated slot outputs</returns>
    Task<SlotOutputs> UpdateSlotAsync(InfrastructureSettings settings, Input<string> resourceGroup, string slotName, string imageTag, Dictionary<string, string>? environmentVariables = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of both deployment slots using a resolved resource group name.
    /// </summary>
    /// <param name="settings">Infrastructure settings.</param>
    /// <param name="resourceGroupName">Resolved resource group name.</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation.</param>
    /// <returns>Current deployment status.</returns>
    Task<GreenBlueDeploymentOutputs> GetDeploymentStatusAsync(InfrastructureSettings settings, string resourceGroupName, CancellationToken cancellationToken = default);

}

