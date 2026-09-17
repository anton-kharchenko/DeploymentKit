using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Interface for managing network infrastructure including VNets, subnets, and private endpoints
/// </summary>
public interface INetworkService : IInfrastructureService
{
    /// <summary>
    /// Creates the network infrastructure including VNet, subnets, and private endpoints
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings</param>
    /// <param name="resourceGroup">Resource group name</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Network infrastructure outputs</returns>
    new Task<NetworkOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default);
}

