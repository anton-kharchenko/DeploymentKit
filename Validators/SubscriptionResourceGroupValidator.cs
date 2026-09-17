using DeploymentKit.Interfaces;
using DeploymentKit.Models.Results;
using DeploymentKit.Settings;
using Azure;
using Azure.Core;

namespace DeploymentKit.Validators;

/// <summary>
/// Validates Azure subscription and resource group targeting to prevent ResourceNotFound errors
/// </summary>
public class SubscriptionResourceGroupValidator(
    ILogger<SubscriptionResourceGroupValidator> logger,
    IArmClientProvider armClientProvider)
    : ISubscriptionResourceGroupValidator
{
    private readonly ILogger<SubscriptionResourceGroupValidator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IArmClientProvider _armClientProvider = armClientProvider ?? throw new ArgumentNullException(nameof(armClientProvider));

    /// <summary>
    /// Validates that the specified Azure subscription exists and is accessible
    /// </summary>
    public async Task<ValidationResult> ValidateSubscriptionAsync(string subscriptionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);
        
        _logger.LogInformation("Validating Azure subscription: {SubscriptionId}", subscriptionId);

        var validationErrors = new List<string>();
        var validationWarnings = new List<string>();

        try
        {
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                validationErrors.Add("Subscription ID is null or empty");
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = validationErrors
                };
            }

            // Validate subscription ID format (GUID)
            if (!Guid.TryParse(subscriptionId, out _))
            {
                validationErrors.Add($"Subscription ID '{subscriptionId}' is not a valid GUID format");
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = validationErrors
                };
            }

            // Try to get the subscription
            var armClient = await _armClientProvider.GetArmClientAsync();
            var subscriptionResource = armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));
            var subscription = await subscriptionResource.GetAsync();

            if (subscription?.Value == null)
            {
                validationErrors.Add($"Subscription '{subscriptionId}' not found or not accessible");
            }
            else
            {
                _logger.LogInformation("Successfully validated subscription: {SubscriptionId} - {DisplayName}",
                    subscriptionId, subscription.Value.Data.DisplayName);

                // Check subscription state
                if (subscription.Value.Data.State != Azure.ResourceManager.Resources.Models.SubscriptionState.Enabled)
                {
                    validationWarnings.Add($"Subscription '{subscriptionId}' is in state '{subscription.Value.Data.State}' (not Enabled)");
                }
            }

            return new ValidationResult
            {
                IsValid = validationErrors.Count == 0,
                Errors = validationErrors,
                Warnings = validationWarnings
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Subscription not found: {SubscriptionId}", subscriptionId);
            validationErrors.Add($"Subscription '{subscriptionId}' not found");
            return new ValidationResult
            {
                IsValid = false,
                Errors = validationErrors
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 403)
        {
            _logger.LogWarning("Access denied to subscription: {SubscriptionId}", subscriptionId);
            validationErrors.Add($"Access denied to subscription '{subscriptionId}'. Check authentication and permissions.");
            return new ValidationResult
            {
                IsValid = false,
                Errors = validationErrors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating subscription: {SubscriptionId}", subscriptionId);
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"Subscription validation failed: {ex.Message}"]
            };
        }
    }

    /// <summary>
    /// Validates that the specified resource group exists in the given subscription
    /// </summary>
    public async Task<ValidationResult> ValidateResourceGroupAsync(string subscriptionId, string resourceGroupName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(subscriptionId, nameof(subscriptionId));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
        
        _logger.LogInformation("Validating resource group: {ResourceGroup} in subscription: {SubscriptionId}",
            resourceGroupName, subscriptionId);

        var validationErrors = new List<string>();
        var validationWarnings = new List<string>();

        try
        {
            if (string.IsNullOrWhiteSpace(resourceGroupName))
            {
                validationErrors.Add("Resource group name is null or empty");
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = validationErrors
                };
            }

            // First validate the subscription
            var subscriptionResult = await ValidateSubscriptionAsync(subscriptionId);
            if (!subscriptionResult.IsValid)
            {
                validationErrors.AddRange(subscriptionResult.Errors);
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = validationErrors
                };
            }

            // Get the subscription resource
            var armClient = await _armClientProvider.GetArmClientAsync();
            var subscriptionResource = armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));

            // Try to get the resource group
            var resourceGroupCollection = subscriptionResource.GetResourceGroups();
            var resourceGroupExists = await resourceGroupCollection.ExistsAsync(resourceGroupName);

            if (!resourceGroupExists.Value)
            {
                validationErrors.Add($"Resource group '{resourceGroupName}' not found in subscription '{subscriptionId}'");
            }
            else
            {
                _logger.LogInformation("Successfully validated resource group: {ResourceGroup}", resourceGroupName);

                // Get additional resource group information
                var resourceGroup = await resourceGroupCollection.GetAsync(resourceGroupName);
                if (resourceGroup?.Value?.Data != null)
                {
                    _logger.LogDebug("Resource group location: {Location}, Provisioning state: {State}",
                        resourceGroup.Value.Data.Location, resourceGroup.Value.Data.ResourceGroupProvisioningState);
                }
            }

            return new ValidationResult
            {
                IsValid = validationErrors.Count == 0,
                Errors = validationErrors,
                Warnings = validationWarnings
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Resource group not found: {ResourceGroup}", resourceGroupName);
            validationErrors.Add($"Resource group '{resourceGroupName}' not found in subscription '{subscriptionId}'");
            return new ValidationResult
            {
                IsValid = false,
                Errors = validationErrors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating resource group: {ResourceGroup}", resourceGroupName);
            return new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { $"Resource group validation failed: {ex.Message}" }
            };
        }
    }

    /// <summary>
    /// Validates that the current Azure authentication context matches the expected subscription
    /// </summary>
    public async Task<ValidationResult> ValidateAuthenticationContextAsync(string expectedSubscriptionId)
    {
        _logger.LogInformation("Validating authentication context for subscription: {SubscriptionId}", expectedSubscriptionId);

        var validationErrors = new List<string>();
        var validationWarnings = new List<string>();

        try
        {
            var currentSubscriptionId = await GetCurrentSubscriptionIdAsync();

            if (string.IsNullOrWhiteSpace(currentSubscriptionId))
            {
                validationErrors.Add("Unable to determine current subscription from authentication context");
            }
            else if (!string.Equals(currentSubscriptionId, expectedSubscriptionId, StringComparison.OrdinalIgnoreCase))
            {
                validationWarnings.Add($"Current authentication context subscription '{currentSubscriptionId}' differs from expected '{expectedSubscriptionId}'");
            }
            else
            {
                _logger.LogInformation("Authentication context matches expected subscription: {SubscriptionId}", expectedSubscriptionId);
            }

            return new ValidationResult
            {
                IsValid = validationErrors.Count == 0,
                Errors = validationErrors,
                Warnings = validationWarnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating authentication context");
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"Authentication context validation failed: {ex.Message}"]
            };
        }
    }

    /// <summary>
    /// Validates the complete subscription and resource group configuration
    /// </summary>
    public async Task<ValidationResult> ValidateSubscriptionAndResourceGroupAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        
        _logger.LogInformation("Validating complete subscription and resource group configuration (SkipAzureAuthValidation: {SkipAuth})", 
            settings.SkipAzureAuthValidation);

        var allErrors = new List<string>();
        var allWarnings = new List<string>();

        try
        {
            // Check for null/empty values in settings and add specific error messages
            if (string.IsNullOrWhiteSpace(settings.SubscriptionId))
            {
                allErrors.Add("SubscriptionId is null or empty");
            }
            
            if (string.IsNullOrWhiteSpace(settings.ResourceGroupName))
            {
                allErrors.Add("ResourceGroupName is null or empty");
            }

            // If we have basic validation errors, return early
            if (allErrors.Count > 0)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = allErrors,
                    Warnings = allWarnings
                };
            }

            // Skip Azure connectivity validation if requested
            if (settings.SkipAzureAuthValidation)
            {
                _logger.LogInformation("Skipping Azure connectivity validation (SkipAzureAuthValidation = true)");
                allWarnings.Add("Azure connectivity validation was skipped. Resource group existence will be validated during deployment.");
                
                return new ValidationResult
                {
                    IsValid = true,
                    Errors = allErrors,
                    Warnings = allWarnings
                };
            }

            // Validate subscription
            var subscriptionResult = await ValidateSubscriptionAsync(settings.SubscriptionId);
            
            // Determine how to handle validation failures based on ValidationMode
            if (!subscriptionResult.IsValid)
            {
                if (settings.ValidationMode == Enums.ValidationMode.Full)
                {
                    // In Full validation mode, treat subscription validation failures as errors
                    _logger.LogError("Subscription validation failed in Full validation mode");
                    allErrors.AddRange(subscriptionResult.Errors);
                }
                else
                {
                    // In Basic/Minimal modes, treat Azure connectivity errors as warnings - they might be using CLI auth
                    _logger.LogWarning("Subscription validation failed - this may be normal if using Azure CLI authentication");
                    allWarnings.AddRange(subscriptionResult.Errors.Select(e => $"⚠️  {e}"));
                    allWarnings.Add("Continuing deployment - subscription will be validated by Azure SDK during deployment");
                }
            }
            else
            {
                allWarnings.AddRange(subscriptionResult.Warnings);
            }

            // Validate resource group (only if subscription is valid)
            if (subscriptionResult.IsValid)
            {
                var resourceGroupResult = await ValidateResourceGroupAsync(settings.SubscriptionId, settings.ResourceGroupName);
                
                // Resource group not existing is not an error for initial deployment (except in Full mode)
                if (!resourceGroupResult.IsValid)
                {
                    if (settings.ValidationMode == Enums.ValidationMode.Full)
                    {
                        // In Full validation mode, resource group must exist
                        _logger.LogError("Resource group validation failed in Full validation mode");
                        allErrors.AddRange(resourceGroupResult.Errors);
                    }
                    else
                    {
                        // In Basic/Minimal modes, this is expected for initial deployments
                        _logger.LogInformation("Resource group validation failed - this is expected for initial deployments");
                        allWarnings.AddRange(resourceGroupResult.Errors.Select(e => $"ℹ️  {e}"));
                    }
                }
                else
                {
                    allWarnings.AddRange(resourceGroupResult.Warnings);
                    
                    // Validate resource group location if both subscription and RG are valid
                    if (!string.IsNullOrWhiteSpace(settings.Location))
                    {
                        var locationResult = await ValidateResourceGroupLocationAsync(
                            settings.SubscriptionId,
                            settings.ResourceGroupName,
                            settings.Location);
                        allErrors.AddRange(locationResult.Errors);
                        allWarnings.AddRange(locationResult.Warnings);
                    }
                }
            }

            // Validate authentication context - don't treat as error
            var authResult = await ValidateAuthenticationContextAsync(settings.SubscriptionId);
            if (!authResult.IsValid)
            {
                allWarnings.AddRange(authResult.Errors.Select(e => $"ℹ️  {e}"));
            }
            allWarnings.AddRange(authResult.Warnings);

            // Only fail if we have actual errors (not warnings converted from errors)
            return new ValidationResult
            {
                IsValid = allErrors.Count == 0,
                Errors = allErrors,
                Warnings = allWarnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Azure connectivity check failed - this is normal if using Azure CLI authentication");
            return new ValidationResult
            {
                IsValid = true,
                Errors = [],
                Warnings = [$"⚠️  Azure connectivity check failed: {ex.Message}. Continuing with deployment - Azure SDK will handle authentication."]
            };
        }
    }

    /// <summary>
    /// Gets the current Azure subscription ID from the authentication context
    /// </summary>
    public virtual async Task<string?> GetCurrentSubscriptionIdAsync()
    {
        try
        {
            // Try to get the default subscription from the ARM client
            var armClient = await _armClientProvider.GetArmClientAsync();
            var subscriptions = armClient.GetSubscriptions();
            await foreach (var subscription in subscriptions)
            {
                // Return the first accessible subscription
                // In practice, this might need more sophisticated logic
                return subscription.Data.SubscriptionId;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current subscription ID");
            return null;
        }
    }

    /// <summary>
    /// Lists all resource groups in the specified subscription
    /// </summary>
    public async Task<IEnumerable<string>> ListResourceGroupsAsync(string subscriptionId)
    {
        _logger.LogDebug("Listing resource groups in subscription: {SubscriptionId}", subscriptionId);

        try
        {
            var armClient = await _armClientProvider.GetArmClientAsync();
            var subscriptionResource = armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));
            var resourceGroups = subscriptionResource.GetResourceGroups();

            var resourceGroupNames = new List<string>();
            await foreach (var resourceGroup in resourceGroups)
            {
                resourceGroupNames.Add(resourceGroup.Data.Name);
            }

            _logger.LogDebug("Found {Count} resource groups in subscription: {SubscriptionId}",
                resourceGroupNames.Count, subscriptionId);

            return resourceGroupNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing resource groups in subscription: {SubscriptionId}", subscriptionId);
            return [];
        }
    }

    /// <summary>
    /// Validates that the resource group location matches the expected location
    /// </summary>
    public async Task<ValidationResult> ValidateResourceGroupLocationAsync(
        string subscriptionId,
        string resourceGroupName,
        string expectedLocation)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(subscriptionId, nameof(subscriptionId));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(expectedLocation, nameof(expectedLocation));
        
        _logger.LogDebug("Validating resource group location. RG: {ResourceGroup}, Expected: {Location}",
            resourceGroupName, expectedLocation);

        var validationErrors = new List<string>();
        var validationWarnings = new List<string>();

        try
        {
            var armClient = await _armClientProvider.GetArmClientAsync();
            var subscriptionResource = armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));
            var resourceGroupCollection = subscriptionResource.GetResourceGroups();

            var resourceGroup = await resourceGroupCollection.GetAsync(resourceGroupName);
            if (resourceGroup?.Value?.Data != null)
            {
                var actualLocation = resourceGroup.Value.Data.Location.Name;

                if (!string.Equals(actualLocation, expectedLocation, StringComparison.OrdinalIgnoreCase))
                {
                    validationWarnings.Add($"Resource group '{resourceGroupName}' is in location '{actualLocation}' but expected '{expectedLocation}'");
                }
                else
                {
                    _logger.LogDebug("Resource group location matches expected: {Location}", expectedLocation);
                }
            }

            return new ValidationResult
            {
                IsValid = validationErrors.Count == 0,
                Errors = validationErrors,
                Warnings = validationWarnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating resource group location");
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"Resource group location validation failed: {ex.Message}"]
            };
        }
    }
}

