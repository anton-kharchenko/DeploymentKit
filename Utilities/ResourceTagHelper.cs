using DeploymentKit.Constants;

namespace DeploymentKit.Utilities;

/// <summary>
/// Helper class for generating consistent resource tags across all Azure resources.
/// </summary>
public static class ResourceTagHelper
{
    private const string UnknownCorrelationId = "unknown";

    /// <summary>
    /// Gets standard tags that should be applied to all resources.
    /// </summary>
    /// <param name="environment">Environment name (dev, staging, prod).</param>
    /// <param name="resourceType">Optional resource type for categorization.</param>
    /// <param name="additionalTags">Additional custom tags.</param>
    /// <returns>Pulumi input map of tags.</returns>
    public static InputMap<string> GetStandardTags(
        string environment,
        string resourceType,
        Dictionary<string, string>? additionalTags = null) =>
        ToInputMap(CreateStandardTags(environment, resourceType, correlationId: null, additionalTags));

    /// <summary>
    /// Gets standard tags that should be applied to all resources.
    /// </summary>
    /// <param name="environment">Environment name (dev, staging, prod).</param>
    /// <param name="resourceType">Optional resource type for categorization.</param>
    /// <param name="correlationId">Deployment correlation identifier.</param>
    /// <param name="additionalTags">Additional custom tags.</param>
    /// <returns>Pulumi input map of tags.</returns>
    public static InputMap<string> GetStandardTags(
        string environment,
        string? resourceType,
        string? correlationId,
        IDictionary<string, string>? additionalTags = null) =>
        ToInputMap(CreateStandardTags(environment, resourceType, correlationId, additionalTags));

    /// <summary>
    /// Creates the plain tag dictionary used by Pulumi resource tag inputs.
    /// </summary>
    public static IReadOnlyDictionary<string, string> CreateStandardTags(
        string environment,
        string? resourceType,
        string? correlationId = null,
        IDictionary<string, string>? additionalTags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(environment);

        var tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ResourceTagNames.Environment] = environment,
            [ResourceTagNames.Project] = InfrastructureConstants.Tags.Project,
            [ResourceTagNames.Owner] = InfrastructureConstants.Tags.Owner,
            [ResourceTagNames.CorrelationId] = string.IsNullOrWhiteSpace(correlationId) ? UnknownCorrelationId : correlationId,
            [ResourceTagNames.CreatedBy] = InfrastructureConstants.Tags.ManagedBy,
            [ResourceTagNames.ManagedBy] = InfrastructureConstants.Tags.ManagedBy
        };

        if (!string.IsNullOrWhiteSpace(resourceType))
        {
            tags[ResourceTagNames.ResourceType] = resourceType;
        }

        if (additionalTags != null)
        {
            foreach (var tag in additionalTags)
            {
                tags[tag.Key] = tag.Value;
            }
        }

        return tags;
    }

    private static InputMap<string> ToInputMap(IReadOnlyDictionary<string, string> tags)
    {
        var inputMap = new InputMap<string>();
        foreach (var tag in tags)
        {
            inputMap[tag.Key] = tag.Value;
        }

        return inputMap;
    }
}
