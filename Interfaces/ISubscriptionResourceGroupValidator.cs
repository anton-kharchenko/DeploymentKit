using DeploymentKit.Models.Results;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Interface for validating Azure subscription and resource group targeting
/// Helps prevent ResourceNotFound errors due to incorrect subscription or resource group targeting
/// </summary>
public interface ISubscriptionResourceGroupValidator
{
    /// <summary>
    /// Validates that the specified Azure subscription exists and is accessible
    /// </summary>
    /// <param name="subscriptionId">Azure subscription ID to validate</param>
    /// <returns>Validation result indicating if subscription is valid and accessible</returns>
    Task<ValidationResult> ValidateSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Validates that the specified resource group exists in the given subscription
    /// </summary>
    /// <param name="subscriptionId">Azure subscription ID</param>
    /// <param name="resourceGroupName">Resource group name to validate</param>
    /// <returns>Validation result indicating if resource group exists</returns>
    Task<ValidationResult> ValidateResourceGroupAsync(string subscriptionId, string resourceGroupName);

    /// <summary>
    /// Validates that the current Azure authentication context matches the expected subscription
    /// </summary>
    /// <param name="expectedSubscriptionId">Expected subscription ID from settings</param>
    /// <returns>Validation result indicating if authentication context is correct</returns>
    Task<ValidationResult> ValidateAuthenticationContextAsync(string expectedSubscriptionId);

    /// <summary>
    /// Validates the complete subscription and resource group configuration
    /// </summary>
    /// <param name="settings">Infrastructure settings containing subscription and resource group information</param>
    /// <returns>Comprehensive validation result for subscription and resource group targeting</returns>
    Task<ValidationResult> ValidateSubscriptionAndResourceGroupAsync(InfrastructureSettings settings);

    /// <summary>
    /// Gets the current Azure subscription ID from the authentication context
    /// </summary>
    /// <returns>Current subscription ID or null if not authenticated</returns>
    Task<string?> GetCurrentSubscriptionIdAsync();

    /// <summary>
    /// Lists all resource groups in the specified subscription
    /// </summary>
    /// <param name="subscriptionId">Azure subscription ID</param>
    /// <returns>List of resource group names in the subscription</returns>
    Task<IEnumerable<string>> ListResourceGroupsAsync(string subscriptionId);

    /// <summary>
    /// Validates that the resource group location matches the expected location
    /// </summary>
    /// <param name="subscriptionId">Azure subscription ID</param>
    /// <param name="resourceGroupName">Resource group name</param>
    /// <param name="expectedLocation">Expected Azure region</param>
    /// <returns>Validation result for resource group location</returns>
    Task<ValidationResult> ValidateResourceGroupLocationAsync(
        string subscriptionId,
        string resourceGroupName,
        string expectedLocation);
}

