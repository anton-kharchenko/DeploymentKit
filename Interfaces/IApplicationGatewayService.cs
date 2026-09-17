using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Interface for managing Application Gateway infrastructure
/// </summary>
public interface IApplicationGatewayService : IInfrastructureService
{
    /// <summary>
    /// Creates the Application Gateway infrastructure for secure external access
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings</param>
    /// <param name="resourceGroup">Resource group name</param>
    /// <param name="network">Network infrastructure outputs</param>
    /// <param name="containerApps">Container Apps outputs</param>
    /// <param name="certificate">Optional SSL certificate outputs for HTTPS</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Application Gateway infrastructure outputs</returns>
    Task<ApplicationGatewayOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, NetworkOutputs network, ContainerAppsOutputs containerApps, CertificateOutputs? certificate = null, CancellationToken cancellationToken = default);
}

