using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for managing PostgreSQL database infrastructure in Azure
/// Handles database server provisioning, configuration, and security settings
/// </summary>
public interface IDatabaseService : IInfrastructureService
{
    /// <summary>
    /// Creates and configures Azure Database for PostgreSQL infrastructure
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including database tier, storage, and backup settings</param>
    /// <param name="resourceGroup">Azure resource group name where the database will be deployed</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Database infrastructure outputs containing connection strings, server details, and security configurations</returns>
     Task<DatabaseOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, NetworkOutputs? network = null, CancellationToken cancellationToken = default);
}

