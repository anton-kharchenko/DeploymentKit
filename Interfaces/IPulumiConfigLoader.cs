using DeploymentKit.Models;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

public interface IPulumiConfigLoader
{
    Task<InfrastructureConfigurationSourceResult> LoadConfigurationAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default);

    IDictionary<string, string?> CreateEnvironmentOverrides(
        InfrastructureConfigurationSourceResult configuration,
        IDictionary<string, string[]>? mappings);

    /// <summary>
    /// Loads all secrets for Key Vault from environment variables.
    /// </summary>
    /// <param name="keyVaultSecretMappings">Optional custom mappings (keyVaultKey -> envVarNames[])</param>
    /// <param name="customPrefixes">Optional custom prefixes for auto-discovery</param>
    Dictionary<string, string> LoadAllSecretsForKeyVault(
        Dictionary<string, string[]>? keyVaultSecretMappings = null,
        string[]? customPrefixes = null);
}

