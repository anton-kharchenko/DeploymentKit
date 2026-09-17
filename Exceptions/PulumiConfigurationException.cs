using DeploymentKit.Enums;

namespace DeploymentKit.Exceptions;

/// <summary>
/// Exception thrown when there are Pulumi configuration or encryption issues
/// </summary>
[Serializable]
public class PulumiConfigurationException : InfrastructureException
{
    /// <summary>
    /// The type of configuration issue encountered
    /// </summary>
    public ConfigurationIssueType IssueType { get; }

    /// <summary>
    /// Suggested resolution steps
    /// </summary>
    public string? SuggestedResolution { get; }

    public PulumiConfigurationException()
    {
    }

    public PulumiConfigurationException(string message) : base(message)
    {
    }

    public PulumiConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public PulumiConfigurationException(
        string message,
        ConfigurationIssueType issueType,
        string? suggestedResolution = null)
        : base(message)
    {
        IssueType = issueType;
        SuggestedResolution = suggestedResolution;
    }

    public PulumiConfigurationException(
        string message,
        Exception innerException,
        ConfigurationIssueType issueType,
        string? suggestedResolution = null)
        : base(message, innerException)
    {
        IssueType = issueType;
        SuggestedResolution = suggestedResolution;
    }
}
