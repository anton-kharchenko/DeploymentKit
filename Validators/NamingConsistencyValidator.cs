using DeploymentKit.Constants;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Results;
using DeploymentKit.Settings;
using System.Text.RegularExpressions;

namespace DeploymentKit.Validators;

/// <summary>
/// Validates naming consistency across Azure resources to prevent ResourceNotFound errors
/// due to naming prefix/environment mismatches
/// </summary>
public class NamingConsistencyValidator(ILogger<NamingConsistencyValidator> logger) : INamingConsistencyValidator
{
    private readonly ILogger<NamingConsistencyValidator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Validates that the naming prefix and environment combination is consistent
    /// </summary>
    public async Task<ValidationResult> ValidateNamingConsistencyAsync(InfrastructureSettings settings)
    {
        _logger.LogInformation("Starting naming consistency validation for environment: {Environment}, prefix: {Prefix}",
            settings.Environment, settings.NamingPrefix);

        var validationErrors = new List<string>();
        var validationWarnings = new List<string>();

        try
        {
            // Validate environment naming consistency
            var envResult = await ValidateEnvironmentNamingAsync(settings);
            if (!envResult.IsValid)
            {
                validationErrors.AddRange(envResult.Errors);
                validationWarnings.AddRange(envResult.Warnings);
            }

            // Validate resource group naming
            var rgResult = await ValidateResourceGroupNamingAsync(settings, settings.ResourceGroupName);
            if (!rgResult.IsValid)
            {
                validationErrors.AddRange(rgResult.Errors);
                validationWarnings.AddRange(rgResult.Warnings);
            }

            // Validate naming prefix format
            if (!IsValidNamingPrefix(settings.NamingPrefix))
            {
                validationErrors.Add($"Naming prefix '{settings.NamingPrefix}' is invalid. Must start with lowercase letter and contain only lowercase letters and numbers.");
            }

            // Validate environment format
            if (!IsValidEnvironment(settings.Environment))
            {
                validationErrors.Add($"Environment '{settings.Environment}' is invalid. Must be one of: dev, test, staging, prod");
            }

            // Generate expected names and validate consistency
            var expectedNames = await GenerateExpectedResourceNamesAsync(settings);
            foreach (var (resourceType, expectedName) in expectedNames)
            {
                if (string.IsNullOrWhiteSpace(expectedName))
                {
                    validationErrors.Add($"Generated name for {resourceType} is empty or invalid");
                }
                else if (expectedName.Length > GetMaxNameLength(resourceType))
                {
                    validationErrors.Add($"Generated name for {resourceType} '{expectedName}' exceeds maximum length of {GetMaxNameLength(resourceType)} characters");
                }
            }

            var result = new ValidationResult
            {
                IsValid = validationErrors.Count == 0,
                Errors = validationErrors,
                Warnings = validationWarnings
            };

            _logger.LogInformation("Naming consistency validation completed. Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}",
                result.IsValid, result.Errors.Count, result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during naming consistency validation");
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"Naming consistency validation failed: {ex.Message}"]
            };
        }
    }

    /// <summary>
    /// Validates that generated resource names match expected patterns (synchronous version)
    /// </summary>
    public ValidationResult ValidateResourceName(string resourceName, InfrastructureSettings settings, string resourceType)
    {
        // Validate parameters and throw exceptions as expected by tests
        ArgumentNullException.ThrowIfNull(resourceName);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceType);

        _logger.LogDebug("Validating resource name '{ResourceName}' for {ResourceType}", resourceName, resourceType);

        var validationErrors = new List<string>();
        var validationWarnings = new List<string>();

        try
        {
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                validationErrors.Add($"Resource name for {resourceType} is null or empty");
            }

            if (string.IsNullOrWhiteSpace(resourceType))
            {
                validationErrors.Add("Resource type is null or empty");
            }

            // Generate expected name for comparison
            var expectedName = GenerateExpectedResourceName(settings, resourceType);

            if (!string.Equals(resourceName, expectedName, StringComparison.OrdinalIgnoreCase))
            {
                validationErrors.Add($"Resource name '{resourceName}' doesn't match expected naming pattern. Expected: '{expectedName}'");
            }

            // Validate name follows Azure naming conventions
            if (!string.IsNullOrWhiteSpace(resourceName) && !IsValidResourceName(resourceType, resourceName))
            {
                validationWarnings.Add($"Resource name '{resourceName}' for {resourceType} may not follow Azure naming conventions");
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
            _logger.LogError(ex, "Error validating resource name '{ResourceName}' for {ResourceType}", resourceName, resourceType);
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"Resource name validation failed for {resourceType}: {ex.Message}"]
            };
        }
    }

    /// <summary>
    /// Validates that generated resource names match expected patterns
    /// </summary>
    public Task<ValidationResult> ValidateResourceNameAsync(
        InfrastructureSettings settings,
        string resourceType,
        string expectedName,
        string actualName)
    {
        try
        {
            _logger.LogDebug("Validating resource name for {ResourceType}. Expected: {Expected}, Actual: {Actual}",
                resourceType, expectedName, actualName);

            var validationErrors = new List<string>();
            var validationWarnings = new List<string>();

            try
            {
                if (string.IsNullOrWhiteSpace(expectedName))
                {
                    validationErrors.Add($"Expected name for {resourceType} is null or empty");
                }

                if (string.IsNullOrWhiteSpace(actualName))
                {
                    validationErrors.Add($"Actual name for {resourceType} is null or empty");
                }

                if (!string.Equals(expectedName, actualName, StringComparison.OrdinalIgnoreCase))
                {
                    validationErrors.Add($"Resource name mismatch for {resourceType}. Expected: '{expectedName}', Actual: '{actualName}'");
                }

                // Validate name follows expected pattern
                if (!string.IsNullOrWhiteSpace(expectedName) && !IsValidResourceName(resourceType, expectedName))
                {
                    validationWarnings.Add($"Resource name '{expectedName}' for {resourceType} may not follow Azure naming conventions");
                }

                return Task.FromResult(new ValidationResult
                {
                    IsValid = validationErrors.Count == 0,
                    Errors = validationErrors,
                    Warnings = validationWarnings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating resource name for {ResourceType}", resourceType);
                return Task.FromResult(new ValidationResult
                {
                    IsValid = false,
                    Errors = [$"Resource name validation failed for {resourceType}: {ex.Message}"]
                });
            }
        }
        catch (Exception exception)
        {
            return Task.FromException<ValidationResult>(exception);
        }
    }

    /// <summary>
    /// Generates expected resource names based on current settings
    /// </summary>
    public Task<Dictionary<string, string>> GenerateExpectedResourceNamesAsync(InfrastructureSettings settings)
    {
        _logger.LogDebug("Generating expected resource names for environment: {Environment}, prefix: {Prefix}",
            settings.Environment, settings.NamingPrefix);

        try
        {
            var names = GenerateExpectedResourceNames(settings);

            _logger.LogDebug("Generated {Count} expected resource names", names.Count);
            return Task.FromResult(names);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating expected resource names");
            return Task.FromResult(new Dictionary<string, string>());
        }
    }

    /// <summary>
    /// Validates that environment naming is consistent (dev vs development)
    /// </summary>
    public Task<ValidationResult> ValidateEnvironmentNamingAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _logger.LogDebug("Validating environment naming consistency for: {Environment}", settings.Environment);

        var validationErrors = new List<string>();
        var validationWarnings = new List<string>();

        try
        {
            var environment = settings.Environment.ToLowerInvariant();

            // Check for common inconsistencies
            if (environment.Contains("development") && !environment.Equals("dev"))
            {
                validationWarnings.Add("Environment contains 'development' but should use 'dev' for consistency");
            }

            if (environment.Contains("production") && !environment.Equals("prod"))
            {
                validationWarnings.Add("Environment contains 'production' but should use 'prod' for consistency");
            }

            // Validate against allowed values
            var allowedEnvironments = new[] { "dev", "development", "test", "staging", "prod", "production" };
            if (!allowedEnvironments.Contains(environment))
            {
                validationErrors.Add($"Environment '{environment}' is not in allowed list: {string.Join(", ", allowedEnvironments)}");
            }

            return Task.FromResult(new ValidationResult
            {
                IsValid = validationErrors.Count == 0,
                Errors = validationErrors,
                Warnings = validationWarnings
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating environment naming");
            return Task.FromResult(new ValidationResult
            {
                IsValid = false,
                Errors = [$"Environment naming validation failed: {ex.Message}"]
            });
        }
    }

    /// <summary>
    /// Validates that resource group naming follows expected patterns (synchronous version)
    /// </summary>
    public ValidationResult ValidateResourceGroupNaming(InfrastructureSettings settings)
    {
        _logger.LogDebug("Validating resource group naming for settings");

        ArgumentNullException.ThrowIfNull(settings);

        var validationErrors = new List<string>();
        var validationWarnings = new List<string>();

        try
        {
            var actualResourceGroupName = settings.ResourceGroupName;
            var expectedName = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.ResourceGroup, settings.NamingPrefix, settings.Environment);

            if (string.IsNullOrWhiteSpace(actualResourceGroupName))
            {
                validationErrors.Add("Resource group name is null or empty");
            }
            else
            {
                // Check if it follows the expected pattern
                if (!actualResourceGroupName.Equals(expectedName, StringComparison.OrdinalIgnoreCase))
                {
                    // Check if it's a variation that might cause issues
                    if (actualResourceGroupName.Contains(settings.NamingPrefix) &&
                        actualResourceGroupName.Contains(settings.Environment))
                    {
                        validationWarnings.Add($"Resource group name '{actualResourceGroupName}' doesn't match expected pattern '{expectedName}' but contains required components");
                    }
                    else
                    {
                        validationErrors.Add($"Resource group name '{actualResourceGroupName}' doesn't match expected pattern '{expectedName}'");
                    }
                }

                // Validate length
                if (actualResourceGroupName.Length > 90)
                {
                    validationErrors.Add($"Resource group name '{actualResourceGroupName}' exceeds maximum length of 90 characters");
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
            _logger.LogError(ex, "Error validating resource group naming");
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"Resource group naming validation failed: {ex.Message}"]
            };
        }
    }

    /// <summary>
    /// Validates that resource group naming follows expected patterns
    /// </summary>
    public Task<ValidationResult> ValidateResourceGroupNamingAsync(InfrastructureSettings settings, string actualResourceGroupName)
    {
        _logger.LogDebug("Validating resource group naming. Expected pattern: {Pattern}, Actual: {Actual}",
            InfrastructureConstants.NamingPatterns.ResourceGroup, actualResourceGroupName);

        var validationErrors = new List<string>();
        var validationWarnings = new List<string>();

        try
        {
            var expectedName = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.ResourceGroup, settings.NamingPrefix, settings.Environment);

            if (string.IsNullOrWhiteSpace(actualResourceGroupName))
            {
                validationErrors.Add("Resource group name is null or empty");
            }
            else
            {
                // Check if it follows the expected pattern
                if (!actualResourceGroupName.Equals(expectedName, StringComparison.OrdinalIgnoreCase))
                {
                    // Check if it's a variation that might cause issues
                    if (actualResourceGroupName.Contains(settings.NamingPrefix) &&
                        actualResourceGroupName.Contains(settings.Environment))
                    {
                        validationWarnings.Add($"Resource group name '{actualResourceGroupName}' doesn't match expected pattern '{expectedName}' but contains required components");
                    }
                    else
                    {
                        validationErrors.Add($"Resource group name '{actualResourceGroupName}' doesn't match expected pattern '{expectedName}'");
                    }
                }

                // Validate length
                if (actualResourceGroupName.Length > 90)
                {
                    validationErrors.Add($"Resource group name '{actualResourceGroupName}' exceeds maximum length of 90 characters");
                }
            }

            return Task.FromResult(new ValidationResult
            {
                IsValid = validationErrors.Count == 0,
                Errors = validationErrors,
                Warnings = validationWarnings
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating resource group naming");
            return Task.FromResult(new ValidationResult
            {
                IsValid = false,
                Errors = [$"Resource group naming validation failed: {ex.Message}"]
            });
        }
    }

    #region Private Helper Methods

    private static bool IsValidNamingPrefix(string prefix) => !string.IsNullOrWhiteSpace(prefix) && Regex.IsMatch(prefix, @"^[a-z][a-z0-9]*$");

    private static bool IsValidEnvironment(string environment)
    {
        if (string.IsNullOrWhiteSpace(environment))
            return false;

        var allowedEnvironments = new[] { "dev", "development", "test", "staging", "prod", "production" };
        return allowedEnvironments.Contains(environment.ToLowerInvariant());
    }

    private static bool IsValidResourceName(string resourceType, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return resourceType.ToLowerInvariant() switch
        {
            "storageaccount" => Regex.IsMatch(name, @"^[a-z0-9]{3,24}$"),
            "keyvault" => Regex.IsMatch(name, @"^[a-zA-Z][a-zA-Z0-9-]{1,22}[a-zA-Z0-9]$"),
            "containerregistry" => Regex.IsMatch(name, @"^[a-zA-Z0-9]{5,50}$"),
            _ => true // Default to valid for other resource types
        };
    }

    private static int GetMaxNameLength(string resourceType)
    {
        return resourceType.ToLowerInvariant() switch
        {
            "storageaccount" => InfrastructureConstants.NamingPatterns.MaxStorageAccountNameLength,
            "keyvault" => InfrastructureConstants.NamingPatterns.MaxKeyVaultNameLength,
            "containerregistry" => InfrastructureConstants.NamingPatterns.MaxContainerRegistryNameLength,
            "resourcegroup" => 90,
            _ => 64 // Default max length for most Azure resources
        };
    }

    private static string GenerateStorageAccountName(string prefix, string environment)
    {
        // Storage account names must be lowercase and alphanumeric only
        var name = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.StorageAccount, prefix, environment);
        return name.Replace("-", "").ToLowerInvariant();
    }

    private static string GenerateKeyVaultName(string prefix, string environment)
    {
        // Key Vault names have specific requirements
        var name = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.KeyVault, prefix, environment);
        return name.Replace("-", "").ToLowerInvariant();
    }

    private static string GenerateContainerRegistryName(string prefix, string environment)
    {
        // Container Registry names must be alphanumeric only
        var name = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.ContainerRegistry, prefix, environment);
        return name.Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Validates that the naming prefix and environment combination is consistent (synchronous version)
    /// </summary>
    public ValidationResult ValidateNamingConsistency(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _logger.LogInformation("Starting naming consistency validation for environment: {Environment}, prefix: {Prefix}",
            settings.Environment, settings.NamingPrefix);

        var validationErrors = new List<string>();
        var validationWarnings = new List<string>();

        try
        {
            var envResult = ValidateEnvironmentNaming(settings);
            if (!envResult.IsValid)
            {
                validationErrors.AddRange(envResult.Errors);
                validationWarnings.AddRange(envResult.Warnings);
            }

            var rgResult = ValidateResourceGroupNaming(settings);
            if (!rgResult.IsValid)
            {
                validationErrors.AddRange(rgResult.Errors);
                validationWarnings.AddRange(rgResult.Warnings);
            }

            if (!IsValidNamingPrefix(settings.NamingPrefix))
            {
                validationErrors.Add($"Naming prefix '{settings.NamingPrefix}' is invalid. Must start with lowercase letter and contain only lowercase letters and numbers.");
            }

            if (!IsValidEnvironment(settings.Environment))
            {
                validationErrors.Add($"Environment '{settings.Environment}' is invalid. Must be one of: dev, test, staging, prod");
            }

            var expectedNames = GenerateExpectedResourceNames(settings);
            foreach (var (resourceType, expectedName) in expectedNames)
            {
                if (string.IsNullOrWhiteSpace(expectedName))
                {
                    validationErrors.Add($"Generated name for {resourceType} is empty or invalid");
                }
                else if (expectedName.Length > GetMaxNameLength(resourceType))
                {
                    validationErrors.Add($"Generated name for {resourceType} '{expectedName}' exceeds maximum length of {GetMaxNameLength(resourceType)} characters");
                }
            }

            var result = new ValidationResult
            {
                IsValid = validationErrors.Count == 0,
                Errors = validationErrors,
                Warnings = validationWarnings
            };

            _logger.LogInformation("Naming consistency validation completed. Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}",
                result.IsValid, result.Errors.Count, result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during synchronous naming consistency validation");
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"Naming consistency validation failed: {ex.Message}"]
            };
        }
    }

    /// <summary>
    /// Validates that environment naming is consistent (synchronous version)
    /// </summary>
    public ValidationResult ValidateEnvironmentNaming(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _logger.LogDebug("Validating environment naming consistency for: {Environment}", settings.Environment);

        var validationErrors = new List<string>();
        var validationWarnings = new List<string>();

        try
        {
            var environment = settings.Environment.ToLowerInvariant();

            if (environment.Contains("development") && !environment.Equals("dev"))
            {
                validationWarnings.Add("Environment contains 'development' but should use 'dev' for consistency");
            }

            if (environment.Contains("production") && !environment.Equals("prod"))
            {
                validationWarnings.Add("Environment contains 'production' but should use 'prod' for consistency");
            }

            var allowedEnvironments = new[] { "dev", "development", "test", "staging", "prod", "production" };
            if (!allowedEnvironments.Contains(environment))
            {
                validationErrors.Add($"Environment '{environment}' is not in allowed list: {string.Join(", ", allowedEnvironments)}");
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
            _logger.LogError(ex, "Error during synchronous environment naming validation");
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"Environment naming validation failed: {ex.Message}"]
            };
        }
    }

    /// <summary>
    /// Generates expected resource name for a specific resource type
    /// </summary>
    public string GenerateExpectedResourceName(InfrastructureSettings settings, string resourceType)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(resourceType))
            throw new ArgumentNullException(nameof(resourceType));

        try
        {
            var expectedNames = GenerateExpectedResourceNames(settings);

            // If the resource type is not in our predefined list, generate a generic name
            if (!expectedNames.TryGetValue(resourceType, out var name))
            {
                // For test purposes, generate a generic pattern
                name = $"{settings.NamingPrefix}-{settings.Environment}-{resourceType.ToLowerInvariant()}";
            }

            return name;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating expected resource name for {ResourceType}", resourceType);
            return string.Empty;
        }
    }

    private static Dictionary<string, string> GenerateExpectedResourceNames(InfrastructureSettings settings)
    {
        return new Dictionary<string, string>
        {
            ["ResourceGroup"] = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.ResourceGroup, settings.NamingPrefix, settings.Environment),
            ["PostgreSqlServer"] = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.PostgreSqlServer, settings.NamingPrefix, settings.Environment),
            ["PostgreSqlDatabase"] = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.PostgreSqlDatabase, settings.NamingPrefix, settings.Environment),
            ["EventHubsNamespace"] = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.EventHubsNamespace, settings.NamingPrefix, settings.Environment),
            ["VirtualNetwork"] = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.VirtualNetwork, settings.NamingPrefix, settings.Environment),
            ["StorageAccount"] = GenerateStorageAccountName(settings.NamingPrefix, settings.Environment),
            ["KeyVault"] = GenerateKeyVaultName(settings.NamingPrefix, settings.Environment),
            ["ContainerRegistry"] = GenerateContainerRegistryName(settings.NamingPrefix, settings.Environment),
            ["RedisCache"] = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.RedisCache, settings.NamingPrefix, settings.Environment),
            ["LogAnalyticsWorkspace"] = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.LogAnalyticsWorkspace, settings.NamingPrefix, settings.Environment),
            ["ApplicationInsights"] = string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.ApplicationInsights, settings.NamingPrefix, settings.Environment)
        };
    }

    #endregion
}


