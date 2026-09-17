using DeploymentKit.Constants;
using DeploymentKit.Interfaces;
using DeploymentKit.Settings;

namespace DeploymentKit.Helpers.Builder;

/// <summary>
/// Helper class for configuring Key Vault settings in InfrastructureBuilder.
/// </summary>
public static class KeyVaultConfigurationHelper
{
    /// <summary>
    /// Processes deferred Key Vault configuration synchronously.
    /// </summary>
    public static void ProcessDeferredConfigurationSync(
        string? envFilePath,
        IEnvFileParser envFileParser,
        KeyVaultSettings? keyVaultSettings,
        bool applyToContainerApps,
        IEnumerable<string>? excludePatterns,
        List<string> validationErrors,
        ILogger logger)
    {
        if (string.IsNullOrEmpty(envFilePath)) return;

        try
        {
            logger.LogInformation(BuilderConstants.Logs.ProcessingKeyVaultEnvSync, envFilePath);

            // Parse .env file
            var envVariables = envFileParser.Parse(envFilePath);
            ApplyKeyVaultSecrets(envVariables, keyVaultSettings, envFileParser, applyToContainerApps, excludePatterns, validationErrors, logger, envFilePath);
        }
        catch (FileNotFoundException)
        {
            // Re-throw FileNotFoundException to maintain backward compatibility
            throw;
        }
        catch (Exception ex)
        {
            var errorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture, BuilderConstants.ErrorMessages.EnvFileParseFailed, envFilePath, ex.Message);
            validationErrors.Add(errorMessage);
            logger.LogError(ex, errorMessage);
        }
    }

    /// <summary>
    /// Processes deferred Key Vault configuration asynchronously.
    /// </summary>
    public static async Task ProcessDeferredConfigurationAsync(
        string? envFilePath,
        IEnvFileParser envFileParser,
        KeyVaultSettings? keyVaultSettings,
        bool applyToContainerApps,
        IEnumerable<string>? excludePatterns,
        List<string> validationErrors,
        ILogger logger)
    {
        if (string.IsNullOrEmpty(envFilePath)) return;

        try
        {
            logger.LogInformation(BuilderConstants.Logs.ProcessingKeyVaultEnvAsync, envFilePath);

            // Parse .env file
            var envVariables = await envFileParser.ParseAsync(envFilePath);
            ApplyKeyVaultSecrets(envVariables, keyVaultSettings, envFileParser, applyToContainerApps, excludePatterns, validationErrors, logger, envFilePath);
        }
        catch (FileNotFoundException)
        {
            // Re-throw FileNotFoundException to maintain backward compatibility
            throw;
        }
        catch (Exception ex)
        {
            var errorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture, BuilderConstants.ErrorMessages.EnvFileParseFailed, envFilePath, ex.Message);
            validationErrors.Add(errorMessage);
            logger.LogError(ex, errorMessage);
        }
    }

    private static void ApplyKeyVaultSecrets(
        Dictionary<string, string> envVariables,
        KeyVaultSettings? keyVaultSettings,
        IEnvFileParser envFileParser,
        bool applyToContainerApps,
        IEnumerable<string>? excludePatterns,
        List<string> validationErrors,
        ILogger logger,
        string envFilePath)
    {
        if (keyVaultSettings == null) return;

        // Validate Key Vault secret names
        var validationErrorsList = envFileParser.ValidateKeyVaultSecretNames(envVariables);
        if (validationErrorsList.Count != 0)
        {
            var errorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture, BuilderConstants.ErrorMessages.InvalidKeyVaultSecretNames, string.Join(", ", validationErrorsList));
            validationErrors.Add(errorMessage);
            logger.LogError(errorMessage);
            return;
        }

        // Filter variables suitable for Key Vault
        var filteredVariables = envFileParser.FilterForKeyVault(envVariables, excludePatterns);

        // Set the ApplyToContainerApps flag
        keyVaultSettings.ApplyToContainerApps = applyToContainerApps;

        // Merge filtered .env variables with existing custom secrets (if any)
        foreach (var kvp in filteredVariables)
        {
            keyVaultSettings.Secrets[kvp.Key] = kvp.Value;
        }

        if (!filteredVariables.Any())
        {
            logger.LogWarning(BuilderConstants.Logs.NoSecretsFoundInEnv, envFilePath);
        }
        else
        {
            logger.LogInformation(BuilderConstants.Logs.KeyVaultSecretsAdded, filteredVariables.Count);
        }
    }
}


