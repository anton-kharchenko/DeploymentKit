using DeploymentKit.Models.Results;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Interface for validating naming consistency across Azure resources
/// Helps prevent resource not found errors due to naming prefix/environment mismatches
/// </summary>
public interface INamingConsistencyValidator
{
    /// <summary>
    /// Validates that the naming prefix and environment combination is consistent
    /// </summary>
    /// <param name="settings">Infrastructure settings containing naming configuration</param>
    /// <returns>Validation result with any naming inconsistencies found</returns>
    Task<ValidationResult> ValidateNamingConsistencyAsync(InfrastructureSettings settings);

    /// <summary>
    /// Validates that generated resource names match expected patterns (synchronous version)
    /// </summary>
    /// <param name="resourceName">Resource name to validate</param>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="resourceType">Type of resource being validated</param>
    /// <returns>Validation result indicating if name matches expected pattern</returns>
    ValidationResult ValidateResourceName(string resourceName, InfrastructureSettings settings, string resourceType);

    /// <summary>
    /// Validates that generated resource names match expected patterns
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="resourceType">Type of resource being validated</param>
    /// <param name="expectedName">Expected resource name</param>
    /// <param name="actualName">Actual resource name from Azure</param>
    /// <returns>Validation result indicating if names match</returns>
    Task<ValidationResult> ValidateResourceNameAsync(
        InfrastructureSettings settings,
        string resourceType,
        string expectedName,
        string actualName);

    /// <summary>
    /// Generates expected resource names based on current settings
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <returns>Dictionary of resource types to expected names</returns>
    Task<Dictionary<string, string>> GenerateExpectedResourceNamesAsync(InfrastructureSettings settings);

    /// <summary>
    /// Validates that environment naming is consistent (dev vs development)
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <returns>Validation result for environment naming consistency</returns>
    Task<ValidationResult> ValidateEnvironmentNamingAsync(InfrastructureSettings settings);

    /// <summary>
    /// Validates that environment naming is consistent (synchronous version)
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <returns>Validation result for environment naming consistency</returns>
    ValidationResult ValidateEnvironmentNaming(InfrastructureSettings settings);

    /// <summary>
    /// Validates that resource group naming follows expected patterns
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="actualResourceGroupName">Actual resource group name from Azure</param>
    /// <returns>Validation result for resource group naming</returns>
    Task<ValidationResult> ValidateResourceGroupNamingAsync(
        InfrastructureSettings settings,
        string actualResourceGroupName);

    /// <summary>
    /// Validates that resource group naming follows expected patterns (synchronous version)
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <returns>Validation result for resource group naming</returns>
    ValidationResult ValidateResourceGroupNaming(InfrastructureSettings settings);

    /// <summary>
    /// Validates that the naming prefix and environment combination is consistent (synchronous version)
    /// </summary>
    /// <param name="settings">Infrastructure settings containing naming configuration</param>
    /// <returns>Validation result with any naming inconsistencies found</returns>
    ValidationResult ValidateNamingConsistency(InfrastructureSettings settings);

    /// <summary>
    /// Generates expected resource name for a specific resource type
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="resourceType">Type of resource to generate name for</param>
    /// <returns>Expected resource name</returns>
    string GenerateExpectedResourceName(InfrastructureSettings settings, string resourceType);
}

