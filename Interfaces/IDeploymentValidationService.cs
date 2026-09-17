using DeploymentKit.Models.Outputs;
using DeploymentKit.Models.Results;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Interface for validating deployment configurations and prerequisites
/// </summary>
public interface IDeploymentValidationService
{
    /// <summary>
    /// Validates green-blue deployment configuration
    /// </summary>
    /// <param name="settings">Infrastructure settings to validate</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateGreenBlueConfigurationAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a deployment is ready to proceed
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="currentOutputs">Current deployment state</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Deployment readiness validation result</returns>
    Task<ValidationResult> ValidateDeploymentReadinessAsync(InfrastructureSettings settings, GreenBlueDeploymentOutputs? currentOutputs = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a deployment can be safely rolled back
    /// </summary>
    /// <param name="currentOutputs">Current deployment state</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Rollback validation result</returns>
    Task<ValidationResult> ValidateRollbackReadinessAsync(GreenBlueDeploymentOutputs currentOutputs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates image availability in container registry
    /// </summary>
    /// <param name="registryUrl">Container registry URL</param>
    /// <param name="imageName">Image name</param>
    /// <param name="imageTag">Image tag</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Image validation result</returns>
    Task<ValidationResult> ValidateImageAvailabilityAsync(string registryUrl, string imageName, string imageTag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the naming prefix and environment against Azure resource-specific constraints.
    /// Performs prefix syntax checks and evaluates computed resource name lengths for Storage Account, Key Vault, and Container Registry.
    /// </summary>
    /// <param name="settings">Infrastructure settings containing NamingPrefix and Environment</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Validation result with errors and recommendations if constraints are exceeded</returns>
    Task<ValidationResult> ValidateNamingPrefixAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default);
}

