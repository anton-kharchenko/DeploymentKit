using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Orchestrates Pulumi stack lifecycle operations for deployment automation.
/// </summary>
public interface IDeploymentOrchestrator
{
    Task<PreviewResult> PreviewAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default);

    Task<UpResult> DeployAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default);

    Task<UpdateResult> RefreshAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default);

    Task<UpdateResult> DestroyAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default);
}

