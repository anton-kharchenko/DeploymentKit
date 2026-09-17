using DeploymentKit.Constants;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using System.Text.RegularExpressions;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Helpers.ContainerApps;

/// <summary>
/// Helper class for managing container app secrets and environment variables.
/// </summary>
public static class ContainerAppSecretHelper
{
    /// <summary>
    /// Builds the list of secrets for a container app, including Key Vault references if configured.
    /// </summary>
    /// <param name="settings">The infrastructure settings.</param>
    /// <param name="containerRegistry">The container registry outputs.</param>
    /// <param name="database">The database outputs.</param>
    /// <param name="keyVault">The Key Vault outputs.</param>
    /// <param name="logger">The logger instance.</param>
    /// <returns>A tuple containing the list of secret arguments and a list of secret names.</returns>
    public static (InputList<SecretArgs> secrets, List<string> secretNames) BuildSecretsListWithKeyVault(
        InfrastructureSettings settings,
        ContainerRegistryOutputs containerRegistry,
        DatabaseOutputs database,
        KeyVaultOutputs? keyVault,
        ILogger logger)
    {
        var useKeyVaultSecrets = ContainerAppIdentityHelper.ShouldUseKeyVaultSecrets(settings, keyVault);
        var DbSecretName = ToKeyVaultSecretName(ServiceConstants.EnvironmentVariables.PostgresConnectionString);
        var hasKeyVaultDb = useKeyVaultSecrets && settings.KeyVault?.Secrets.Keys.Any(key =>
            string.Equals(key, DbSecretName, StringComparison.OrdinalIgnoreCase)) == true;

        // Build secrets as a List first, then convert to InputList to ensure proper materialization
        var secretsList = new List<SecretArgs>();
        var secretNames = new List<string>();

        // Add standard secrets based on placeholder mode
        if (settings.Container is { UsePlaceholderImages: true })
        {
            secretsList.Add(new SecretArgs
            {
                Name = ServiceConstants.ContainerApps.DbPasswordSecretRef,
                Value = Output.CreateSecret(settings.Database?.Password ?? throw new InvalidOperationException("Database password is required"))
            });
            secretNames.Add(ServiceConstants.ContainerApps.DbPasswordSecretRef);

            if (!hasKeyVaultDb)
            {
                secretsList.Add(new SecretArgs
                {
                    Name = ServiceConstants.ContainerApps.PostgresConnectionStringSecretRef,
                    Value = database.ConnectionString
                });
                secretNames.Add(ServiceConstants.ContainerApps.PostgresConnectionStringSecretRef);
            }
        }
        else
        {
            secretsList.Add(new SecretArgs
            {
                Name = ServiceConstants.ContainerApps.AcrPasswordSecretRef,
                Value = containerRegistry.Password
            });
            secretNames.Add(ServiceConstants.ContainerApps.AcrPasswordSecretRef);

            secretsList.Add(new SecretArgs
            {
                Name = ServiceConstants.ContainerApps.DbPasswordSecretRef,
                Value = Output.CreateSecret(settings.Database?.Password ?? throw new InvalidOperationException("Database password is required"))
            });
            secretNames.Add(ServiceConstants.ContainerApps.DbPasswordSecretRef);

            if (!hasKeyVaultDb)
            {
                secretsList.Add(new SecretArgs
                {
                    Name = ServiceConstants.ContainerApps.PostgresConnectionStringSecretRef,
                    Value = database.ConnectionString
                });
                secretNames.Add(ServiceConstants.ContainerApps.PostgresConnectionStringSecretRef);
            }
        }

        // Add Key Vault secrets to Container App secrets if ApplyToContainerApps is enabled
        if (useKeyVaultSecrets)
        {
            if (settings.KeyVault != null)
            {
                logger.LogInformation("Adding {SecretCount} Key Vault secrets to Container App secrets store", settings.KeyVault.Secrets.Count);

                foreach (var secret in settings.KeyVault.Secrets)
                {
                    // Sanitize secret name for Container App (lowercase, alphanumeric and hyphens only)
                    var secretName = secret.Key.ToLowerInvariant();

                    secretsList.Add(new SecretArgs
                    {
                        Name = secretName,
                        KeyVaultUrl = Output.Format($"{keyVault!.VaultUri}secrets/{secret.Key}"),
                        Identity = "System"
                    });
                    secretNames.Add(secretName);

                    logger.LogDebug("Added Container App secret {SecretName} from Key Vault", secretName);
                }
            }
        }

        logger.LogInformation("Total secrets configured for Container App: {Count}", secretsList.Count);

        // Return array directly - Pulumi has implicit conversion from T[] to InputList<T>
        return (secretsList.ToArray(), secretNames);
    }

    /// <summary>
    /// Builds the list of environment variables for a container app.
    /// </summary>
    /// <param name="settings">The infrastructure settings.</param>
    /// <param name="cache">The cache outputs.</param>
    /// <param name="eventHubs">The Event Hubs outputs.</param>
    /// <param name="monitoring">The monitoring outputs.</param>
    /// <param name="keyVault">The Key Vault outputs.</param>
    /// <param name="azureFrontDoorId">The Azure Front Door ID.</param>
    /// <param name="logger">The logger instance.</param>
    /// <returns>A list of environment variable arguments.</returns>
    public static InputList<EnvironmentVarArgs> BuildEnvironmentVariables(
        InfrastructureSettings settings,
        CacheOutputs cache,
        EventHubsOutputs eventHubs,
        MonitoringOutputs monitoring,
        KeyVaultOutputs? keyVault,
        Input<string>? azureFrontDoorId,
        ILogger logger)
    {
        var useKeyVaultSecrets = ContainerAppIdentityHelper.ShouldUseKeyVaultSecrets(settings, keyVault);
        var DbSecretName = ToKeyVaultSecretName(ServiceConstants.EnvironmentVariables.PostgresConnectionString);
        var hasKeyVaultDb = useKeyVaultSecrets && settings.KeyVault?.Secrets.Keys.Any(key =>
            string.Equals(key, DbSecretName, StringComparison.OrdinalIgnoreCase)) == true;

        // Build environment variables as a List first, then convert to InputList to ensure proper materialization
        var envVarsList = new List<EnvironmentVarArgs>
        {
            new() { Name = ServiceConstants.EnvironmentVariables.AspNetCoreUrls, Value = ServiceConstants.ContainerDefaults.AspNetCoreUrlsValue },
            new() { Name = ServiceConstants.EnvironmentVariables.AspNetCoreEnvironment, Value = settings.Environment },
            hasKeyVaultDb
                ? new EnvironmentVarArgs
                {
                    Name = ServiceConstants.EnvironmentVariables.PostgresConnectionString,
                    SecretRef = DbSecretName.ToLowerInvariant()
                }
                : new EnvironmentVarArgs
                {
                    Name = ServiceConstants.EnvironmentVariables.PostgresConnectionString,
                    SecretRef = ServiceConstants.ContainerApps.PostgresConnectionStringSecretRef
                },
            new() { Name = ServiceConstants.EnvironmentVariables.RedisConnectionString, Value = cache.ConnectionString },
            new() { Name = ServiceConstants.EnvironmentVariables.EventHubsConnectionString, Value = eventHubs.EventHubsConnectionString },
            new() { Name = ServiceConstants.EnvironmentVariables.ApplicationInsightsConnectionString, Value = monitoring.ApplicationInsightsConnectionString },
            new() { Name = ServiceConstants.EnvironmentVariables.OtelExporterEndpoint, Value = ServiceConstants.ContainerDefaults.OtelExporterEndpointValue }
        };
        var envVarNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ServiceConstants.EnvironmentVariables.AspNetCoreUrls,
            ServiceConstants.EnvironmentVariables.AspNetCoreEnvironment,
            ServiceConstants.EnvironmentVariables.PostgresConnectionString,
            ServiceConstants.EnvironmentVariables.RedisConnectionString,
            ServiceConstants.EnvironmentVariables.EventHubsConnectionString,
            ServiceConstants.EnvironmentVariables.ApplicationInsightsConnectionString,
            ServiceConstants.EnvironmentVariables.OtelExporterEndpoint
        };

        if (azureFrontDoorId != null)
        {
            envVarNames.Add(ServiceConstants.EnvironmentVariables.AzureFrontDoorId);
            envVarsList.Add(new EnvironmentVarArgs
            {
                Name = ServiceConstants.EnvironmentVariables.AzureFrontDoorId,
                Value = azureFrontDoorId
            });
        }

        // If ApplyToContainerApps is enabled and KeyVault secrets exist, add them as environment variables
        if (useKeyVaultSecrets)
        {
            if (settings.KeyVault != null)
            {
                logger.LogInformation("Adding {SecretCount} Key Vault secrets as environment variables to Container Apps", settings.KeyVault.Secrets.Count);

                foreach (var secret in settings.KeyVault.Secrets)
                {
                    // Convert Key Vault secret name format (hyphens) to environment variable format (underscores)
                    var envVarName = secret.Key.Replace("-", "_").ToUpperInvariant();

                    // Secret name in Container App secrets store (lowercase)
                    var secretRef = secret.Key.ToLowerInvariant();

                    // Add as environment variable with secret reference (not plain text value)
                    if (!envVarNames.Add(envVarName))
                    {
                        continue;
                    }

                    envVarsList.Add(new EnvironmentVarArgs { Name = envVarName, SecretRef = secretRef });

                    logger.LogDebug(
                        "Added environment variable {EnvVarName} referencing Container App secret {SecretRef}",
                        envVarName, secretRef);
                }
            }
        }

        logger.LogInformation("Total environment variables configured for Container App: {Count}", envVarsList.Count);

        // Return array directly - Pulumi has implicit conversion from T[] to InputList<T>
        return envVarsList.ToArray();
    }

    private static string ToKeyVaultSecretName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var transformed = name.Replace('_', '-');
        transformed = Regex.Replace(transformed, @"[^a-zA-Z0-9-]", string.Empty);
        return transformed.Trim('-');
    }
}



