using DeploymentKit.Exceptions;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.Resources;

namespace DeploymentKit.Helpers.Infrastructure;

/// <summary>
/// Helper methods for InfrastructureOrchestrator to reduce complexity.
/// </summary>
public static class InfrastructureOrchestratorHelper
{
    /// <summary>
    /// Creates the resource group for the infrastructure.
    /// </summary>
    public static ResourceGroup CreateResourceGroup(
        InfrastructureSettings settings,
        string correlationId,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "ResourceGroupCreation",
            ["CorrelationId"] = correlationId
        });

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Use the resource group name from settings
            var resourceGroupName = settings.ResourceGroupName;

            logger.LogInformation("Creating resource group: {ResourceGroupName} in location: {Location} for CorrelationId: {CorrelationId}",
                resourceGroupName, settings.Location, correlationId);

            var resourceGroup = new ResourceGroup(resourceGroupName, new ResourceGroupArgs
            {
                ResourceGroupName = resourceGroupName,
                Location = settings.Location,
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, "resource-group")
            });

            logger.LogInformation("Successfully created resource group: {ResourceGroupName} for CorrelationId: {CorrelationId}",
                resourceGroupName, correlationId);

            return resourceGroup;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create resource group for environment: {Environment} with CorrelationId: {CorrelationId}. Error: {ErrorMessage}",
                settings.Environment, correlationId, ex.Message);

            throw new ResourceCreationException(
                $"Failed to create resource group for environment '{settings.Environment}' (CorrelationId: {correlationId})",
                ex,
                "ResourceGroup",
                "ResourceGroup",
                settings.Environment,
                correlationId,
                "RG_CREATION_FAILED");
        }
    }

    /// <summary>
    /// Validates the infrastructure settings.
    /// </summary>
    public static void ValidateSettings(InfrastructureSettings settings, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(settings);

        logger.LogDebug("Validating infrastructure settings for environment: {Environment}", settings.Environment);

        if (string.IsNullOrWhiteSpace(settings.Environment))
        {
            logger.LogError("Environment validation failed: Environment is required");
            throw new ConfigurationValidationException("Environment is required", "Environment", "InfrastructureSettings");
        }

        if (string.IsNullOrWhiteSpace(settings.Location))
        {
            logger.LogError("Location validation failed: Location is required");
            throw new ConfigurationValidationException("Location is required", "Location", "InfrastructureSettings");
        }

        if (string.IsNullOrWhiteSpace(settings.NamingPrefix))
        {
            logger.LogError("NamingPrefix validation failed: NamingPrefix is required");
            throw new ConfigurationValidationException("NamingPrefix is required", "NamingPrefix", "InfrastructureSettings");
        }

        if (string.IsNullOrWhiteSpace(settings.ResourceGroupName))
        {
            logger.LogError("ResourceGroupName validation failed: ResourceGroupName is required");
            throw new ConfigurationValidationException("ResourceGroupName is required", "ResourceGroupName", "InfrastructureSettings");
        }

        if (string.IsNullOrWhiteSpace(settings.SubscriptionId))
        {
            logger.LogError("SubscriptionId validation failed: SubscriptionId is required");
            throw new ConfigurationValidationException("SubscriptionId is required", "SubscriptionId", "InfrastructureSettings");
        }

        logger.LogDebug("Infrastructure settings validation completed successfully for environment: {Environment}", settings.Environment);
    }
}

