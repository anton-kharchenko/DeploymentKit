using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for managing Redis Cache infrastructure in Azure
/// Supports both Azure Managed Redis (default, recommended) and Azure Cache for Redis (legacy)
/// Provides high-performance caching capabilities for the Application application
/// </summary>
public interface ICacheService : IInfrastructureService
{
    /// <summary>
    /// Creates and configures Azure Redis Cache infrastructure
    /// Automatically selects between Azure Managed Redis or Azure Cache for Redis based on settings.Cache.UseAzureManagedRedis
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including cache tier and capacity</param>
    /// <param name="resourceGroup">Azure resource group name where the cache will be deployed</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Cache infrastructure outputs containing connection strings and endpoints</returns>
    new Task<CacheOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default);
}

