using DeploymentKit.Constants;
using DeploymentKit.Exceptions;
using DeploymentKit.Settings;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DeploymentKit.Utilities
{
    /// <summary>
    /// Provides validation for infrastructure configuration settings
    /// </summary>
    public static class ConfigurationValidator
    {
        /// <summary>
        /// Validates infrastructure settings using data annotations
        /// </summary>
        /// <param name="settings">The infrastructure settings to validate</param>
        /// <param name="logger">Optional logger for validation messages</param>
        /// <param name="throwOnError">Whether to throw exceptions on validation errors</param>
        /// <returns>True if validation passes, false otherwise</returns>
        /// <exception cref="ConfigurationValidationException">Thrown when critical validation fails</exception>
        /// <exception cref="ArgumentNullException">Thrown when settings is null</exception>
        public static bool ValidateSettings(InfrastructureSettings settings, ILogger? logger = null, bool throwOnError = false)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(settings);

            var isValid = Validator.TryValidateObject(settings, validationContext, validationResults, true);

            if (!isValid)
            {
                logger?.LogError("Infrastructure settings validation failed:");
                var errors = new List<string>();

                foreach (var result in validationResults)
                {
                    logger?.LogError("- {ValidationError}", result.ErrorMessage);
                    errors.Add(result.ErrorMessage ?? "Unknown validation error");
                }

                if (throwOnError)
                {
                    throw new ConfigurationValidationException(
                        $"Infrastructure settings validation failed: {string.Join(", ", errors)}",
                        "InfrastructureSettings",
                        "DataAnnotationValidation");
                }
            }
            else
            {
                logger?.LogInformation("Infrastructure settings validation passed");
            }

            return isValid;
        }

        /// <summary>
        /// Validates Azure resource naming conventions with enhanced error handling
        /// </summary>
        /// <param name="settings">The infrastructure settings to validate</param>
        /// <param name="logger">Optional logger for validation messages</param>
        /// <param name="throwOnError">Whether to throw exceptions on validation errors</param>
        /// <returns>True if naming conventions are valid, false otherwise</returns>
        /// <exception cref="ConfigurationValidationException">Thrown when critical naming validation fails</exception>
        /// <exception cref="ArgumentNullException">Thrown when settings is null</exception>
        public static bool ValidateNamingConventions(InfrastructureSettings settings, ILogger? logger = null, bool throwOnError = false)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var isValid = true;
            var errors = new List<string>();

            // Validate naming prefix
            if (string.IsNullOrWhiteSpace(settings.NamingPrefix))
            {
                var error = "Naming prefix cannot be empty";
                logger?.LogError(error);
                errors.Add(error);
                isValid = false;
            }
            else if (settings.NamingPrefix.Length > 20)
            {
                var error = "Naming prefix cannot exceed 20 characters";
                logger?.LogError(error);
                errors.Add(error);
                isValid = false;
            }
            else if (!IsValidAzureResourceName(settings.NamingPrefix))
            {
                var error = "Naming prefix must start with a lowercase letter and contain only lowercase letters and numbers";
                logger?.LogError(error);
                errors.Add(error);
                isValid = false;
            }

            // Validate environment
            if (string.IsNullOrWhiteSpace(settings.Environment))
            {
                var error = "Environment cannot be empty";
                logger?.LogError(error);
                errors.Add(error);
                isValid = false;
            }
            else if (settings.Environment.Length < 3)
            {
                const string error = "Environment must be at least 3 characters long";
                logger?.LogError(error);
                errors.Add(error);
                isValid = false;
            }
            else if (settings.Environment.Length > 10)
            {
                const string error = "Environment cannot exceed 10 characters";
                logger?.LogError(error);
                errors.Add(error);
                isValid = false;
            }
            else if (!Regex.IsMatch(settings.Environment, @"^[a-zA-Z0-9]+$"))
            {
                const string error = "Environment can only contain alphanumeric characters";
                logger?.LogError(error);
                errors.Add(error);
                isValid = false;
            }
            else if (!InfrastructureConstants.Validation.ValidEnvironments.Contains(settings.Environment.ToLowerInvariant()))
            {
                logger?.LogWarning("Environment '{Environment}' is not a standard environment name. Consider using: {ValidEnvironments}",
                    settings.Environment, string.Join(", ", InfrastructureConstants.Validation.ValidEnvironments));
            }

            // Validate prefix impact on resource-specific name lengths
            if (isValid)
            {
                var storageLen = ComputeSanitizedNameLength(InfrastructureConstants.NamingPatterns.StorageAccount, settings.NamingPrefix, settings.Environment);
                if (storageLen > InfrastructureConstants.NamingPatterns.MaxStorageAccountNameLength)
                {
                    var error = $"Computed Storage Account name length ({storageLen}) exceeds maximum allowed ({InfrastructureConstants.NamingPatterns.MaxStorageAccountNameLength}). Consider reducing NamingPrefix length.";
                    logger?.LogError(error);
                    errors.Add(error);
                    isValid = false;
                }

                var keyVaultLen = ComputeSanitizedNameLength(InfrastructureConstants.NamingPatterns.KeyVault, settings.NamingPrefix, settings.Environment);
                if (keyVaultLen > InfrastructureConstants.NamingPatterns.MaxKeyVaultNameLength)
                {
                    var error = $"Computed Key Vault name length ({keyVaultLen}) exceeds maximum allowed ({InfrastructureConstants.NamingPatterns.MaxKeyVaultNameLength}). Consider reducing NamingPrefix length.";
                    logger?.LogError(error);
                    errors.Add(error);
                    isValid = false;
                }

                var acrLen = ComputeSanitizedNameLength(InfrastructureConstants.NamingPatterns.ContainerRegistry, settings.NamingPrefix, settings.Environment);
                if (acrLen > InfrastructureConstants.NamingPatterns.MaxContainerRegistryNameLength)
                {
                    var error = $"Computed Container Registry name length ({acrLen}) exceeds maximum allowed ({InfrastructureConstants.NamingPatterns.MaxContainerRegistryNameLength}). Consider reducing NamingPrefix length.";
                    logger?.LogError(error);
                    errors.Add(error);
                    isValid = false;
                }
            }

            if (!isValid && throwOnError)
            {
                throw new ConfigurationValidationException(
                    $"Naming convention validation failed: {string.Join(", ", errors)}",
                    "NamingConventions",
                    "AzureResourceNaming");
            }

            return isValid;
        }

        /// <summary>
        /// Validates Azure location/region with enhanced error handling
        /// </summary>
        /// <param name="location">The Azure location to validate</param>
        /// <param name="logger">Optional logger for validation messages</param>
        /// <param name="throwOnError">Whether to throw exceptions on validation errors</param>
        /// <returns>True if location is valid, false otherwise</returns>
        /// <exception cref="ConfigurationValidationException">Thrown when location validation fails</exception>
        /// <exception cref="ArgumentNullException">Thrown when location is null</exception>
        public static bool ValidateAzureLocation(string location, ILogger? logger = null, bool throwOnError = false)
        {
            ArgumentNullException.ThrowIfNull(location);

            if (string.IsNullOrWhiteSpace(location))
            {
                const string error = "Azure location cannot be empty";
                logger?.LogError(error);

                return throwOnError ? throw new ConfigurationValidationException(error, "AzureLocationType", "LocationRequired") : false;
            }

            if (InfrastructureConstants.Validation.ValidAzureLocations.Contains(location))
            {
                return true;
            }

            var warning = $"Location '{location}' may not be a valid Azure region";
            logger?.LogWarning(warning);

            return throwOnError ? throw new ConfigurationValidationException(warning, "AzureLocationType", "InvalidRegion") : false;

        }

        /// <summary>
        /// Validates resource limits and quotas with enhanced error handling
        /// </summary>
        /// <param name="settings">The infrastructure settings to validate</param>
        /// <param name="logger">Optional logger for validation messages</param>
        /// <param name="throwOnError">Whether to throw exceptions on validation errors</param>
        /// <returns>True if resource limits are valid, false otherwise</returns>
        /// <exception cref="ConfigurationValidationException">Thrown when resource limit validation fails</exception>
        /// <exception cref="ArgumentNullException">Thrown when settings is null</exception>
        public static bool ValidateResourceLimits(InfrastructureSettings settings, ILogger? logger = null, bool throwOnError = false)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var errors = new List<string>();

            // Validate database storage size
            if (settings.Database is { StorageSizeGb: < 32 or > 32767 })
            {
                const string error = "Database storage size must be between 32GB and 32TB";
                logger?.LogError(error);
                errors.Add(error);
            }
            else if (settings.Database != null && !InfrastructureConstants.Database.ValidStorageSizesGb.Contains(settings.Database.StorageSizeGb))
            {
                var closestValidSize = InfrastructureConstants.Database.ValidStorageSizesGb
                    .OrderBy(size => Math.Abs(size - settings.Database.StorageSizeGb))
                    .First();
                var error = $"Database storage size {settings.Database.StorageSizeGb}GB is not a valid Azure PostgreSQL size. Use one of: {string.Join(", ", InfrastructureConstants.Database.ValidStorageSizesGb)}GB. Closest valid size: {closestValidSize}GB";
                logger?.LogError(error);
                errors.Add(error);
            }

            // Validate container resource limits
            if (settings.Container is { CpuLimit: <= 0 or > 4 })
            {
                const string warning = "Container CPU limit should be between 0.1 and 4.0 cores";
                logger?.LogWarning(warning);
            }

            // Validate log retention
            if (settings.Monitoring is { LogRetentionDays: < 30 or > 730 })
            {
                const string warning = "Log retention should be between 30 and 730 days for optimal cost/compliance balance";
                logger?.LogWarning(warning);
            }

            if (errors.Count != 0 && throwOnError)
            {
                throw new ConfigurationValidationException(
                    $"Resource limit validation failed: {string.Join(", ", errors)}",
                    "ResourceLimits",
                    "ResourceQuotaValidation");
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// Performs comprehensive validation of all configuration aspects
        /// </summary>
        /// <param name="settings">The infrastructure settings to validate</param>
        /// <param name="logger">Optional logger for validation messages</param>
        /// <param name="throwOnError">Whether to throw exceptions on validation errors</param>
        /// <returns>True if all validations pass, false otherwise</returns>
        /// <exception cref="ConfigurationValidationException">Thrown when any critical validation fails</exception>
        /// <exception cref="ArgumentNullException">Thrown when settings is null</exception>
        public static bool ValidateAllSettings(InfrastructureSettings settings, ILogger? logger = null, bool throwOnError = false)
        {
            ArgumentNullException.ThrowIfNull(settings);

            try
            {
                logger?.LogInformation("Starting comprehensive configuration validation");

                var isValid = true;

                // Perform all validations
                isValid &= ValidateSettings(settings, logger, throwOnError);
                isValid &= ValidateNamingConventions(settings, logger, throwOnError);
                isValid &= ValidateAzureLocation(settings.Location, logger, throwOnError);
                isValid &= ValidateResourceLimits(settings, logger, throwOnError);
                isValid &= ValidateNetworkCidrs(settings.Network, logger);

                logger?.LogInformation("Configuration validation completed. Valid: {IsValid}", isValid);
                return isValid;
            }
            catch (ConfigurationValidationException)
            {
                throw; // Re-throw configuration validation exceptions
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Unexpected error during configuration validation");

                if (throwOnError)
                {
                    throw new ConfigurationValidationException(
                        "Unexpected error during configuration validation",
                        ex,
                        "ConfigurationValidation",
                        "UnexpectedError");
                }
                return false;
            }
        }

        /// <summary>
        /// Checks if a string is a valid Azure resource name
        /// </summary>
        private static bool IsValidAzureResourceName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Azure resource names must be at least 3 characters long
            return name.Length >= 3 &&
                   // Must start with a lowercase letter and contain only lowercase letters and numbers
                   // This matches the RegularExpression validation in InfrastructureSettings
                   Regex.IsMatch(name, @"^[a-z][a-z0-9]*$");
        }

        /// <summary>
        /// Computes sanitized resource name length using the specified pattern and environment, consistent with ResourceNamingService.
        /// </summary>
        private static int ComputeSanitizedNameLength(string pattern, string prefix, string environment) => Regex.Replace(string.Format(System.Globalization.CultureInfo.InvariantCulture, pattern, prefix, environment), @"[^a-zA-Z0-9]", "").ToLowerInvariant().Length;

        /// <summary>
        /// Validates if a CIDR notation is properly aligned for its subnet mask.
        /// For example, 10.0.1.0/23 is invalid (should be 10.0.0.0/23), but 10.0.0.0/23 is valid.
        /// </summary>
        private static bool IsValidCidrAlignment(string cidr)
        {
            var parts = cidr.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[1], out var prefixLength))
                return false;

            var ipParts = parts[0].Split('.');
            if (ipParts.Length != 4)
                return false;

            // Convert IP to integer
            long ipValue = 0;
            for (var i = 0; i < 4; i++)
            {
                if (!int.TryParse(ipParts[i], out var octet) || octet < 0 || octet > 255)
                    return false;
                ipValue = (ipValue << 8) | (byte)octet;
            }

            // Calculate the network mask
            var mask = prefixLength == 0 ? 0 : ~((1L << (32 - prefixLength)) - 1) & 0xFFFFFFFFL;

            // Check if IP is aligned with the mask (network address)
            return (ipValue & mask) == ipValue;
        }

        /// <summary>
        /// Validates network CIDR configurations
        /// </summary>
        private static bool ValidateNetworkCidrs(NetworkSettings? network, ILogger? logger)
        {
            if (network == null)
                return true;

            var isValid = true;

            if (!string.IsNullOrEmpty(network.ContainerAppsSubnet) && !IsValidCidrAlignment(network.ContainerAppsSubnet))
            {
                logger?.LogError("Container Apps subnet {Cidr} is not properly aligned for its mask. For /23, use even third octets (e.g., 10.0.0.0/23, 10.0.2.0/23)", network.ContainerAppsSubnet);
                isValid = false;
            }

            if (!string.IsNullOrEmpty(network.DatabaseSubnet) && !IsValidCidrAlignment(network.DatabaseSubnet))
            {
                logger?.LogError("Database subnet {Cidr} is not properly aligned for its mask", network.DatabaseSubnet);
                isValid = false;
            }

            if (!string.IsNullOrEmpty(network.ApplicationGatewaySubnet) && !IsValidCidrAlignment(network.ApplicationGatewaySubnet))
            {
                logger?.LogError("Application Gateway subnet {Cidr} is not properly aligned for its mask", network.ApplicationGatewaySubnet);
                isValid = false;
            }

            if (!string.IsNullOrEmpty(network.PrivateEndpointsSubnet) && !IsValidCidrAlignment(network.PrivateEndpointsSubnet))
            {
                logger?.LogError("Private Endpoints subnet {Cidr} is not properly aligned for its mask", network.PrivateEndpointsSubnet);
                isValid = false;
            }

            return isValid;
        }
    }
}


