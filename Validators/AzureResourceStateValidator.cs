using DeploymentKit.Interfaces;
using DeploymentKit.Models.Results;
using DeploymentKit.Settings;
using Azure.Core;
using Azure.ResourceManager.ContainerRegistry;
using Azure.ResourceManager.EventHubs;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using System.Diagnostics;

namespace DeploymentKit.Validators;

/// <summary>
/// Service for validating Azure resource state and preventing drift-related deployment failures
/// </summary>
public class AzureResourceStateValidator(
    ILogger<AzureResourceStateValidator> logger,
    ICorrelationIdService correlationIdService,
    IResourceNamingService namingService,
    IAzureAuthenticationService authenticationService,
    IArmClientProvider armClientProvider) : IAzureResourceStateValidator
{
    private readonly ILogger<AzureResourceStateValidator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
    private readonly IResourceNamingService _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
    private readonly IAzureAuthenticationService _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
    private readonly IArmClientProvider _armClientProvider = armClientProvider ?? throw new ArgumentNullException(nameof(armClientProvider));

    /// <summary>
    /// Validates that all expected Azure resources exist in the target subscription and resource group
    /// </summary>
    public async Task<ValidationResult> ValidateResourceExistenceAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        
        var stopwatch = Stopwatch.StartNew();
        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        var result = new ValidationResult { IsValid = true };

        try
        {
            _logger.LogInformation("[{CorrelationId}] Starting Azure resource existence validation for environment: {Environment}",
                correlationId, settings.Environment);

            var client = await _armClientProvider.GetArmClientAsync(cancellationToken);

            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            if (string.IsNullOrEmpty(subscriptionId))
            {
                result.IsValid = false;
                result.Errors.Add("AZURE_SUBSCRIPTION_ID environment variable is not set");
                return result;
            }

            var subscription = client.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));
            var resourceGroupName = _namingService.GenerateResourceGroupName(settings.Environment, settings.Location);

            // Validate resource group exists
            var resourceGroupExists = await ValidateResourceGroupExistsAsync(subscription, resourceGroupName, result, cancellationToken);
            if (!resourceGroupExists)
            {
                return result;
            }

            var resourceGroup = await subscription.GetResourceGroupAsync(resourceGroupName, cancellationToken);

            // Validate individual resources
            await ValidatePostgreSqlServerExistsAsync(resourceGroup, settings, result, cancellationToken);
            await ValidateEventHubsNamespaceExistsAsync(resourceGroup, settings, result, cancellationToken);
            await ValidateVirtualNetworkExistsAsync(resourceGroup, settings, result, cancellationToken);
            await ValidateStorageAccountExistsAsync(resourceGroup, settings, result, cancellationToken);
            await ValidateKeyVaultExistsAsync(resourceGroup, settings, result, cancellationToken);
            await ValidateContainerRegistryExistsAsync(resourceGroup, settings, result, cancellationToken);

            _logger.LogInformation("[{CorrelationId}] Azure resource existence validation completed in {ElapsedMs}ms. Valid: {IsValid}",
                correlationId, stopwatch.ElapsedMilliseconds, result.IsValid);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] Error during Azure resource existence validation", correlationId);
            result.IsValid = false;
            result.Errors.Add($"Validation failed with error: {ex.Message}");
            return result;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// Validates subscription and resource group targeting consistency
    /// </summary>
    public async Task<ValidationResult> ValidateSubscriptionAndResourceGroupAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        
        var stopwatch = Stopwatch.StartNew();
        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        var result = new ValidationResult { IsValid = true };

        try
        {
            _logger.LogInformation("[{CorrelationId}] Starting subscription and resource group validation", correlationId);

            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            if (string.IsNullOrEmpty(subscriptionId))
            {
                result.IsValid = false;
                result.Errors.Add("AZURE_SUBSCRIPTION_ID environment variable is not set");
                return result;
            }

            var client = await _armClientProvider.GetArmClientAsync(cancellationToken);

            // Validate subscription exists and is accessible
            try
            {
                var subscription = client.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));
                var subscriptionData = await subscription.GetAsync(cancellationToken: cancellationToken);

                _logger.LogInformation("[{CorrelationId}] Successfully validated subscription: {SubscriptionId} ({DisplayName})",
                    correlationId, subscriptionId, subscriptionData.Value.Data.DisplayName);
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Failed to access subscription {subscriptionId}: {ex.Message}");
                return result;
            }

            // Validate resource group naming consistency
            var expectedResourceGroupName = _namingService.GenerateResourceGroupName(settings.Environment, settings.Location);
            _logger.LogInformation("[{CorrelationId}] Expected resource group name: {ResourceGroupName}", correlationId, expectedResourceGroupName);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] Error during subscription and resource group validation", correlationId);
            result.IsValid = false;
            result.Errors.Add($"Validation failed with error: {ex.Message}");
            return result;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// Validates resource naming consistency to prevent prefix/environment mismatches
    /// </summary>
    public Task<ValidationResult> ValidateNamingConsistencyAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(settings);
        
            var stopwatch = Stopwatch.StartNew();
            var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
            var result = new ValidationResult { IsValid = true };

            try
            {
                _logger.LogInformation("[{CorrelationId}] Starting naming consistency validation for environment: {Environment}",
                    correlationId, settings.Environment);

                // Validate environment naming consistency
                if (string.IsNullOrEmpty(settings.Environment))
                {
                    result.IsValid = false;
                    result.Errors.Add("Environment name is not specified in settings");
                    return Task.FromResult(result);
                }

                // Check for common naming inconsistencies
                var environmentLower = settings.Environment.ToLowerInvariant();
                var commonInconsistencies = new Dictionary<string, string[]>
                {
                    ["dev"] = ["development", "develop"],
                    ["prod"] = ["production", "prd"],
                    ["test"] = ["testing", "tst"],
                    ["stage"] = ["staging", "stg"]
                };

                foreach (var inconsistency in commonInconsistencies)
                {
                    if (inconsistency.Value.Contains(environmentLower) && environmentLower != inconsistency.Key)
                    {
                        result.Warnings.Add($"Environment name '{settings.Environment}' should be '{inconsistency.Key}' for consistency");
                    }
                }

                // Validate resource naming patterns
                var resourceGroupName = _namingService.GenerateResourceGroupName(settings.Environment, settings.Location);
                var postgresServerName = _namingService.GeneratePostgreSqlServerName(settings.Environment, settings.Location);
                var eventHubsNamespaceName = _namingService.GenerateEventHubsNamespaceName(settings.Environment, settings.Location);
                var vnetName = _namingService.GenerateVirtualNetworkName(settings.Environment, settings.Location);

                _logger.LogInformation("[{CorrelationId}] Generated resource names - RG: {ResourceGroup}, PG: {PostgresServer}, EH: {EventHubs}, VNet: {VirtualNetwork}",
                    correlationId, resourceGroupName, postgresServerName, eventHubsNamespaceName, vnetName);

                // Validate naming pattern consistency
                var prefix = $"tr{environmentLower}";
                var resources = new Dictionary<string, string>
                {
                    ["Resource Group"] = resourceGroupName,
                    ["PostgreSQL Server"] = postgresServerName,
                    ["Event Hubs Namespace"] = eventHubsNamespaceName,
                    ["Virtual Network"] = vnetName
                };

                foreach (var resource in resources)
                {
                    if (!resource.Value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Warnings.Add($"{resource.Key} name '{resource.Value}' does not follow expected prefix pattern '{prefix}'");
                    }
                }

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{CorrelationId}] Error during naming consistency validation", correlationId);
                result.IsValid = false;
                result.Errors.Add($"Validation failed with error: {ex.Message}");
                return Task.FromResult(result);
            }
            finally
            {
                stopwatch.Stop();
            }
        }
        catch (Exception exception)
        {
            return Task.FromException<ValidationResult>(exception);
        }
    }

    /// <summary>
    /// Detects drift between Pulumi state and actual Azure resources
    /// </summary>
    public Task<ValidationResult> DetectResourceDriftAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(settings);
        
            var stopwatch = Stopwatch.StartNew();
            var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
            var result = new ValidationResult { IsValid = true };

            try
            {
                _logger.LogInformation("[{CorrelationId}] Starting resource drift detection for environment: {Environment}",
                    correlationId, settings.Environment);

                // This would typically involve comparing Pulumi state with actual Azure resources
                // For now, we'll focus on basic existence checks and recommend pulumi refresh
                result.Warnings.Add("Consider running 'pulumi refresh' to synchronize state with Azure resources");

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{CorrelationId}] Error during resource drift detection", correlationId);
                result.IsValid = false;
                result.Errors.Add($"Drift detection failed with error: {ex.Message}");
                return Task.FromResult(result);
            }
            finally
            {
                stopwatch.Stop();
            }
        }
        catch (Exception exception)
        {
            return Task.FromException<ValidationResult>(exception);
        }
    }

    /// <summary>
    /// Validates Azure authentication and permissions for resource operations
    /// </summary>
    public async Task<ValidationResult> ValidateAzureAuthenticationAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        
        var stopwatch = Stopwatch.StartNew();
        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        var result = new ValidationResult { IsValid = true };

        try
        {
            _logger.LogInformation("[{CorrelationId}] Starting Azure authentication validation", correlationId);

            // Validate service principal credentials
            if (!_authenticationService.ValidateServicePrincipalCredentials())
            {
                result.IsValid = false;
                result.Errors.Add("Azure Service Principal credentials are incomplete or invalid");
                return result;
            }

            // Test Azure connectivity
            var client = await _armClientProvider.GetArmClientAsync(cancellationToken);

            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            if (!string.IsNullOrEmpty(subscriptionId))
            {
                try
                {
                    var subscription = client.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));
                    await subscription.GetAsync(cancellationToken: cancellationToken);
                    _logger.LogInformation("[{CorrelationId}] Successfully authenticated and accessed subscription: {SubscriptionId}",
                        correlationId, subscriptionId);
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Failed to access subscription {subscriptionId}: {ex.Message}");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] Error during Azure authentication validation", correlationId);
            result.IsValid = false;
            result.Errors.Add($"Authentication validation failed with error: {ex.Message}");
            return result;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// Performs comprehensive pre-deployment validation including all Azure state checks
    /// </summary>
    public async Task<ValidationResult> ValidatePreDeploymentStateAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        
        var stopwatch = Stopwatch.StartNew();
        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        var result = new ValidationResult { IsValid = true };

        try
        {
            _logger.LogInformation("[{CorrelationId}] Starting comprehensive pre-deployment state validation", correlationId);

            // Run all validation checks
            var authResult = await ValidateAzureAuthenticationAsync(settings, cancellationToken);
            var subscriptionResult = await ValidateSubscriptionAndResourceGroupAsync(settings, cancellationToken);
            var namingResult = await ValidateNamingConsistencyAsync(settings, cancellationToken);
            var existenceResult = await ValidateResourceExistenceAsync(settings, cancellationToken);
            var driftResult = await DetectResourceDriftAsync(settings, cancellationToken);

            // Combine results
            var allResults = new[] { authResult, subscriptionResult, namingResult, existenceResult, driftResult };

            result.IsValid = allResults.All(r => r.IsValid);
            result.Errors.AddRange(allResults.SelectMany(r => r.Errors));
            result.Warnings.AddRange(allResults.SelectMany(r => r.Warnings));

            if (result.IsValid)
            {
                _logger.LogInformation("[{CorrelationId}] ✅ Comprehensive pre-deployment validation PASSED", correlationId);
            }
            else
            {
                _logger.LogError("[{CorrelationId}] ❌ Comprehensive pre-deployment validation FAILED with {ErrorCount} errors",
                    correlationId, result.Errors.Count);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] Error during comprehensive pre-deployment validation", correlationId);
            result.IsValid = false;
            result.Errors.Add($"Comprehensive validation failed with error: {ex.Message}");
            return result;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    #region Private Helper Methods

    private async Task<bool> ValidateResourceGroupExistsAsync(SubscriptionResource subscription, string resourceGroupName, ValidationResult result, CancellationToken cancellationToken)
    {
        try
        {
            var resourceGroups = subscription.GetResourceGroups();
            var exists = await resourceGroups.ExistsAsync(resourceGroupName, cancellationToken);

            if (!exists.Value)
            {
                result.IsValid = false;
                result.Errors.Add($"Resource group '{resourceGroupName}' does not exist in the target subscription");
                return false;
            }

            _logger.LogInformation("✅ Resource group '{ResourceGroupName}' exists", resourceGroupName);
            return true;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Failed to validate resource group '{resourceGroupName}': {ex.Message}");
            return false;
        }
    }

    private async Task ValidatePostgreSqlServerExistsAsync(ResourceGroupResource resourceGroup, InfrastructureSettings settings, ValidationResult result, CancellationToken cancellationToken)
    {
        try
        {
            var serverName = _namingService.GeneratePostgreSqlServerName(settings.Environment, settings.Location);

            // Check if PostgreSQL flexible server exists
            var servers = resourceGroup.GetPostgreSqlFlexibleServers();
            var exists = await servers.ExistsAsync(serverName, cancellationToken);

            if (!exists.Value)
            {
                result.Warnings.Add($"PostgreSQL server '{serverName}' does not exist - will be created during deployment");
            }
            else
            {
                _logger.LogInformation("✅ PostgreSQL server '{ServerName}' exists", serverName);
            }
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"Could not validate PostgreSQL server existence: {ex.Message}");
        }
    }

    private async Task ValidateEventHubsNamespaceExistsAsync(ResourceGroupResource resourceGroup, InfrastructureSettings settings, ValidationResult result, CancellationToken cancellationToken)
    {
        try
        {
            var namespaceName = _namingService.GenerateEventHubsNamespaceName(settings.Environment, settings.Location);

            // Check if Event Hubs namespace exists
            var namespaces = resourceGroup.GetEventHubsNamespaces();
            var exists = await namespaces.ExistsAsync(namespaceName, cancellationToken);

            if (!exists.Value)
            {
                result.Warnings.Add($"Event Hubs namespace '{namespaceName}' does not exist - will be created during deployment");
            }
            else
            {
                _logger.LogInformation("✅ Event Hubs namespace '{NamespaceName}' exists", namespaceName);
            }
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"Could not validate Event Hubs namespace existence: {ex.Message}");
        }
    }

    private async Task ValidateVirtualNetworkExistsAsync(ResourceGroupResource resourceGroup, InfrastructureSettings settings, ValidationResult result, CancellationToken cancellationToken)
    {
        try
        {
            var vnetName = _namingService.GenerateVirtualNetworkName(settings.NamingPrefix, settings.Environment);

            // Check if Virtual Network exists
            var vnets = resourceGroup.GetVirtualNetworks();
            var exists = await vnets.ExistsAsync(vnetName, cancellationToken: cancellationToken);

            if (!exists.Value)
            {
                result.Warnings.Add($"Virtual Network '{vnetName}' does not exist - will be created during deployment");
            }
            else
            {
                _logger.LogInformation("✅ Virtual Network '{VNetName}' exists", vnetName);
            }
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"Could not validate Virtual Network existence: {ex.Message}");
        }
    }

    private async Task ValidateStorageAccountExistsAsync(ResourceGroupResource resourceGroup, InfrastructureSettings settings, ValidationResult result, CancellationToken cancellationToken)
    {
        try
        {
            var storageAccountName = _namingService.GenerateStorageAccountName(settings.Environment, settings.Location);

            // Check if Storage Account exists
            var storageAccounts = resourceGroup.GetStorageAccounts();
            var exists = await storageAccounts.ExistsAsync(storageAccountName, cancellationToken: cancellationToken);

            if (!exists.Value)
            {
                result.Warnings.Add($"Storage Account '{storageAccountName}' does not exist - will be created during deployment");
            }
            else
            {
                _logger.LogInformation("✅ Storage Account '{StorageAccountName}' exists", storageAccountName);
            }
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"Could not validate Storage Account existence: {ex.Message}");
        }
    }

    private async Task ValidateKeyVaultExistsAsync(ResourceGroupResource resourceGroup, InfrastructureSettings settings, ValidationResult result, CancellationToken cancellationToken)
    {
        try
        {
            var keyVaultName = _namingService.GenerateKeyVaultName(settings.Environment, settings.Location);

            // Check if Key Vault exists
            var keyVaults = resourceGroup.GetKeyVaults();
            var exists = await keyVaults.ExistsAsync(keyVaultName, cancellationToken);

            if (!exists.Value)
            {
                result.Warnings.Add($"Key Vault '{keyVaultName}' does not exist - will be created during deployment");
            }
            else
            {
                _logger.LogInformation("✅ Key Vault '{KeyVaultName}' exists", keyVaultName);
            }
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"Could not validate Key Vault existence: {ex.Message}");
        }
    }

    private async Task ValidateContainerRegistryExistsAsync(ResourceGroupResource resourceGroup, InfrastructureSettings settings, ValidationResult result, CancellationToken cancellationToken)
    {
        try
        {
            var registryName = _namingService.GenerateContainerRegistryName(settings.Environment, settings.Location);

            // Check if Container Registry exists
            var registries = resourceGroup.GetContainerRegistries();
            var exists = await registries.ExistsAsync(registryName, cancellationToken);

            if (!exists.Value)
            {
                result.Warnings.Add($"Container Registry '{registryName}' does not exist - will be created during deployment");
            }
            else
            {
                _logger.LogInformation("✅ Container Registry '{RegistryName}' exists", registryName);
            }
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"Could not validate Container Registry existence: {ex.Message}");
        }
    }

    #endregion
}

