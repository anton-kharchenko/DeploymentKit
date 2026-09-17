using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Base interface for all infrastructure services providing common contract for resource creation
/// </summary>
public interface IInfrastructureService
{
    /// <summary>
    /// Creates infrastructure resources asynchronously
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings</param>
    /// <param name="resourceGroup">Target resource group</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Infrastructure outputs</returns>
    Task<object> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default);

    Task<object> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup);
}

