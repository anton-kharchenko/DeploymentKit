using DeploymentKit.Constants;
// For InfrastructureBuilder access if needed, though we should try to rely on passed params

namespace DeploymentKit.Validators;

/// <summary>
/// Implementation of IInfrastructureBuilderValidator.
/// </summary>
public class InfrastructureBuilderValidator : IInfrastructureBuilderValidator
{
    /// <inheritdoc />
    public List<string> Validate(
        string? deploymentName,
        string? environment,
        string? location,
        string? subscriptionId,
        bool hasResourcesConfigured,
        string? keyVaultEnvFilePath)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(deploymentName))
        {
            errors.Add(BuilderConstants.ErrorMessages.DeploymentNameRequired);
        }

        if (string.IsNullOrWhiteSpace(environment))
        {
            errors.Add(BuilderConstants.ErrorMessages.EnvironmentRequired);
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            errors.Add(BuilderConstants.ErrorMessages.LocationRequired);
        }

        if (string.IsNullOrWhiteSpace(subscriptionId))
        {
            errors.Add(BuilderConstants.ErrorMessages.SubscriptionIdRequired);
        }

        if (!hasResourcesConfigured)
        {
            errors.Add(BuilderConstants.ErrorMessages.AtLeastOneResource);
        }

        return errors;
    }
}

