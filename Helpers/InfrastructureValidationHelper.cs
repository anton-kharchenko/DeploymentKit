using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Exceptions;
using DeploymentKit.Extensions;
using DeploymentKit.Interfaces;

namespace DeploymentKit.Helpers;

/// <summary>
/// Helper class for validating Application configuration.
/// </summary>
public class InfrastructureValidationHelper
{
    private readonly IResourceNameValidator _resourceNameValidator;
    private readonly ILogger _logger;
    private readonly List<string> _validationErrors;

    /// <summary>
    /// Initializes a new instance of the <see cref="InfrastructureValidationHelper"/> class.
    /// </summary>
    /// <param name="resourceNameValidator">The resource name validator.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="validationErrors">The list to collect validation errors.</param>
    public InfrastructureValidationHelper(
        IResourceNameValidator resourceNameValidator,
        ILogger logger,
        List<string> validationErrors)
    {
        _resourceNameValidator = resourceNameValidator ?? throw new ArgumentNullException(nameof(resourceNameValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validationErrors = validationErrors ?? throw new ArgumentNullException(nameof(validationErrors));
    }

    /// <summary>
    /// Validates and sets the deployment name.
    /// </summary>
    public string? ValidateAndSetDeploymentName(string deploymentName, string? environment)
    {
        if (string.IsNullOrWhiteSpace(deploymentName))
        {
            throw new ArgumentException(BuilderConstants.ValidationMessages.DeploymentNameRequired, nameof(deploymentName));
        }

        // Validate name format (alphanumeric, hyphens allowed, 3-50 characters)
        if (!IsValidDeploymentName(deploymentName))
        {
            _validationErrors.Add(BuilderConstants.ValidationMessages.DeploymentNameFormat);
            return null;
        }

        // Check if name is available using the validation service
        var environmentType = ParseEnvironmentType(environment ?? "development");

        try
        {
            _resourceNameValidator.ValidateAndThrowIfDuplicate(deploymentName, ResourceType.ContainerApp, environmentType);
            _logger.LogInformation(BuilderConstants.Logs.DeploymentNameValidated, deploymentName);
            return deploymentName;
        }
        catch (DuplicateResourceNameException)
        {
            // Re-throw duplicate name exceptions so they can be caught by tests
            throw;
        }
        catch (Exception ex)
        {
            _validationErrors.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, BuilderConstants.ValidationMessages.DeploymentNameUnavailable, deploymentName, ex.Message));
            _logger.LogWarning(BuilderConstants.Logs.DeploymentNameValidationFailed, deploymentName, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Validates and sets the environment.
    /// </summary>
    public string? ValidateAndSetEnvironment(string environment)
    {
        if (string.IsNullOrWhiteSpace(environment))
        {
            _validationErrors.Add(BuilderConstants.ValidationMessages.EnvironmentRequired);
            return null;
        }

        var normalizedEnv = environment.ToLowerInvariant();
        if (!normalizedEnv.Equals(EnvironmentType.Development.ToStringValue(), StringComparison.InvariantCultureIgnoreCase) &&
            !normalizedEnv.Equals(EnvironmentType.Production.ToStringValue(), StringComparison.InvariantCultureIgnoreCase))
        {
            _validationErrors.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, BuilderConstants.ValidationMessages.InvalidEnvironment, EnvironmentType.Development.ToStringValue(), EnvironmentType.Production.ToStringValue()));
            return null;
        }

        _logger.LogInformation(BuilderConstants.Logs.EnvironmentSet, normalizedEnv);
        return normalizedEnv;
    }

    /// <summary>
    /// Validates that the required configuration is present.
    /// </summary>
    public void ValidateRequiredConfiguration(
        string? deploymentName,
        string? environment,
        string? location,
        string? subscriptionId,
        bool hasResourceConfigured,
        string? keyVaultEnvFilePath)
    {
        if (string.IsNullOrWhiteSpace(deploymentName))
        {
            _validationErrors.Add("Deployment name is required");
        }

        if (string.IsNullOrWhiteSpace(environment))
        {
            _validationErrors.Add("Environment is required");
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            _validationErrors.Add("Location is required");
        }

        if (string.IsNullOrWhiteSpace(subscriptionId))
        {
            _validationErrors.Add(BuilderConstants.ValidationMessages.SubscriptionIdMissing);
        }

        // Validate that at least one resource is configured
        if (!hasResourceConfigured)
        {
            _validationErrors.Add(BuilderConstants.ValidationMessages.NoResourcesConfigured);
        }

        // Validate .env file path if provided
        if (!string.IsNullOrWhiteSpace(keyVaultEnvFilePath) && !File.Exists(keyVaultEnvFilePath))
        {
            _validationErrors.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, BuilderConstants.ValidationMessages.KeyVaultEnvFileNotFound, keyVaultEnvFilePath));
        }
    }

    /// <summary>
    /// Checks if a deployment name is valid.
    /// </summary>
    public static bool IsValidDeploymentName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 50)
            return false;

        return name.All(c => char.IsLetterOrDigit(c) || c == '-');
    }

    /// <summary>
    /// Parses the environment string to an EnvironmentType.
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


