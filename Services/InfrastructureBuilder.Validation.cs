using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Interfaces;

namespace DeploymentKit.Services;

/// <summary>
/// Partial class for InfrastructureBuilder containing validation logic.
/// </summary>
public partial class InfrastructureBuilder
{
    // Validation configuration
    private ValidationMode _validationMode = ValidationMode.Basic;
    private bool _skipAzureAuthValidation;

    /// <summary>
    /// Sets the validation mode for the deployment
    /// </summary>
    /// <param name="validationMode">The validation mode to use</param>
    /// <returns>The builder instance for method chaining</returns>
    public IInfrastructureBuilder SetValidationMode(ValidationMode validationMode)
    {
        _validationMode = validationMode;
        _logger.LogInformation(BuilderConstants.Logs.ValidationModeSet, validationMode);

        if (validationMode == ValidationMode.Basic)
        {
            _logger.LogInformation(BuilderConstants.Logs.ValidationModeBasic);
        }
        else if (validationMode == ValidationMode.Skip)
        {
            _logger.LogWarning(BuilderConstants.Logs.ValidationModeSkip);
        }

        return this;
    }

    /// <summary>
    /// Skips Azure authentication validation - useful when using Azure CLI authentication
    /// </summary>
    /// <param name="skip">Whether to skip Azure authentication validation</param>
    /// <returns>The builder instance for method chaining</returns>
    public IInfrastructureBuilder SkipAzureAuthValidation(bool skip = true)
    {
        _skipAzureAuthValidation = skip;

        if (skip)
        {
            _logger.LogInformation(BuilderConstants.Logs.AzureAuthValidationSkipped);
        }
        else
        {
            _logger.LogInformation(BuilderConstants.Logs.AzureAuthValidationEnabled);
        }

        return this;
    }

    public bool Validate()
    {
        _validationErrors.Clear();
        ValidateRequiredConfiguration();
        return !_validationErrors.Any();
    }

    public IReadOnlyList<string> GetValidationErrors()
    {
        _validationErrors.Clear();
        ValidateRequiredConfiguration();
        return _validationErrors.AsReadOnly();
    }

    #region Private Helper Methods

    private void ValidateRequiredConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_deploymentName))
        {
            _validationErrors.Add(BuilderConstants.ErrorMessages.DeploymentNameRequired);
        }

        if (string.IsNullOrWhiteSpace(_environment))
        {
            _validationErrors.Add(BuilderConstants.ErrorMessages.EnvironmentRequired);
        }

        if (string.IsNullOrWhiteSpace(_location))
        {
            _validationErrors.Add(BuilderConstants.ErrorMessages.LocationRequired);
        }

        if (string.IsNullOrWhiteSpace(_subscriptionId))
        {
            _validationErrors.Add(BuilderConstants.ErrorMessages.SubscriptionIdRequired);
        }

        // Validate that at least one resource is configured
        if (!_addKeyVault && !_addRedis && !_addMessageBroker && !_addInsights &&
            !_addDatabase && !_addContainerRegistry && !_addStorage && !_addContainerApps &&
            !_addNetworking && !_addApplicationGateway && !_addDomainOptimization && !_addVpn)
        {
            _validationErrors.Add(BuilderConstants.ErrorMessages.AtLeastOneResource);
        }

    }

    private static bool IsValidDeploymentName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 50)
            return false;

        return name.All(c => char.IsLetterOrDigit(c) || c == '-');
    }

    /// <summary>
    /// Sanitizes a string to be used as a naming prefix by removing invalid characters
    /// </summary>
    private static string SanitizeNamingPrefix(string name)
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

    #endregion
}

