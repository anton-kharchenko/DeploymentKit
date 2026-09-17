using DeploymentKit.Enums;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for tracking and managing resource names to prevent duplicates
/// Provides thread-safe operations for registering and validating resource names across deployments
/// </summary>
public interface IResourceNameRegistry
{
    /// <summary>
    /// Registers a resource name in the registry to track its usage
    /// </summary>
    /// <param name="resourceName">The name of the resource to register</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment</param>
    /// <param name="correlationId">Optional correlation ID for tracking</param>
    /// <returns>True if registration was successful, false if name already exists</returns>
    bool RegisterResourceName(string resourceName, ResourceType resourceType, EnvironmentType environment, string? correlationId = null);

    /// <summary>
    /// Registers a resource name in the registry to track its usage
    /// </summary>
    /// <param name="resourceName">The name of the resource to register</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment as string</param>
    /// <param name="correlationId">Optional correlation ID for tracking</param>
    /// <returns>True if registration was successful, false if name already exists</returns>
    bool RegisterResourceName(string resourceName, ResourceType resourceType, string environment, string? correlationId = null);

    /// <summary>
    /// Checks if a resource name is already registered
    /// </summary>
    /// <param name="resourceName">The name to check for existence</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment</param>
    /// <returns>True if the name is already registered, false otherwise</returns>
    bool IsResourceNameRegistered(string resourceName, ResourceType resourceType, EnvironmentType environment);

    /// <summary>
    /// Checks if a resource name is already registered
    /// </summary>
    /// <param name="resourceName">The name to check for existence</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment as string</param>
    /// <returns>True if the name is already registered, false otherwise</returns>
    bool IsResourceNameRegistered(string resourceName, ResourceType resourceType, string environment);

    /// <summary>
    /// Unregisters a resource name from the registry
    /// </summary>
    /// <param name="resourceName">The name of the resource to unregister</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment</param>
    /// <returns>True if unregistration was successful, false if name was not found</returns>
    bool UnregisterResourceName(string resourceName, ResourceType resourceType, EnvironmentType environment);

    /// <summary>
    /// Unregisters a resource name from the registry
    /// </summary>
    /// <param name="resourceName">The name of the resource to unregister</param>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment as string</param>
    /// <returns>True if unregistration was successful, false if name was not found</returns>
    bool UnregisterResourceName(string resourceName, ResourceType resourceType, string environment);

    /// <summary>
    /// Gets all registered resource names for a specific resource type and environment
    /// </summary>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment</param>
    /// <returns>Collection of registered resource names</returns>
    IEnumerable<string> GetRegisteredResourceNames(ResourceType resourceType, EnvironmentType environment);

    /// <summary>
    /// Gets all registered resource names for a specific resource type and environment
    /// </summary>
    /// <param name="resourceType">The type of Azure resource</param>
    /// <param name="environment">The target environment as string</param>
    /// <returns>Collection of registered resource names</returns>
    IEnumerable<string> GetRegisteredResourceNames(ResourceType resourceType, string environment);

    /// <summary>
    /// Clears all registered resource names for a specific environment
    /// </summary>
    /// <param name="environment">The target environment to clear</param>
    void ClearEnvironmentRegistry(EnvironmentType environment);

    /// <summary>
    /// Clears all registered resource names for a specific environment
    /// </summary>
    /// <param name="environment">The target environment to clear as string</param>
    void ClearEnvironmentRegistry(string environment);

    /// <summary>
    /// Gets the total count of registered resources across all types and environments
    /// </summary>
    /// <returns>Total number of registered resources</returns>
    int GetTotalRegisteredResourcesCount();
}
