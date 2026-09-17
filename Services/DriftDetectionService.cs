using DeploymentKit.Constants;
using DeploymentKit.Extensions;
using DeploymentKit.Interfaces;
using DeploymentKit.Settings;
using Azure.ResourceManager.ContainerRegistry;
using Azure.ResourceManager.EventHubs;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Azure.ResourceManager.Storage;

namespace DeploymentKit.Services;

/// <summary>
/// Service for detecting drift between Pulumi state and actual Azure resources.
/// Identifies when resources have been modified outside of Pulumi management.
/// </summary>
public class DriftDetectionService(
    ILogger<DriftDetectionService> logger,
    IAzureAuthenticationService authService,
    IArmClientProvider armClientProvider,
    INamingConsistencyValidator? namingValidator = null)
    : IDriftDetectionService
{
    private readonly ILogger<DriftDetectionService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IAzureAuthenticationService _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    private readonly IArmClientProvider _armClientProvider = armClientProvider ?? throw new ArgumentNullException(nameof(armClientProvider));

    public async Task<PreDeploymentValidationResult> DetectDriftAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _logger.LogInformation("Starting comprehensive drift detection for environment: {Environment}", settings.Environment);

        var result = new PreDeploymentValidationResult();

        try
        {
            // Validate required settings first
            if (string.IsNullOrWhiteSpace(settings.SubscriptionId))
            {
                result.Errors.Add("SubscriptionId is required for drift detection");
                return result;
            }

            // Validate Azure authentication first
            if (!_authService.ValidateServicePrincipalCredentials())
            {
                result.Errors.Add("Azure Service Principal credentials are incomplete or invalid");
                return result;
            }

            // Detect drift for each resource type
            var driftTasks = new List<Task<PreDeploymentValidationResult>>
            {
                DetectPostgreSqlDriftAsync(settings),
                DetectEventHubsDriftAsync(settings),
                DetectVirtualNetworkDriftAsync(settings),
                DetectStorageAccountDriftAsync(settings),
                DetectKeyVaultDriftAsync(settings),
                DetectContainerRegistryDriftAsync(settings)
            };

            var driftResults = await Task.WhenAll(driftTasks);

            // Combine all drift detection results
            foreach (var driftResult in driftResults)
            {
                result.Errors.AddRange(driftResult.Errors);
                result.Warnings.AddRange(driftResult.Warnings);
                result.ValidatedResources.AddRange(driftResult.ValidatedResources);
            }

            result.IsValid = result.Errors.Count == 0;

            if (result.IsValid)
            {
                _logger.LogInformation("✅ No configuration drift detected");
            }
            else
            {
                _logger.LogWarning("⚠️ Configuration drift detected with {ErrorCount} errors and {WarningCount} warnings",
                    result.Errors.Count, result.Warnings.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect drift for environment: {Environment}", settings.Environment);
            result.Errors.Add($"Drift detection failed: {ex.Message}");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public async Task<PreDeploymentValidationResult> DetectResourceDriftAsync(string resourceType, string resourceName, InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(resourceName);
        ArgumentNullException.ThrowIfNull(settings);

        // Validate that parameters are not empty or whitespace
        if (string.IsNullOrWhiteSpace(resourceType))
        {
            throw new ArgumentNullException(nameof(resourceType));
        }

        if (string.IsNullOrWhiteSpace(resourceName))
        {
            throw new ArgumentNullException(nameof(resourceName));
        }

        _logger.LogInformation("Detecting drift for resource: {ResourceType}/{ResourceName}", resourceType, resourceName);

        return resourceType.ToLowerInvariant() switch
        {
            "postgresql" => await DetectPostgreSqlDriftAsync(settings),
            "eventhubs" => await DetectEventHubsDriftAsync(settings),
            "virtualnetwork" => await DetectVirtualNetworkDriftAsync(settings),
            "storageaccount" => await DetectStorageAccountDriftAsync(settings),
            "keyvault" => await DetectKeyVaultDriftAsync(settings),
            "containerregistry" => await DetectContainerRegistryDriftAsync(settings),
            _ => new PreDeploymentValidationResult
            {
                Errors = { $"Unknown resource type for drift detection: {resourceType}" }
            }
        };
    }

    public async Task<PreDeploymentValidationResult> DetectPostgreSqlDriftAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var result = new PreDeploymentValidationResult();

        try
        {
            _logger.LogInformation("Detecting PostgreSQL drift for environment: {Environment}", settings.Environment);

            var expectedServerName = namingValidator?.GenerateExpectedResourceName(settings, "PostgreSQL")
                ?? $"{settings.NamingPrefix}-{settings.Environment}-postgresql";
            var armClient = _armClientProvider.GetArmClient();
            var subscription = await armClient.GetDefaultSubscriptionAsync();
            var resourceGroup = await subscription.GetResourceGroupAsync(settings.ResourceGroupName);

            if (resourceGroup.HasValue)
            {
                var servers = resourceGroup.Value.GetPostgreSqlFlexibleServers();
                var serverExists = await servers.ExistsAsync(expectedServerName);

                if (serverExists.Value)
                {
                    var server = await servers.GetAsync(expectedServerName);
                    var serverResource = server.Value;

                    // Check for configuration drift
                    var driftIssues = new List<string>();

                    // Check location drift
                    if (!string.Equals(serverResource.Data.Location.ToString(), settings.Location, StringComparison.OrdinalIgnoreCase))
                    {
                        driftIssues.Add($"Location mismatch: Expected '{settings.Location}', Found '{serverResource.Data.Location.ToString()}'");
                    }

                    // Check SKU drift if specified
                    if (settings.Database?.SkuName != null)
                    {
                        var expectedSku = settings.Database.SkuName.ToStringValue();
                        var actualSku = serverResource.Data.Sku?.Name;

                        if (!string.Equals(actualSku, expectedSku, StringComparison.OrdinalIgnoreCase))
                        {
                            driftIssues.Add($"SKU mismatch: Expected '{expectedSku}', Found '{actualSku}'");
                        }
                    }

                    if (driftIssues.Any())
                    {
                        result.Warnings.AddRange(driftIssues.Select(issue => $"PostgreSQL drift: {issue}"));
                        _logger.LogWarning("PostgreSQL configuration drift detected: {Issues}", string.Join(", ", driftIssues));
                    }
                    else
                    {
                        result.ValidatedResources.Add($"✅ PostgreSQL server '{expectedServerName}' - No drift detected");
                    }
                }
                else
                {
                    result.Warnings.Add($"PostgreSQL server '{expectedServerName}' not found in Azure - may indicate state inconsistency");
                }
            }
            else
            {
                result.Errors.Add($"Resource group '{settings.ResourceGroupName}' not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect PostgreSQL drift");
            result.Errors.Add($"PostgreSQL drift detection failed: {ex.Message}");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public async Task<PreDeploymentValidationResult> DetectEventHubsDriftAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var result = new PreDeploymentValidationResult();

        try
        {
            _logger.LogInformation("Detecting Event Hubs drift for environment: {Environment}", settings.Environment);

            var expectedNamespaceName = namingValidator?.GenerateExpectedResourceName(settings, "EventHubs")
                ?? $"{settings.NamingPrefix}-{settings.Environment}-eventhubs";

            var armClient = _armClientProvider.GetArmClient();
            var subscription = await armClient.GetDefaultSubscriptionAsync();
            var resourceGroup = await subscription.GetResourceGroupAsync(settings.ResourceGroupName);

            if (resourceGroup.HasValue)
            {
                var namespaces = resourceGroup.Value.GetEventHubsNamespaces();
                var namespaceExists = await namespaces.ExistsAsync(expectedNamespaceName);

                if (namespaceExists.Value)
                {
                    var namespaceResource = await namespaces.GetAsync(expectedNamespaceName);
                    var eventHubsNamespace = namespaceResource.Value;

                    // Check for configuration drift
                    var driftIssues = new List<string>();

                    // Check location drift
                    if (!string.Equals(eventHubsNamespace.Data.Location.ToString(), settings.Location, StringComparison.OrdinalIgnoreCase))
                    {
                        driftIssues.Add($"Location mismatch: Expected '{settings.Location}', Found '{eventHubsNamespace.Data.Location.ToString()}'");
                    }

                    // Check SKU drift
                    if (settings.EventHubs?.SkuName != null)
                    {
                        var expectedSku = settings.EventHubs.SkuName.ToStringValue();
                        var actualSku = eventHubsNamespace.Data.Sku?.Name.ToString();

                        if (!string.Equals(actualSku, expectedSku, StringComparison.OrdinalIgnoreCase))
                        {
                            driftIssues.Add($"SKU mismatch: Expected '{expectedSku}', Found '{actualSku}'");
                        }
                    }

                    if (driftIssues.Any())
                    {
                        result.Warnings.AddRange(driftIssues.Select(issue => $"Event Hubs drift: {issue}"));
                        _logger.LogWarning("Event Hubs configuration drift detected: {Issues}", string.Join(", ", driftIssues));
                    }
                    else
                    {
                        result.ValidatedResources.Add($"✅ Event Hubs namespace '{expectedNamespaceName}' - No drift detected");
                    }
                }
                else
                {
                    result.Warnings.Add($"Event Hubs namespace '{expectedNamespaceName}' not found in Azure - may indicate state inconsistency");
                }
            }
            else
            {
                result.Errors.Add($"Resource group '{settings.ResourceGroupName}' not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect Event Hubs drift");
            result.Errors.Add($"Event Hubs drift detection failed: {ex.Message}");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public async Task<PreDeploymentValidationResult> DetectVirtualNetworkDriftAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var result = new PreDeploymentValidationResult();

        try
        {
            _logger.LogInformation("Detecting Virtual Network drift for environment: {Environment}", settings.Environment);

            var expectedVNetName = namingValidator?.GenerateExpectedResourceName(settings, "VirtualNetwork")
                ?? $"{settings.NamingPrefix}-{settings.Environment}-vnet";

            var armClient = _armClientProvider.GetArmClient();
            var subscription = await armClient.GetDefaultSubscriptionAsync();
            var resourceGroup = await subscription.GetResourceGroupAsync(settings.ResourceGroupName);

            if (resourceGroup.HasValue)
            {
                var virtualNetworks = resourceGroup.Value.GetVirtualNetworks();
                var vnetExists = await virtualNetworks.ExistsAsync(expectedVNetName);

                if (vnetExists.Value)
                {
                    var vnet = await virtualNetworks.GetAsync(expectedVNetName);
                    var virtualNetwork = vnet.Value;

                    // Check for configuration drift
                    var driftIssues = new List<string>();

                    // Check location drift
                    if (!string.Equals(virtualNetwork.Data.Location.ToString(), settings.Location, StringComparison.OrdinalIgnoreCase))
                    {
                        driftIssues.Add($"Location mismatch: Expected '{settings.Location}', Found '{virtualNetwork.Data.Location.ToString()}'");
                    }

                    // Check address space drift
                    if (settings.Network?.VirtualNetworkAddressSpace != null)
                    {
                        var expectedAddressSpace = settings.Network.VirtualNetworkAddressSpace;
                        var actualAddressSpaces = virtualNetwork.Data.AddressSpace?.AddressPrefixes;

                        if (actualAddressSpaces == null || !actualAddressSpaces.Contains(expectedAddressSpace))
                        {
                            driftIssues.Add($"Address space mismatch: Expected '{expectedAddressSpace}', Found '{string.Join(", ", actualAddressSpaces ?? new List<string>())}'");
                        }
                    }

                    if (driftIssues.Any())
                    {
                        result.Warnings.AddRange(driftIssues.Select(issue => $"Virtual Network drift: {issue}"));
                        _logger.LogWarning("Virtual Network configuration drift detected: {Issues}", string.Join(", ", driftIssues));
                    }
                    else
                    {
                        result.ValidatedResources.Add($"✅ Virtual Network '{expectedVNetName}' - No drift detected");
                    }
                }
                else
                {
                    result.Warnings.Add($"Virtual Network '{expectedVNetName}' not found in Azure - may indicate state inconsistency");
                }
            }
            else
            {
                result.Errors.Add($"Resource group '{settings.ResourceGroupName}' not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect Virtual Network drift");
            result.Errors.Add($"Virtual Network drift detection failed: {ex.Message}");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public async Task<PreDeploymentValidationResult> DetectStorageAccountDriftAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var result = new PreDeploymentValidationResult();

        try
        {
            _logger.LogInformation("Detecting Storage Account drift for environment: {Environment}", settings.Environment);

            var expectedStorageName = namingValidator?.GenerateExpectedResourceName(settings, "StorageAccount")
                ?? $"{settings.NamingPrefix}{settings.Environment}storage".Replace("-", "").ToLowerInvariant();

            var armClient = _armClientProvider.GetArmClient();
            var subscription = await armClient.GetDefaultSubscriptionAsync();
            var resourceGroup = await subscription.GetResourceGroupAsync(settings.ResourceGroupName);

            if (resourceGroup.HasValue)
            {
                var storageAccounts = resourceGroup.Value.GetStorageAccounts();
                var storageExists = await storageAccounts.ExistsAsync(expectedStorageName);

                if (storageExists.Value)
                {
                    var storage = await storageAccounts.GetAsync(expectedStorageName);
                    var storageAccount = storage.Value;

                    // Check for configuration drift
                    var driftIssues = new List<string>();

                    // Check location drift
                    if (!string.Equals(storageAccount.Data.Location.ToString(), settings.Location, StringComparison.OrdinalIgnoreCase))
                    {
                        driftIssues.Add($"Location mismatch: Expected '{settings.Location}', Found '{storageAccount.Data.Location.ToString()}'");
                    }

                    // Check SKU drift
                    var actualSku = storageAccount.Data.Sku?.Name.ToString();
                    var expectedSku = StorageConstants.StandardLrs; // Default from constants

                    if (!string.Equals(actualSku, expectedSku, StringComparison.OrdinalIgnoreCase))
                    {
                        driftIssues.Add($"SKU mismatch: Expected '{expectedSku}', Found '{actualSku}'");
                    }

                    if (driftIssues.Any())
                    {
                        result.Warnings.AddRange(driftIssues.Select(issue => $"Storage Account drift: {issue}"));
                        _logger.LogWarning("Storage Account configuration drift detected: {Issues}", string.Join(", ", driftIssues));
                    }
                    else
                    {
                        result.ValidatedResources.Add($"✅ Storage Account '{expectedStorageName}' - No drift detected");
                    }
                }
                else
                {
                    result.Warnings.Add($"Storage Account '{expectedStorageName}' not found in Azure - may indicate state inconsistency");
                }
            }
            else
            {
                result.Errors.Add($"Resource group '{settings.ResourceGroupName}' not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect Storage Account drift");
            result.Errors.Add($"Storage Account drift detection failed: {ex.Message}");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public async Task<PreDeploymentValidationResult> DetectKeyVaultDriftAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var result = new PreDeploymentValidationResult();

        try
        {
            _logger.LogInformation("Detecting Key Vault drift for environment: {Environment}", settings.Environment);

            var expectedKeyVaultName = namingValidator?.GenerateExpectedResourceName(settings, "KeyVault")
                ?? $"{settings.NamingPrefix}-{settings.Environment}-kv";

            var armClient = _armClientProvider.GetArmClient();
            var subscription = await armClient.GetDefaultSubscriptionAsync();
            var resourceGroup = await subscription.GetResourceGroupAsync(settings.ResourceGroupName);

            if (resourceGroup.HasValue)
            {
                var keyVaults = resourceGroup.Value.GetKeyVaults();
                var keyVaultExists = await keyVaults.ExistsAsync(expectedKeyVaultName);

                if (keyVaultExists.Value)
                {
                    var keyVault = await keyVaults.GetAsync(expectedKeyVaultName);
                    var keyVaultResource = keyVault.Value;

                    // Check for configuration drift
                    var driftIssues = new List<string>();

                    // Check location drift
                    if (!string.Equals(keyVaultResource.Data.Location.ToString(), settings.Location, StringComparison.OrdinalIgnoreCase))
                    {
                        driftIssues.Add($"Location mismatch: Expected '{settings.Location}', Found '{keyVaultResource.Data.Location.ToString()}'");
                    }

                    // Check SKU drift
                    var actualSku = keyVaultResource.Data.Properties?.Sku?.Name.ToString();
                    var expectedSku = "Standard"; // Default SKU

                    if (!string.Equals(actualSku, expectedSku, StringComparison.OrdinalIgnoreCase))
                    {
                        driftIssues.Add($"SKU mismatch: Expected '{expectedSku}', Found '{actualSku}'");
                    }

                    if (driftIssues.Any())
                    {
                        result.Warnings.AddRange(driftIssues.Select(issue => $"Key Vault drift: {issue}"));
                        _logger.LogWarning("Key Vault configuration drift detected: {Issues}", string.Join(", ", driftIssues));
                    }
                    else
                    {
                        result.ValidatedResources.Add($"✅ Key Vault '{expectedKeyVaultName}' - No drift detected");
                    }
                }
                else
                {
                    result.Warnings.Add($"Key Vault '{expectedKeyVaultName}' not found in Azure - may indicate state inconsistency");
                }
            }
            else
            {
                result.Errors.Add($"Resource group '{settings.ResourceGroupName}' not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect Key Vault drift");
            result.Errors.Add($"Key Vault drift detection failed: {ex.Message}");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public async Task<PreDeploymentValidationResult> DetectContainerRegistryDriftAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var result = new PreDeploymentValidationResult();

        try
        {
            _logger.LogInformation("Detecting Container Registry drift for environment: {Environment}", settings.Environment);

            var expectedRegistryName = namingValidator?.GenerateExpectedResourceName(settings, "ContainerRegistry")
                ?? $"{settings.NamingPrefix}{settings.Environment}acr".Replace("-", "").ToLowerInvariant();

            var armClient = _armClientProvider.GetArmClient();
            var subscription = await armClient.GetDefaultSubscriptionAsync();
            var resourceGroup = await subscription.GetResourceGroupAsync(settings.ResourceGroupName);

            if (resourceGroup.HasValue)
            {
                var registries = resourceGroup.Value.GetContainerRegistries();
                var registryExists = await registries.ExistsAsync(expectedRegistryName);

                if (registryExists.Value)
                {
                    var registry = await registries.GetAsync(expectedRegistryName);
                    var containerRegistry = registry.Value;

                    // Check for configuration drift
                    var driftIssues = new List<string>();

                    // Check location drift
                    if (!string.Equals(containerRegistry.Data.Location.ToString(), settings.Location, StringComparison.OrdinalIgnoreCase))
                    {
                        driftIssues.Add($"Location mismatch: Expected '{settings.Location}', Found '{containerRegistry.Data.Location.ToString()}'");
                    }

                    // Check SKU drift
                    var actualSku = containerRegistry.Data.Sku?.Name.ToString();
                    var expectedSku = "Basic"; // Default SKU

                    if (!string.Equals(actualSku, expectedSku, StringComparison.OrdinalIgnoreCase))
                    {
                        driftIssues.Add($"SKU mismatch: Expected '{expectedSku}', Found '{actualSku}'");
                    }

                    if (driftIssues.Any())
                    {
                        result.Warnings.AddRange(driftIssues.Select(issue => $"Container Registry drift: {issue}"));
                        _logger.LogWarning("Container Registry configuration drift detected: {Issues}", string.Join(", ", driftIssues));
                    }
                    else
                    {
                        result.ValidatedResources.Add($"✅ Container Registry '{expectedRegistryName}' - No drift detected");
                    }
                }
                else
                {
                    result.Warnings.Add($"Container Registry '{expectedRegistryName}' not found in Azure - may indicate state inconsistency");
                }
            }
            else
            {
                result.Errors.Add($"Resource group '{settings.ResourceGroupName}' not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect Container Registry drift");
            result.Errors.Add($"Container Registry drift detection failed: {ex.Message}");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public async Task<PreDeploymentValidationResult> GenerateDriftReportAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _logger.LogInformation("Generating comprehensive drift report for environment: {Environment}", settings.Environment);

        var driftResult = await DetectDriftAsync(settings);

        // Enhance the result with additional reporting information
        if (driftResult.IsValid)
        {
            driftResult.ValidatedResources.Add("📊 Drift Report: No configuration drift detected across all resources");
        }
        else
        {
            driftResult.ValidatedResources.Add($"📊 Drift Report: {driftResult.Errors.Count} errors and {driftResult.Warnings.Count} warnings detected");

            // Add remediation suggestions
            if (driftResult.Warnings.Any(w => w.Contains("not found in Azure")))
            {
                driftResult.ValidatedResources.Add("💡 Suggestion: Run 'pulumi refresh' to update state with actual Azure resources");
            }

            if (driftResult.Warnings.Any(w => w.Contains("mismatch")))
            {
                driftResult.ValidatedResources.Add("💡 Suggestion: Review and update infrastructure settings or run 'pulumi up' to align resources");
            }
        }

        return driftResult;
    }
}

