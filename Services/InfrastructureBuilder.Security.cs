using DeploymentKit.Constants;
using DeploymentKit.Extensions;
using DeploymentKit.Helpers;
using DeploymentKit.Helpers.Builder;
using DeploymentKit.Interfaces;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Partial class for InfrastructureBuilder containing security-related resource configuration methods.
/// </summary>
public partial class InfrastructureBuilder
{
    private bool _addKeyVault;
    private KeyVaultSettings? _keyVaultSettings;
    private string? _keyVaultEnvFilePath;
    private IEnumerable<string>? _keyVaultExcludePatterns;
    private bool _keyVaultApplyToContainerApps;

    /// <summary>
    /// Adds Azure Key Vault to the infrastructure.
    /// </summary>
    /// <param name="envFilePath">Path to the .env file containing secrets.</param>
    /// <param name="excludePatterns">List of patterns to exclude from secrets.</param>
    /// <param name="customSettings">Custom Key Vault settings.</param>
    /// <param name="applyToContainerApps">Whether to apply secrets to container apps.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddKeyVault(string envFilePath, IEnumerable<string>? excludePatterns = null, KeyVaultSettings? customSettings = null, bool applyToContainerApps = false)
    {
        if (string.IsNullOrWhiteSpace(envFilePath))
        {
            throw new ArgumentException(BuilderConstants.ErrorMessages.EnvFilePathRequired, nameof(envFilePath));
        }

        _logger.LogInformation(BuilderConstants.Logs.ConfiguringKeyVault, envFilePath);

        _addKeyVault = true;
        _keyVaultEnvFilePath = envFilePath;
        _keyVaultExcludePatterns = excludePatterns;
        _keyVaultSettings = customSettings ?? InfrastructureDefaultSettingsFactory.GetDefaultKeyVaultSettings();
        _keyVaultApplyToContainerApps = applyToContainerApps;

        // Ensure SkuNameString is populated from enum if it's empty (for custom settings)
        if (string.IsNullOrWhiteSpace(_keyVaultSettings.SkuNameString))
        {
            _keyVaultSettings.SkuNameString = _keyVaultSettings.SkuName.ToStringValue();
        }

        return this;
    }

    private void ProcessDeferredConfigurationSync()
    {
        KeyVaultConfigurationHelper.ProcessDeferredConfigurationSync(
            _keyVaultEnvFilePath,
            _envFileParser,
            _keyVaultSettings,
            _keyVaultApplyToContainerApps,
            _keyVaultExcludePatterns,
            _validationErrors,
            _logger);
    }

    private Task ProcessDeferredConfigurationAsync()
    {
        return KeyVaultConfigurationHelper.ProcessDeferredConfigurationAsync(
            _keyVaultEnvFilePath,
            _envFileParser,
            _keyVaultSettings,
            _keyVaultApplyToContainerApps,
            _keyVaultExcludePatterns,
            _validationErrors,
            _logger);
    }

    /// <summary>
    /// Adds Azure Key Vault with custom settings.
    /// </summary>
    /// <param name="keyVaultSettings">Custom Key Vault settings.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddKeyVault(KeyVaultSettings keyVaultSettings)
    {
        _addKeyVault = true;
        _keyVaultSettings = keyVaultSettings ?? throw new ArgumentNullException(nameof(keyVaultSettings));

        // Ensure SkuNameString is populated from enum if it's empty
        if (string.IsNullOrWhiteSpace(_keyVaultSettings.SkuNameString))
        {
            _keyVaultSettings.SkuNameString = _keyVaultSettings.SkuName.ToStringValue();
        }

        _logger.LogInformation(BuilderConstants.Logs.KeyVaultAddedCustom);
        return this;
    }

    /// <summary>
    /// Adds Azure Key Vault with default settings.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddKeyVault()
    {
        _addKeyVault = true;
        _keyVaultSettings = InfrastructureDefaultSettingsFactory.GetDefaultKeyVaultSettings();
        _logger.LogInformation(BuilderConstants.Logs.KeyVaultAddedDefault);
        return this;
    }

    private void ApplyKeyVaultSettingsSync() => ProcessDeferredConfigurationSync();

    private Task ApplyKeyVaultSettingsAsync() => ProcessDeferredConfigurationAsync();
}

