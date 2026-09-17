using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for managing Azure Blob Storage infrastructure
/// Provides blob storage containers, access policies, and lifecycle management for the Application application
/// </summary>
public interface IBlobStorageService : IInfrastructureService
{
    /// <summary>
    /// Creates and configures Azure Blob Storage infrastructure with containers and access policies
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including container configurations, access tiers, and lifecycle policies</param>
    /// <param name="resourceGroup">Azure resource group name where the blob storage will be deployed</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Blob storage infrastructure outputs containing connection strings, container details, and access configurations</returns>
    new Task<BlobStorageOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default);
}
