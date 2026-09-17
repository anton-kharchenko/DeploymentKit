using DeploymentKit.Enums;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for validating resource names and detecting duplicates before resource creation
/// Provides comprehensive validation logic to prevent naming conflicts in Azure deployments
/// </summary>
public interface IResourceNameValidator
{
    /// <summary>
    /// Validates that a resource name is unique and available for use
    /// </summary>
    /// <param name="resourceName">The name to validate</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment</param>
    /// <param name="correlationId">Optional correlation ID for tracking</param>
    /// <returns>True if the name is valid and available, false otherwise</returns>
    bool ValidateResourceNameUniqueness(string resourceName, ResourceType resourceType, EnvironmentType environment, string? correlationId = null);

    /// <summary>
    /// Validates that a resource name is unique and available for use
    /// </summary>
    /// <param name="resourceName">The name to validate</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment as string</param>
    /// <param name="correlationId">Optional correlation ID for tracking</param>
    /// <returns>True if the name is valid and available, false otherwise</returns>
    bool ValidateResourceNameUniqueness(string resourceName, ResourceType resourceType, string environment, string? correlationId = null);

    /// <summary>
    /// Validates a resource name and throws an exception if it's already in use
    /// </summary>
    /// <param name="resourceName">The name to validate</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment</param>
    /// <param name="correlationId">Optional correlation ID for tracking</param>
    /// <exception cref="Exceptions.DuplicateResourceNameException">Thrown when the resource name is already in use</exception>
    void ValidateAndThrowIfDuplicate(string resourceName, ResourceType resourceType, EnvironmentType environment, string? correlationId = null);

    /// <summary>
    /// Validates a resource name and throws an exception if it's already in use
    /// </summary>
    /// <param name="resourceName">The name to validate</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment as string</param>
    /// <param name="correlationId">Optional correlation ID for tracking</param>
    /// <exception cref="Exceptions.DuplicateResourceNameException">Thrown when the resource name is already in use</exception>
    void ValidateAndThrowIfDuplicate(string resourceName, ResourceType resourceType, string environment, string? correlationId = null);

    /// <summary>
    /// Validates multiple resource names in a batch operation
    /// </summary>
    /// <param name="resourceNames">Collection of resource names to validate</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment</param>
    /// <param name="correlationId">Optional correlation ID for tracking</param>
    /// <returns>Dictionary with resource names as keys and validation results as values</returns>
    Dictionary<string, bool> ValidateBatchResourceNames(IEnumerable<string> resourceNames, ResourceType resourceType, EnvironmentType environment, string? correlationId = null);

    /// <summary>
    /// Validates multiple resource names in a batch operation
    /// </summary>
    /// <param name="resourceNames">Collection of resource names to validate</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment as string</param>
    /// <param name="correlationId">Optional correlation ID for tracking</param>
    /// <returns>Dictionary with resource names as keys and validation results as values</returns>
    Dictionary<string, bool> ValidateBatchResourceNames(IEnumerable<string> resourceNames, ResourceType resourceType, string environment, string? correlationId = null);

    /// <summary>
    /// Suggests alternative names when a conflict is detected
    /// </summary>
    /// <param name="baseResourceName">The original name that caused a conflict</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment</param>
    /// <param name="maxSuggestions">Maximum number of alternative names to suggest</param>
    /// <returns>Collection of suggested alternative names</returns>
    IEnumerable<string> SuggestAlternativeNames(string baseResourceName, ResourceType resourceType, EnvironmentType environment, int maxSuggestions = 5);

    /// <summary>
    /// Suggests alternative names when a conflict is detected
    /// </summary>
    /// <param name="baseResourceName">The original name that caused a conflict</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment as string</param>
    /// <param name="maxSuggestions">Maximum number of alternative names to suggest</param>
    /// <returns>Collection of suggested alternative names</returns>
    IEnumerable<string> SuggestAlternativeNames(string baseResourceName, ResourceType resourceType, string environment, int maxSuggestions = 5);

    /// <summary>
    /// Registers a validated resource name to prevent future conflicts
    /// </summary>
    /// <param name="resourceName">The name to register after successful validation</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment</param>
    /// <param name="correlationId">Optional correlation ID for tracking</param>
    /// <returns>True if registration was successful</returns>
    bool RegisterValidatedResourceName(string resourceName, ResourceType resourceType, EnvironmentType environment, string? correlationId = null);

    /// <summary>
    /// Registers a validated resource name to prevent future conflicts
    /// </summary>
    /// <param name="resourceName">The name to register after successful validation</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment as string</param>
    /// <param name="correlationId">Optional correlation ID for tracking</param>
    /// <returns>True if registration was successful</returns>
    bool RegisterValidatedResourceName(string resourceName, ResourceType resourceType, string environment, string? correlationId = null);
}
