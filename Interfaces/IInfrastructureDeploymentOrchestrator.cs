using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Orchestrates stack lifecycle operations for DeploymentKit infrastructure.
/// </summary>
public interface IInfrastructureDeploymentOrchestrator
{
    Task<PreviewResult> PreviewAsync(
        InfrastructureDeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default);

    Task<UpResult> DeployAsync(
        InfrastructureDeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default);

    Task<UpdateResult> RefreshAsync(
        InfrastructureDeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default);

    Task<UpdateResult> DestroyAsync(
        InfrastructureDeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default);
}

