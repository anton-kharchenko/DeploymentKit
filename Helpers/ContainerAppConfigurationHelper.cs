using DeploymentKit.Constants;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ContainerAppManagedServiceIdentityArgs = Pulumi.AzureNative.App.Inputs.ManagedServiceIdentityArgs;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Helpers;

/// <summary>
/// Helper class for configuring Container Apps secrets and environment variables.
/// </summary>
public static class ContainerAppConfigurationHelper
{
    private const string KeyVaultSecretsUserRoleId = ServiceConstants.KeyVault.SecretsUserRoleId;

    /// <summary>
    /// Builds Container App secrets list including Key Vault secrets if ApplyToContainerApps is enabled
    /// </summary>
    public static (InputList<SecretArgs> secrets, List<string> secretNames) BuildSecretsListWithKeyVault(
        InfrastructureSettings settings,
        ContainerRegistryOutputs containerRegistry,
        DatabaseOutputs database,
        KeyVaultOutputs? keyVault,
        ILogger logger)
    {
        var useKeyVaultSecrets = ShouldUseKeyVaultSecrets(settings, keyVault);
        var dbSecretName = ToKeyVaultSecretName(ServiceConstants.EnvironmentVariables.PostgresConnectionString);
        var hasKeyVaultDb = useKeyVaultSecrets && settings.KeyVault?.Secrets.Keys.Any(key =>
            string.Equals(key, dbSecretName, StringComparison.OrdinalIgnoreCase)) == true;

        // Build secrets as a List first, then convert to InputList to ensure proper materialization
        var secretsList = new List<SecretArgs>();
        var secretNames = new List<string>();

        // Add standard secrets based on placeholder mode
        if (settings.Container is { UsePlaceholderImages: true })
        {
            secretsList.Add(new SecretArgs
            {
                Name = ServiceConstants.ContainerApps.DbPasswordSecretRef,
                Value = Output.CreateSecret(settings.Database?.Password ?? throw new InvalidOperationException(ValidationConstants.ContainerApps.DatabasePasswordRequired))
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
                Value = Output.CreateSecret(settings.Database?.Password ?? throw new InvalidOperationException(ValidationConstants.ContainerApps.DatabasePasswordRequired))
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

        logger.LogInformation("Total secrets configured for Container App: {Count}", secretsList.Count);

        // Return array directly - Pulumi has implicit conversion from T[] to InputList<T>
        return (secretsList.ToArray(), secretNames);
    }

    /// <summary>
    /// Builds environment variables list including Key Vault secrets if ApplyToContainerApps is enabled
    /// </summary>
    public static InputList<EnvironmentVarArgs> BuildEnvironmentVariables(
        InfrastructureSettings settings,
        CacheOutputs cache,
        EventHubsOutputs eventHubs,
        MonitoringOutputs monitoring,
        KeyVaultOutputs? keyVault,
        Input<string>? azureFrontDoorId,
        ILogger logger)
    {
        var useKeyVaultSecrets = ShouldUseKeyVaultSecrets(settings, keyVault);
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

                envVarsList.Add(new EnvironmentVarArgs
                {
                    Name = envVarName,
                    SecretRef = secretRef
                });

                logger.LogDebug("Added environment variable {EnvVarName} referencing Container App secret {SecretRef}", envVarName, secretRef);
            }
        }

        logger.LogInformation("Total environment variables configured for Container App: {Count}", envVarsList.Count);

        // Return array directly - Pulumi has implicit conversion from T[] to InputList<T>
        return envVarsList.ToArray();
    }

    /// <summary>
    /// Builds environment variables list including Key Vault secrets if ApplyToContainerApps is enabled (Overload for Slot settings)
    /// </summary>
    public static InputList<EnvironmentVarArgs> BuildEnvironmentVariables(
        InfrastructureSettings settings,
        CacheOutputs cache,
        MonitoringOutputs monitoring,
        KeyVaultOutputs? keyVault,
        SlotSettings slotSettings,
        ILogger logger)
    {
        var useKeyVaultSecrets = ShouldUseKeyVaultSecrets(settings, keyVault);
        var DbSecretName = ToKeyVaultSecretName(ServiceConstants.EnvironmentVariables.PostgresConnectionString);
        var hasKeyVaultDb = useKeyVaultSecrets && settings.KeyVault?.Secrets.Keys.Any(key =>
            string.Equals(key, DbSecretName, StringComparison.OrdinalIgnoreCase)) == true;

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
            new() { Name = ServiceConstants.EnvironmentVariables.ApplicationInsightsConnectionString, Value = monitoring.ApplicationInsightsConnectionString },
            new() { Name = ServiceConstants.EnvironmentVariables.OtelExporterEndpoint, Value = ServiceConstants.ContainerDefaults.OtelExporterEndpointValue },
            // Slot specific variables
            new() { Name = "DEPLOYMENT_SLOT", Value = slotSettings.SlotName },
            new() { Name = "DEPLOYMENT_VERSION", Value = slotSettings.VersionString }
        };

        var envVarNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ServiceConstants.EnvironmentVariables.AspNetCoreUrls,
            ServiceConstants.EnvironmentVariables.AspNetCoreEnvironment,
            ServiceConstants.EnvironmentVariables.PostgresConnectionString,
            ServiceConstants.EnvironmentVariables.RedisConnectionString,
            ServiceConstants.EnvironmentVariables.ApplicationInsightsConnectionString,
            ServiceConstants.EnvironmentVariables.OtelExporterEndpoint,
            "DEPLOYMENT_SLOT",
            "DEPLOYMENT_VERSION"
        };

        // Add custom environment variables from slot settings
        if (slotSettings.EnvironmentVariables != null)
        {
            foreach (var envVar in slotSettings.EnvironmentVariables)
            {
                if (envVarNames.Add(envVar.Key))
                {
                    envVarsList.Add(new EnvironmentVarArgs { Name = envVar.Key, Value = envVar.Value });
                }
            }
        }

        // If ApplyToContainerApps is enabled and KeyVault secrets exist, add them as environment variables
        if (useKeyVaultSecrets)
        {
            logger.LogInformation("Adding {SecretCount} Key Vault secrets as environment variables to Container Apps", settings.KeyVault.Secrets.Count);

            foreach (var secret in settings.KeyVault.Secrets)
            {
                var envVarName = secret.Key.Replace("-", "_").ToUpperInvariant();
                var secretRef = secret.Key.ToLowerInvariant();

                if (!envVarNames.Add(envVarName))
                {
                    continue;
                }

                envVarsList.Add(new EnvironmentVarArgs
                {
                    Name = envVarName,
                    SecretRef = secretRef
                });

                logger.LogDebug("Added environment variable {EnvVarName} referencing Container App secret {SecretRef}", envVarName, secretRef);
            }
        }

        return envVarsList.ToArray();
    }

    /// <summary>
    /// Determines if Key Vault secrets should be used for Container Apps
    /// </summary>
    public static bool ShouldUseKeyVaultSecrets(InfrastructureSettings settings, KeyVaultOutputs? keyVault) =>
        settings.KeyVault?.ApplyToContainerApps == true &&
        keyVault != null &&
        settings.KeyVault.Secrets?.Count > 0;

    /// <summary>
    /// Gets the managed identity configuration for Container Apps based on Key Vault usage
    /// </summary>
    public static ContainerAppManagedServiceIdentityArgs? GetContainerAppIdentity(InfrastructureSettings settings, KeyVaultOutputs? keyVault)
    {
        if (!ShouldUseKeyVaultSecrets(settings, keyVault))
        {
            return null;
        }

        return new ContainerAppManagedServiceIdentityArgs
        {
            Type = ManagedServiceIdentityType.SystemAssigned
        };
    }

    /// <summary>
    /// Configures RBAC role assignments for Container Apps to access Key Vault secrets
    /// </summary>
    public static void ConfigureKeyVaultAccessForContainerApps(
        InfrastructureSettings settings,
        KeyVaultOutputs? keyVault,
        ContainerApp apiApp,
        ContainerApp jobsApp,
        string keyVaultName)
    {
        if (!ShouldUseKeyVaultSecrets(settings, keyVault) || settings.KeyVault?.EnableRbacAuthorization != true)
        {
            return;
        }

        var roleDefinitionId = $"/subscriptions/{settings.SubscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{KeyVaultSecretsUserRoleId}";

        CreateKeyVaultRoleAssignment(
            CreateDeterministicGuid($"{keyVaultName}-{settings.Environment}-api-{KeyVaultSecretsUserRoleId}"),
            apiApp,
            keyVault!,
            roleDefinitionId);

        CreateKeyVaultRoleAssignment(
            CreateDeterministicGuid($"{keyVaultName}-{settings.Environment}-jobs-{KeyVaultSecretsUserRoleId}"),
            jobsApp,
            keyVault!,
            roleDefinitionId);
    }

    /// <summary>
    /// Configures RBAC role assignments for a single Container App to access Key Vault secrets
    /// </summary>
    public static void ConfigureKeyVaultAccess(
        InfrastructureSettings settings,
        KeyVaultOutputs? keyVault,
        ContainerApp app,
        string keyVaultName,
        string suffix)
    {
        if (!ShouldUseKeyVaultSecrets(settings, keyVault) || settings.KeyVault?.EnableRbacAuthorization != true)
        {
            return;
        }

        var roleDefinitionId = $"/subscriptions/{settings.SubscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{KeyVaultSecretsUserRoleId}";

        CreateKeyVaultRoleAssignment(
            CreateDeterministicGuid($"{keyVaultName}-{settings.Environment}-{suffix}-{KeyVaultSecretsUserRoleId}"),
            app,
            keyVault!,
            roleDefinitionId);
    }

    private static void CreateKeyVaultRoleAssignment(
        string roleAssignmentName,
        ContainerApp app,
        KeyVaultOutputs keyVault,
        string roleDefinitionId)
    {
        var principalId = app.Identity.Apply(identity => identity?.PrincipalId ?? string.Empty);

        _ = new global::Pulumi.AzureNative.Authorization.RoleAssignment(roleAssignmentName, new global::Pulumi.AzureNative.Authorization.RoleAssignmentArgs
        {
            PrincipalId = principalId,
            RoleDefinitionId = roleDefinitionId,
            Scope = keyVault.ResourceId
        }, new CustomResourceOptions
        {
            DependsOn = new[] { app }
        });
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

    private static string CreateDeterministicGuid(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Guid.NewGuid().ToString();
        }

        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash).ToString();
    }
}



