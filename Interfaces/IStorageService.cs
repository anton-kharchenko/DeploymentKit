using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for managing Azure Storage Account infrastructure
/// Provides blob storage, file shares, and other storage services for the Application application
/// </summary>
public interface IStorageService : IInfrastructureService
{
    /// <summary>
    /// Creates and configures Azure Storage Account infrastructure with appropriate containers and access policies
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including storage tier, replication, and security settings</param>
    /// <param name="resourceGroup">Azure resource group name where the storage account will be deployed</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Storage infrastructure outputs containing connection strings, endpoints, and container details</returns>
    new Task<StorageOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default);
}

