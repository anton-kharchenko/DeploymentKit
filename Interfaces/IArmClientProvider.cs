using Azure.ResourceManager;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Interface for providing Azure Resource Manager (ARM) client instances
/// Abstracts ARM client creation to enable proper dependency injection and testing
/// </summary>
public interface IArmClientProvider
{
    /// <summary>
    /// Gets an Azure Resource Manager client instance
    /// </summary>
    /// <returns>An ArmClient instance configured with appropriate credentials</returns>
    ArmClient GetArmClient();

    /// <summary>
    /// Gets an Azure Resource Manager client instance asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>An ArmClient instance configured with appropriate credentials</returns>
    Task<ArmClient> GetArmClientAsync(CancellationToken cancellationToken = default);
}

