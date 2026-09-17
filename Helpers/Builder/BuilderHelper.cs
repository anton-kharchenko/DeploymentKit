using DeploymentKit.Constants;
using DeploymentKit.Enums;

namespace DeploymentKit.Helpers.Builder;

/// <summary>
/// Helper class for InfrastructureBuilder operations.
/// </summary>
public static class BuilderHelper
{
    /// <summary>
    /// Validates if the deployment name meets the requirements.
    /// </summary>
    public static bool IsValidDeploymentName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 50)
            return false;

        return name.All(c => char.IsLetterOrDigit(c) || c == '-');
    }

    /// <summary>
    /// Parses the environment type string into an EnvironmentType enum.
    /// </summary>
    public static EnvironmentType ParseEnvironmentType(string environment)
    {
        return environment.ToLowerInvariant() switch
        {
            "dev" or "development" => EnvironmentType.Development,
            "prod" or "production" => EnvironmentType.Production,
            _ => EnvironmentType.Development
        };
    }

    /// <summary>
    /// Maps user-friendly environment names to validation-compliant values.
    /// </summary>
    public static string MapEnvironmentName(string environment)
    {
        return environment.ToLowerInvariant() switch
        {
            "development" => "dev",
            "production" => "prod",
            "dev" => "dev",
            "test" => "test",
            "staging" => "staging",
            "prod" => "prod",
            _ => "dev" // Default to dev for unknown values
        };
    }

    /// <summary>
    /// Sanitizes a string to be used as a naming prefix by removing invalid characters.
    /// </summary>
    public static string SanitizeNamingPrefix(string name)
    {
        // Remove hyphens and underscores, keep only alphanumeric characters
        var sanitized = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

        // Ensure it starts with a letter
        if (string.IsNullOrEmpty(sanitized) || !char.IsLetter(sanitized[0]))
        {
            sanitized = InfrastructureConstants.Defaults.NamingPrefix + sanitized;
        }

        // Limit to 20 characters (max naming prefix length)
        if (sanitized.Length > 20)
        {
            sanitized = sanitized[..20];
        }

        return sanitized;
    }
}

