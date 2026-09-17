using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using DeploymentKit.Interfaces;
using System.Collections.Concurrent;

namespace DeploymentKit.Services;

/// <summary>
/// Thread-safe service for tracking and managing resource names to prevent duplicates
/// Uses ConcurrentDictionary for high-performance concurrent operations
/// </summary>
public class ResourceNameRegistry(ILogger<ResourceNameRegistry> logger) : IResourceNameRegistry
{
    private readonly ConcurrentDictionary<string, ResourceRegistrationInfo> _registeredResources = new();
    private readonly ILogger<ResourceNameRegistry> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public bool RegisterResourceName(string resourceName, ResourceType resourceType, EnvironmentType environment, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            _logger.LogWarning("Attempted to register null or empty resource name. CorrelationId: {CorrelationId}", correlationId);
            return false;
        }

        var registrationKey = GenerateRegistrationKey(resourceName, resourceType, environment);
        var registrationInfo = new ResourceRegistrationInfo
        {
            ResourceName = resourceName,
            ResourceType = resourceType,
            Environment = environment,
            CorrelationId = correlationId,
            RegisteredAt = DateTime.UtcNow
        };

        var wasAdded = _registeredResources.TryAdd(registrationKey, registrationInfo);

        if (wasAdded)
        {
            _logger.LogInformation("Successfully registered resource name '{ResourceName}' of type '{ResourceType}' in environment '{Environment}'. CorrelationId: {CorrelationId}",
                resourceName, resourceType.ToStringValue(), environment.ToStringValue(), correlationId);
        }
        else
        {
            _logger.LogWarning("Failed to register resource name '{ResourceName}' of type '{ResourceType}' in environment '{Environment}' - already exists. CorrelationId: {CorrelationId}",
                resourceName, resourceType.ToStringValue(), environment.ToStringValue(), correlationId);
        }

        return wasAdded;
    }

    public bool RegisterResourceName(string resourceName, ResourceType resourceType, string environment, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            _logger.LogWarning("Attempted to register null or empty resource name. CorrelationId: {CorrelationId}", correlationId);
            return false;
        }

        var registrationKey = GenerateRegistrationKey(resourceName, resourceType, environment);
        var registrationInfo = new ResourceRegistrationInfo
        {
            ResourceName = resourceName,
            ResourceType = resourceType,
            Environment = environment,
            CorrelationId = correlationId,
            RegisteredAt = DateTime.UtcNow
        };

        var wasAdded = _registeredResources.TryAdd(registrationKey, registrationInfo);

        if (wasAdded)
        {
            _logger.LogInformation("Successfully registered resource name '{ResourceName}' of type '{ResourceType}' in environment '{Environment}'. CorrelationId: {CorrelationId}",
                resourceName, resourceType.ToStringValue(), environment, correlationId);
        }
        else
        {
            _logger.LogWarning("Failed to register resource name '{ResourceName}' of type '{ResourceType}' in environment '{Environment}' - already exists. CorrelationId: {CorrelationId}",
                resourceName, resourceType.ToStringValue(), environment, correlationId);
        }

        return wasAdded;
    }

    public bool IsResourceNameRegistered(string resourceName, ResourceType resourceType, EnvironmentType environment)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            return false;
        }

        var registrationKey = GenerateRegistrationKey(resourceName, resourceType, environment);
        return _registeredResources.ContainsKey(registrationKey);
    }

    public bool IsResourceNameRegistered(string resourceName, ResourceType resourceType, string environment)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            return false;
        }

        var registrationKey = GenerateRegistrationKey(resourceName, resourceType, environment);
        return _registeredResources.ContainsKey(registrationKey);
    }

    public bool UnregisterResourceName(string resourceName, ResourceType resourceType, EnvironmentType environment)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            _logger.LogWarning("Attempted to unregister null or empty resource name");
            return false;
        }

        var registrationKey = GenerateRegistrationKey(resourceName, resourceType, environment);
        var wasRemoved = _registeredResources.TryRemove(registrationKey, out var removedInfo);

        if (wasRemoved && removedInfo != null)
        {
            _logger.LogInformation("Successfully unregistered resource name '{ResourceName}' of type '{ResourceType}' in environment '{Environment}'. CorrelationId: {CorrelationId}",
                resourceName, resourceType.ToStringValue(), environment.ToStringValue(), removedInfo.CorrelationId);
        }
        else
        {
            _logger.LogWarning("Failed to unregister resource name '{ResourceName}' of type '{ResourceType}' in environment '{Environment}' - not found",
                resourceName, resourceType.ToStringValue(), environment.ToStringValue());
        }

        return wasRemoved;
    }

    public bool UnregisterResourceName(string resourceName, ResourceType resourceType, string environment)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            _logger.LogWarning("Attempted to unregister null or empty resource name");
            return false;
        }

        var registrationKey = GenerateRegistrationKey(resourceName, resourceType, environment);
        var wasRemoved = _registeredResources.TryRemove(registrationKey, out var removedInfo);

        if (wasRemoved && removedInfo != null)
        {
            _logger.LogInformation("Successfully unregistered resource name '{ResourceName}' of type '{ResourceType}' in environment '{Environment}'. CorrelationId: {CorrelationId}",
                resourceName, resourceType.ToStringValue(), environment, removedInfo.CorrelationId);
        }
        else
        {
            _logger.LogWarning("Failed to unregister resource name '{ResourceName}' of type '{ResourceType}' in environment '{Environment}' - not found",
                resourceName, resourceType.ToStringValue(), environment);
        }

        return wasRemoved;
    }

    public IEnumerable<string> GetRegisteredResourceNames(ResourceType resourceType, EnvironmentType environment)
    {
        return _registeredResources.Values
            .Where(info => info.ResourceType == resourceType &&
                          (info.Environment is EnvironmentType env && env == environment))
            .Select(info => info.ResourceName)
            .ToList();
    }

    public IEnumerable<string> GetRegisteredResourceNames(ResourceType resourceType, string environment)
    {
        return _registeredResources.Values
            .Where(info => info.ResourceType == resourceType &&
                          (info.Environment is string envStr && envStr == environment))
            .Select(info => info.ResourceName)
            .ToList();
    }

    public void ClearEnvironmentRegistry(EnvironmentType environment)
    {
        var keysToRemove = _registeredResources
            .Where(kvp => kvp.Value.Environment is EnvironmentType env && env == environment)
            .Select(kvp => kvp.Key)
            .ToList();

        var removedCount = keysToRemove.Count(key => _registeredResources.TryRemove(key, out _));

        _logger.LogInformation("Cleared {RemovedCount} registered resources from environment '{Environment}'", removedCount, environment.ToStringValue());
    }

    public void ClearEnvironmentRegistry(string environment)
    {
        var keysToRemove = _registeredResources
            .Where(kvp => kvp.Value.Environment is string envStr && envStr == environment)
            .Select(kvp => kvp.Key)
            .ToList();

        var removedCount = keysToRemove.Count(key => _registeredResources.TryRemove(key, out _));

        _logger.LogInformation("Cleared {RemovedCount} registered resources from environment '{Environment}'", removedCount, environment);
    }

    public int GetTotalRegisteredResourcesCount()
    {
        return _registeredResources.Count;
    }

    private static string GenerateRegistrationKey(string resourceName, ResourceType resourceType, EnvironmentType environment)
    {
        return $"{resourceType.ToStringValue()}:{environment.ToStringValue()}:{resourceName.ToLowerInvariant()}";
    }

    private static string GenerateRegistrationKey(string resourceName, ResourceType resourceType, string environment)
    {
        return $"{resourceType.ToStringValue()}:{environment}:{resourceName.ToLowerInvariant()}";
    }

    /// <summary>
    /// Internal class to store resource registration information
    /// </summary>
    private sealed class ResourceRegistrationInfo
    {
        public required string ResourceName { get; init; }
        public required ResourceType ResourceType { get; init; }
        public required object Environment { get; init; } // Changed to object to support both enum and string
        public string? CorrelationId { get; init; }
        public DateTime RegisteredAt { get; init; }
    }
}

