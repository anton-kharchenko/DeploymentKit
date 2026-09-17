using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for managing Azure Event Hubs infrastructure
/// Provides high-throughput event streaming and messaging capabilities for real-time data processing
/// </summary>
public interface IEventHubsService : IInfrastructureService
{
    /// <summary>
    /// Creates and configures Azure Event Hubs infrastructure
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including throughput units and partition count</param>
    /// <param name="resourceGroup">Azure resource group name where the Event Hubs will be deployed</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Event Hubs infrastructure outputs containing connection strings and namespace details</returns>
    new Task<EventHubsOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Event Hubs infrastructure outputs after successful creation and configuration
    /// Contains connection strings, namespace details, and event hub configurations for application integration
    /// </summary>
    EventHubsOutputs Outputs { get; }
}

