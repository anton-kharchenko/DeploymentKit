using DeploymentKit.Services;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service for detecting drift between Pulumi state and actual Azure resources.
/// Helps identify when resources have been modified outside of Pulumi.
/// </summary>
public interface IDriftDetectionService
{
    /// <summary>
    /// Detects drift for all resources defined in the infrastructure settings.
    /// </summary>
    /// <param name="settings">Infrastructure settings containing resource definitions</param>
    /// <returns>Drift detection result with identified inconsistencies</returns>
    Task<PreDeploymentValidationResult> DetectDriftAsync(InfrastructureSettings settings);
    
    /// <summary>
    /// Detects drift for a specific resource type.
    /// </summary>
    /// <param name="resourceType">Type of resource to check for drift</param>
    /// <param name="resourceName">Name of the specific resource</param>
    /// <param name="settings">Infrastructure settings</param>
    /// <returns>Drift detection result for the specific resource</returns>
    Task<PreDeploymentValidationResult> DetectResourceDriftAsync(string resourceType, string resourceName, InfrastructureSettings settings);
    
    /// <summary>
    /// Detects configuration drift for PostgreSQL database resources.
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <returns>Drift detection result for PostgreSQL resources</returns>
    Task<PreDeploymentValidationResult> DetectPostgreSqlDriftAsync(InfrastructureSettings settings);
    
    /// <summary>
    /// Detects configuration drift for Event Hubs resources.
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <returns>Drift detection result for Event Hubs resources</returns>
    Task<PreDeploymentValidationResult> DetectEventHubsDriftAsync(InfrastructureSettings settings);
    
    /// <summary>
    /// Detects configuration drift for Virtual Network resources.
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <returns>Drift detection result for Virtual Network resources</returns>
    Task<PreDeploymentValidationResult> DetectVirtualNetworkDriftAsync(InfrastructureSettings settings);
    
    /// <summary>
    /// Detects configuration drift for Storage Account resources.
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <returns>Drift detection result for Storage Account resources</returns>
    Task<PreDeploymentValidationResult> DetectStorageAccountDriftAsync(InfrastructureSettings settings);
    
    /// <summary>
    /// Detects configuration drift for Key Vault resources.
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <returns>Drift detection result for Key Vault resources</returns>
    Task<PreDeploymentValidationResult> DetectKeyVaultDriftAsync(InfrastructureSettings settings);
    
    /// <summary>
    /// Detects configuration drift for Container Registry resources.
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <returns>Drift detection result for Container Registry resources</returns>
    Task<PreDeploymentValidationResult> DetectContainerRegistryDriftAsync(InfrastructureSettings settings);
    
    /// <summary>
    /// Generates a comprehensive drift report with recommendations.
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <returns>Detailed drift report with remediation suggestions</returns>
    Task<PreDeploymentValidationResult> GenerateDriftReportAsync(InfrastructureSettings settings);
}
