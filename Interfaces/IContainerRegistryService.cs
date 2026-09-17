using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for managing Azure Container Registry infrastructure
/// Provides secure container image storage and management for the Application application deployments
/// </summary>
public interface IContainerRegistryService : IInfrastructureService
{
    /// <summary>
    /// Creates and configures Azure Container Registry infrastructure with appropriate access policies and security settings
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including registry tier, admin access, and security configurations</param>
    /// <param name="resourceGroup">Azure resource group name where the container registry will be deployed</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Container registry infrastructure outputs containing login server, credentials, and access details</returns>
    new Task<ContainerRegistryOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default);
}

