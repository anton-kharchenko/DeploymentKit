using DeploymentKit.Services;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Orchestrates comprehensive validation across all validation services.
/// Provides a unified interface for running pre-deployment, drift detection, and state validation.
/// </summary>
public interface IValidationOrchestratorService
{
    /// <summary>
    /// Runs comprehensive validation including pre-deployment checks, Azure state validation, and drift detection.
    /// </summary>
    /// <param name="settings">Infrastructure settings to validate</param>
    /// <param name="includeStateValidation">Whether to include Azure state validation (requires Azure connectivity)</param>
    /// <param name="includeDriftDetection">Whether to include drift detection (requires Azure connectivity)</param>
    /// <returns>Comprehensive validation result</returns>
    Task<PreDeploymentValidationResult> RunComprehensiveValidationAsync(InfrastructureSettings settings, bool includeStateValidation = true, bool includeDriftDetection = true);

    /// <summary>
    /// Runs only pre-deployment validation without Azure connectivity requirements.
    /// </summary>
    /// <param name="settings">Infrastructure settings to validate</param>
    /// <returns>Pre-deployment validation result</returns>
    Task<PreDeploymentValidationResult> RunPreDeploymentValidationAsync(InfrastructureSettings settings);

    /// <summary>
    /// Runs Azure state validation to check resource existence and consistency.
    /// </summary>
    /// <param name="settings">Infrastructure settings to validate</param>
    /// <returns>Azure state validation result</returns>
    Task<PreDeploymentValidationResult> RunAzureStateValidationAsync(InfrastructureSettings settings);

    /// <summary>
    /// Runs drift detection to identify configuration inconsistencies.
    /// </summary>
    /// <param name="settings">Infrastructure settings to validate</param>
    /// <returns>Drift detection result</returns>
    Task<PreDeploymentValidationResult> RunDriftDetectionAsync(InfrastructureSettings settings);

    /// <summary>
    /// Generates a comprehensive validation report with recommendations.
    /// </summary>
    /// <param name="settings">Infrastructure settings to validate</param>
    /// <returns>Comprehensive validation report</returns>
    Task<PreDeploymentValidationResult> GenerateValidationReportAsync(InfrastructureSettings settings);

    /// <summary>
    /// Validates specific resource types only.
    /// </summary>
    /// <param name="settings">Infrastructure settings to validate</param>
    /// <param name="resourceTypes">Specific resource types to validate (e.g., "PostgreSQL", "EventHubs")</param>
    /// <returns>Resource-specific validation result</returns>
    Task<PreDeploymentValidationResult> ValidateSpecificResourcesAsync(InfrastructureSettings settings, params string[] resourceTypes);

    /// <summary>
    /// Performs a quick validation check for CI/CD pipelines.
    /// </summary>
    /// <param name="settings">Infrastructure settings to validate</param>
    /// <returns>Quick validation result optimized for CI/CD</returns>
    Task<PreDeploymentValidationResult> RunQuickValidationAsync(InfrastructureSettings settings);
}

