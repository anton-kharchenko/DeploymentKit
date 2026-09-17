using DeploymentKit.Enums;
using DeploymentKit.Exceptions;
using DeploymentKit.Extensions;
using DeploymentKit.Interfaces;
using ResourceType = DeploymentKit.Enums.ResourceType;

namespace DeploymentKit.Services;

/// <summary>
/// Service for validating resource names and detecting duplicates before resource creation
/// Provides comprehensive validation logic to prevent naming conflicts in Azure deployments
/// </summary>
public class ResourceNameValidationService(
    IResourceNameRegistry resourceNameRegistry,
    ILogger<ResourceNameValidationService> logger)
    : IResourceNameValidator
{
    private readonly IResourceNameRegistry _resourceNameRegistry = resourceNameRegistry ?? throw new ArgumentNullException(nameof(resourceNameRegistry));
    private readonly ILogger<ResourceNameValidationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public bool ValidateResourceNameUniqueness(string resourceName, ResourceType resourceType, EnvironmentType environment, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            _logger.LogWarning("Resource name validation failed: null or empty name provided. CorrelationId: {CorrelationId}", correlationId);
            return false;
        }

        var isRegistered = _resourceNameRegistry.IsResourceNameRegistered(resourceName, resourceType, environment);

        if (isRegistered)
        {
            _logger.LogWarning("Resource name validation failed: '{ResourceName}' of type '{ResourceType}' already exists in environment '{Environment}'. CorrelationId: {CorrelationId}",
                resourceName, resourceType.ToStringValue(), environment.ToStringValue(), correlationId);
        }
        else
        {
            _logger.LogDebug("Resource name validation passed: '{ResourceName}' of type '{ResourceType}' is available in environment '{Environment}'. CorrelationId: {CorrelationId}",
                resourceName, resourceType.ToStringValue(), environment.ToStringValue(), correlationId);
        }

        return !isRegistered;
    }

    public void ValidateAndThrowIfDuplicate(string resourceName, ResourceType resourceType, EnvironmentType environment, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            throw new ArgumentException("Resource name cannot be null or empty", nameof(resourceName));
        }

        if (_resourceNameRegistry.IsResourceNameRegistered(resourceName, resourceType, environment))
        {
            var errorMessage = $"Resource name '{resourceName}' of type '{resourceType.ToStringValue()}' already exists in environment '{environment.ToStringValue()}'";

            _logger.LogError("Duplicate resource name detected: {ErrorMessage}. CorrelationId: {CorrelationId}", errorMessage, correlationId);

            throw new DuplicateResourceNameException(
                errorMessage,
                resourceName,
                resourceType.ToStringValue(),
                environment.ToStringValue(),
                resourceName,
                resourceName,
                correlationId);
        }

        _logger.LogDebug("Resource name validation passed for '{ResourceName}' of type '{ResourceType}' in environment '{Environment}'. CorrelationId: {CorrelationId}",
            resourceName, resourceType.ToStringValue(), environment.ToStringValue(), correlationId);
    }

    public Dictionary<string, bool> ValidateBatchResourceNames(IEnumerable<string> resourceNames, ResourceType resourceType, EnvironmentType environment, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(resourceNames);

        var results = new Dictionary<string, bool>();
        var namesList = resourceNames.ToList();

        _logger.LogInformation("Starting batch validation for {Count} resource names of type '{ResourceType}' in environment '{Environment}'. CorrelationId: {CorrelationId}",
            namesList.Count, resourceType.ToStringValue(), environment.ToStringValue(), correlationId);

        foreach (var resourceName in namesList)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                results[resourceName] = false;
                continue;
            }

            var isValid = ValidateResourceNameUniqueness(resourceName, resourceType, environment, correlationId);
            results[resourceName] = isValid;
        }

        var validCount = results.Values.Count(v => v);
        var invalidCount = results.Count - validCount;

        _logger.LogInformation("Batch validation completed: {ValidCount} valid, {InvalidCount} invalid resource names. CorrelationId: {CorrelationId}",
            validCount, invalidCount, correlationId);

        return results;
    }

    public IEnumerable<string> SuggestAlternativeNames(string baseResourceName, ResourceType resourceType, EnvironmentType environment, int maxSuggestions = 5)
    {
        if (string.IsNullOrWhiteSpace(baseResourceName))
        {
            throw new ArgumentException("Base resource name cannot be null or empty", nameof(baseResourceName));
        }

        if (maxSuggestions < 0)
        {
            throw new ArgumentException("Max suggestions must be greater than or equal to zero", nameof(maxSuggestions));
        }

        if (maxSuggestions == 0)
        {
            return new List<string>();
        }

        var suggestions = new List<string>();
        var baseName = baseResourceName.Trim();

        _logger.LogDebug("Generating alternative names for '{BaseResourceName}' of type '{ResourceType}' in environment '{Environment}'",
            baseName, resourceType.ToStringValue(), environment.ToStringValue());

        // Strategy 1: Append numeric suffixes
        for (var i = 1; i <= maxSuggestions && suggestions.Count < maxSuggestions; i++)
        {
            var suggestion = $"{baseName}-{i:D2}";
            if (ValidateResourceNameUniqueness(suggestion, resourceType, environment))
            {
                suggestions.Add(suggestion);
            }
        }

        // Strategy 2: Append timestamp-based suffixes if we need more suggestions
        if (suggestions.Count < maxSuggestions)
        {
            var timestamp = DateTime.UtcNow.ToString("MMddHHmm", System.Globalization.CultureInfo.InvariantCulture);
            var timestampSuggestion = $"{baseName}-{timestamp}";
            if (ValidateResourceNameUniqueness(timestampSuggestion, resourceType, environment))
            {
                suggestions.Add(timestampSuggestion);
            }
        }

        // Strategy 3: Append random suffixes if we still need more
        var random = new Random();
        while (suggestions.Count < maxSuggestions)
        {
            var randomSuffix = random.Next(1000, 9999);
            var randomSuggestion = $"{baseName}-{randomSuffix}";
            if (ValidateResourceNameUniqueness(randomSuggestion, resourceType, environment) &&
                !suggestions.Contains(randomSuggestion))
            {
                suggestions.Add(randomSuggestion);
            }

            // Prevent infinite loop
            if (suggestions.Count == 0 && random.Next(1, 100) > 95)
            {
                break;
            }
        }

        _logger.LogInformation("Generated {Count} alternative names for '{BaseResourceName}'", suggestions.Count, baseName);
        return suggestions;
    }

    public bool RegisterValidatedResourceName(string resourceName, ResourceType resourceType, EnvironmentType environment, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            _logger.LogWarning("Cannot register null or empty resource name. CorrelationId: {CorrelationId}", correlationId);
            return false;
        }

        // First validate that the name is unique
        if (!ValidateResourceNameUniqueness(resourceName, resourceType, environment, correlationId))
        {
            _logger.LogWarning("Cannot register resource name '{ResourceName}' - validation failed (already exists). CorrelationId: {CorrelationId}",
                resourceName, correlationId);
            return false;
        }

        // Register the validated name
        var registered = _resourceNameRegistry.RegisterResourceName(resourceName, resourceType, environment, correlationId);

        if (registered)
        {
            _logger.LogInformation("Successfully registered validated resource name '{ResourceName}' of type '{ResourceType}' in environment '{Environment}'. CorrelationId: {CorrelationId}",
                resourceName, resourceType.ToStringValue(), environment.ToStringValue(), correlationId);
        }

        return registered;
    }

    public bool ValidateResourceNameUniqueness(string resourceName, ResourceType resourceType, string environment, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            _logger.LogWarning("Resource name validation failed: null or empty name provided. CorrelationId: {CorrelationId}", correlationId);
            return false;
        }

        // Convert string environment to EnvironmentType for internal processing
        if (!Enum.TryParse<EnvironmentType>(environment, true, out var environmentType))
        {
            _logger.LogWarning("Invalid environment value: '{Environment}'. CorrelationId: {CorrelationId}", environment, correlationId);
            return false;
        }

        return ValidateResourceNameUniqueness(resourceName, resourceType, environmentType, correlationId);
    }

    public void ValidateAndThrowIfDuplicate(string resourceName, ResourceType resourceType, string environment, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            throw new ArgumentException("Resource name cannot be null or empty", nameof(resourceName));
        }

        // Convert string environment to EnvironmentType for internal processing
        if (!Enum.TryParse<EnvironmentType>(environment, true, out var environmentType))
        {
            throw new ArgumentException($"Invalid environment value: '{environment}'", nameof(environment));
        }

        ValidateAndThrowIfDuplicate(resourceName, resourceType, environmentType, correlationId);
    }

    public Dictionary<string, bool> ValidateBatchResourceNames(IEnumerable<string> resourceNames, ResourceType resourceType, string environment, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(resourceNames);

        // Convert string environment to EnvironmentType for internal processing
        return !Enum.TryParse<EnvironmentType>(environment, true, out var environmentType) ? throw new ArgumentException($"Invalid environment value: '{environment}'", nameof(environment)) : ValidateBatchResourceNames(resourceNames, resourceType, environmentType, correlationId);
    }

    public IEnumerable<string> SuggestAlternativeNames(string baseResourceName, ResourceType resourceType, string environment, int maxSuggestions = 5)
    {
        // Convert string environment to EnvironmentType for internal processing
        if (!Enum.TryParse<EnvironmentType>(environment, true, out var environmentType))
        {
            throw new ArgumentException($"Invalid environment value: '{environment}'", nameof(environment));
        }

        return SuggestAlternativeNames(baseResourceName, resourceType, environmentType, maxSuggestions);
    }

    public bool RegisterValidatedResourceName(string resourceName, ResourceType resourceType, string environment, string? correlationId = null)
    {
        // Convert string environment to EnvironmentType for internal processing
        if (!Enum.TryParse<EnvironmentType>(environment, true, out var environmentType))
        {
            _logger.LogWarning("Invalid environment value: '{Environment}'. CorrelationId: {CorrelationId}", environment, correlationId);
            return false;
        }

        return RegisterValidatedResourceName(resourceName, resourceType, environmentType, correlationId);
    }
}

