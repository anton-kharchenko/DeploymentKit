using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for managing Azure Cosmos DB infrastructure
/// Provides NoSQL database capabilities with global distribution and multi-model support for the Application application
/// </summary>
public interface ICosmosDbService : IInfrastructureService
{
    /// <summary>
    /// Creates and configures Azure Cosmos DB infrastructure with databases, containers, and throughput settings
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including consistency levels, throughput, and geo-replication settings</param>
    /// <param name="resourceGroup">Azure resource group name where the Cosmos DB will be deployed</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Cosmos DB infrastructure outputs containing connection strings, endpoint details, and database configurations</returns>
    new Task<CosmosDbOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default);
}
