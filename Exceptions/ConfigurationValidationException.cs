namespace DeploymentKit.Exceptions;

/// <summary>
/// Exception thrown when configuration validation fails
/// </summary>
[Serializable]
public class ConfigurationValidationException : InfrastructureException
{
    public string? ConfigurationSection { get; }
    public string? ValidationRule { get; }

    public ConfigurationValidationException() { }

    public ConfigurationValidationException(string message) : base(message) { }

    public ConfigurationValidationException(string message, Exception innerException) : base(message, innerException) { }

    public ConfigurationValidationException(string message, string? configurationSection, string? validationRule) : base(message)
    {
        ConfigurationSection = configurationSection;
        ValidationRule = validationRule;
    }

    public ConfigurationValidationException(string message, Exception innerException, string? configurationSection, string? validationRule) : base(message, innerException)
    {
        ConfigurationSection = configurationSection;
        ValidationRule = validationRule;
    }
}
