using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for managing Azure Table Storage infrastructure
/// Provides NoSQL key-value storage for structured data with high availability for the Application application
/// </summary>
public interface ITableStorageService : IInfrastructureService
{
    /// <summary>
    /// Creates and configures Azure Table Storage infrastructure with tables and access policies
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including table configurations, access policies, and performance settings</param>
    /// <param name="resourceGroup">Azure resource group name where the table storage will be deployed</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Table storage infrastructure outputs containing connection strings, table details, and access configurations</returns>
    new Task<TableStorageOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default);
}
