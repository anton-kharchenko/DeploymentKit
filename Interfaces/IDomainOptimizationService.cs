using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Interface for managing domain optimization infrastructure including CDN, DNS, and Traffic Manager
/// </summary>
public interface IDomainOptimizationService : IInfrastructureService
{
    /// <summary>
    /// Creates the domain optimization infrastructure for maximum performance
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings</param>
    /// <param name="resourceGroup">Resource group name</param>
    /// <param name="applicationGateway">Application Gateway outputs</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Domain optimization infrastructure outputs</returns>
    Task<DomainOptimizationOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, ApplicationGatewayOutputs applicationGateway, CancellationToken cancellationToken = default);
}

