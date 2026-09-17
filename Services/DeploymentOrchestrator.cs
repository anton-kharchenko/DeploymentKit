using DeploymentKit.Interfaces;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Generic deployment orchestrator that delegates to the existing orchestration implementation.
/// </summary>
public sealed class DeploymentOrchestrator(IInfrastructureDeploymentOrchestrator orchestrator) : IDeploymentOrchestrator
{
    private readonly IInfrastructureDeploymentOrchestrator _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));

    public Task<PreviewResult> PreviewAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default) =>
        _orchestrator.PreviewAsync(ToInfrastructureOptions(options), cancellationToken);

    public Task<UpResult> DeployAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default) =>
        _orchestrator.DeployAsync(ToInfrastructureOptions(options), cancellationToken);

    public Task<UpdateResult> RefreshAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default) =>
        _orchestrator.RefreshAsync(ToInfrastructureOptions(options), cancellationToken);

    public Task<UpdateResult> DestroyAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default) =>
        _orchestrator.DestroyAsync(ToInfrastructureOptions(options), cancellationToken);

    private static InfrastructureDeploymentOrchestratorOptions ToInfrastructureOptions(DeploymentOrchestratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options as InfrastructureDeploymentOrchestratorOptions ?? new InfrastructureDeploymentOrchestratorOptions
        {
            StackName = options.StackName,
            ProjectName = options.ProjectName,
            WorkingDirectory = options.WorkingDirectory,
            EscEnvironment = options.EscEnvironment,
            PulumiConfig = options.PulumiConfig,
            Config = options.Config,
            SecretConfigKeys = options.SecretConfigKeys,
            EnvironmentFallbackMappings = options.EnvironmentFallbackMappings,
            RequiredConfigKeys = options.RequiredConfigKeys,
            EnvironmentVariables = options.EnvironmentVariables
        };
    }
}

