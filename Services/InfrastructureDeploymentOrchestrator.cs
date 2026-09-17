using DeploymentKit.Interfaces;
using DeploymentKit.Models;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Instance-based orchestration API for Pulumi stack lifecycle operations.
/// </summary>
public sealed class InfrastructureDeploymentOrchestrator(
    IPulumiAutomationService automationService,
    IPulumiConfigLoader configLoader,
    ILogger<InfrastructureDeploymentOrchestrator> logger) : IInfrastructureDeploymentOrchestrator
{
    private readonly IPulumiAutomationService _automationService = automationService ?? throw new ArgumentNullException(nameof(automationService));
    private readonly IPulumiConfigLoader _configLoader = configLoader ?? throw new ArgumentNullException(nameof(configLoader));
    private readonly ILogger<InfrastructureDeploymentOrchestrator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task<PreviewResult> PreviewAsync(
        InfrastructureDeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(options, _automationService.PreviewAsync, cancellationToken);

    public Task<UpResult> DeployAsync(
        InfrastructureDeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(options, _automationService.UpAsync, cancellationToken);

    public Task<UpdateResult> RefreshAsync(
        InfrastructureDeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(options, _automationService.RefreshAsync, cancellationToken);

    public Task<UpdateResult> DestroyAsync(
        InfrastructureDeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(options, _automationService.DestroyAsync, cancellationToken);

    private async Task<T> ExecuteAsync<T>(
        InfrastructureDeploymentOrchestratorOptions options,
        Func<PulumiStackOperationRequest, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(operation);

        ValidateOptions(options);

        _logger.LogInformation(
            "Starting stack operation for project '{ProjectName}', stack '{StackName}', workDir '{WorkingDirectory}'",
            options.ProjectName,
            options.StackName,
            options.WorkingDirectory);

        InfrastructureConfigurationSourceResult configuration = await _configLoader.LoadConfigurationAsync(options, cancellationToken);

        var request = new PulumiStackOperationRequest
        {
            StackName = options.StackName,
            ProjectName = options.ProjectName,
            WorkingDirectory = options.WorkingDirectory,
            EscEnvironment = options.EscEnvironment,
            Config = configuration.Values,
            SecretConfigKeys = configuration.SecretKeys,
            EnvironmentVariables = options.EnvironmentVariables,
            RefreshBeforeUpdate = options.RefreshBeforeUpdate
        };

        return await operation(request, cancellationToken);
    }

    private static void ValidateOptions(InfrastructureDeploymentOrchestratorOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.StackName))
        {
            throw new ArgumentException("StackName is required", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.ProjectName))
        {
            throw new ArgumentException("ProjectName is required", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.WorkingDirectory))
        {
            throw new ArgumentException("WorkingDirectory is required", nameof(options));
        }
    }
}
