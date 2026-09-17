using DeploymentKit.Models.Results;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Interface for validating Azure resource state and preventing drift-related deployment failures
/// </summary>
public interface IAzureResourceStateValidator
{
    /// <summary>
    /// Validates that all expected Azure resources exist in the target subscription and resource group
    /// </summary>
    /// <param name="settings">Infrastructure settings containing resource configurations</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Validation result indicating resource existence status</returns>
    Task<ValidationResult> ValidateResourceExistenceAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates subscription and resource group targeting consistency
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Validation result for subscription and resource group targeting</returns>
    Task<ValidationResult> ValidateSubscriptionAndResourceGroupAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates resource naming consistency to prevent prefix/environment mismatches
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Validation result for naming consistency</returns>
    Task<ValidationResult> ValidateNamingConsistencyAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects drift between Pulumi state and actual Azure resources
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Drift detection result</returns>
    Task<ValidationResult> DetectResourceDriftAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates Azure authentication and permissions for resource operations
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Authentication and permissions validation result</returns>
    Task<ValidationResult> ValidateAzureAuthenticationAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs comprehensive pre-deployment validation including all Azure state checks
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Comprehensive validation result</returns>
    Task<ValidationResult> ValidatePreDeploymentStateAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default);

}

