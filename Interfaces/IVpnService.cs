using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Interface for managing VPN Gateway infrastructure
/// </summary>
public interface IVpnService
{
    /// <summary>
    /// Creates VPN Gateway infrastructure including Point-to-Site configuration
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="resourceGroup">Resource group name</param>
    /// <param name="networkOutputs">Network infrastructure outputs</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>VPN infrastructure outputs</returns>
    Task<VpnOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, NetworkOutputs networkOutputs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates VPN client configuration for secure connectivity
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="vpnOutputs">VPN infrastructure outputs</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>VPN client configuration</returns>
    Task<string> GenerateClientConfigurationAsync(InfrastructureSettings settings, VpnOutputs vpnOutputs, CancellationToken cancellationToken = default);
}

