using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for managing Azure Key Vault infrastructure
/// Provides secure storage and management of application secrets, certificates, and keys
/// </summary>
public interface IKeyVaultService : IInfrastructureService
{
    /// <summary>
    /// Creates and configures Azure Key Vault infrastructure with appropriate access policies
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including access policies and security configurations</param>
    /// <param name="resourceGroup">Azure resource group name where the Key Vault will be deployed</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Key Vault infrastructure outputs containing vault URI, access policies, and security configurations</returns>
    new Task<KeyVaultOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default);
}

