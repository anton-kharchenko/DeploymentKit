using DeploymentKit.Interfaces;
using DeploymentKit.Models;
using DeploymentKit.Utilities;

namespace DeploymentKit.Services;

/// <summary>
/// Executes Pulumi stack operations through the Pulumi Automation API.
/// </summary>
public sealed class PulumiAutomationService(
    ILegacyEnvironmentBridge legacyEnvironmentBridge,
    ILogger<PulumiAutomationService> logger) : IPulumiAutomationService
{
    private const string PulumiEscEnvironmentVariable = "PULUMI_ENV";
    private static readonly SemaphoreSlim EnvironmentMutationLock = new(1, 1);

    private readonly ILegacyEnvironmentBridge _legacyEnvironmentBridge = legacyEnvironmentBridge ?? throw new ArgumentNullException(nameof(legacyEnvironmentBridge));
    private readonly ILogger<PulumiAutomationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task<UpResult> UpAsync(PulumiStackOperationRequest request, CancellationToken cancellationToken = default) =>
        ExecuteWithScopedEnvironmentAsync(
            request,
            (stack, token) => stack.UpAsync(new UpOptions
            {
                OnStandardOutput = output => _logger.LogInformation("{PulumiOutput}", output),
                OnStandardError = error => _logger.LogError("{PulumiError}", error),
                Refresh = request.RefreshBeforeUpdate,
                ShowSecrets = false
            }, token),
            cancellationToken);

    public Task<PreviewResult> PreviewAsync(PulumiStackOperationRequest request, CancellationToken cancellationToken = default) =>
        ExecuteWithScopedEnvironmentAsync(
            request,
            (stack, token) => stack.PreviewAsync(new PreviewOptions
            {
                OnStandardOutput = output => _logger.LogInformation("{PulumiOutput}", output),
                OnStandardError = error => _logger.LogError("{PulumiError}", error)
            }, token),
            cancellationToken);

    public Task<UpdateResult> RefreshAsync(PulumiStackOperationRequest request, CancellationToken cancellationToken = default) =>
        ExecuteWithScopedEnvironmentAsync(
            request,
            (stack, token) => stack.RefreshAsync(new RefreshOptions
            {
                OnStandardOutput = output => _logger.LogInformation("{PulumiOutput}", output),
                OnStandardError = error => _logger.LogError("{PulumiError}", error),
                ShowSecrets = false
            }, token),
            cancellationToken);

    public Task<UpdateResult> DestroyAsync(PulumiStackOperationRequest request, CancellationToken cancellationToken = default) =>
        ExecuteWithScopedEnvironmentAsync(
            request,
            (stack, token) => stack.DestroyAsync(new DestroyOptions
            {
                OnStandardOutput = output => _logger.LogInformation("{PulumiOutput}", output),
                OnStandardError = error => _logger.LogError("{PulumiError}", error),
                ShowSecrets = false
            }, token),
            cancellationToken);

    private async Task<T> ExecuteWithScopedEnvironmentAsync<T>(
        PulumiStackOperationRequest request,
        Func<WorkspaceStack, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(operation);

        await EnvironmentMutationLock.WaitAsync(cancellationToken);

        try
        {
            var scopedEnvironment = BuildScopedEnvironmentVariables(request);
            using var scope = _legacyEnvironmentBridge.Apply(scopedEnvironment);

            LocalProgramArgs stackArgs = new(request.StackName, request.WorkingDirectory);
            using WorkspaceStack stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs, cancellationToken);

            await ApplyConfigAsync(stack, request, cancellationToken);
            return await operation(stack, cancellationToken);
        }
        finally
        {
            EnvironmentMutationLock.Release();
        }
    }

    private static IDictionary<string, string?> BuildScopedEnvironmentVariables(PulumiStackOperationRequest request)
    {
        var environmentVariables = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (request.EnvironmentVariables != null)
        {
            foreach (var (key, value) in request.EnvironmentVariables)
            {
                environmentVariables[key] = value;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.EscEnvironment))
        {
            environmentVariables[PulumiEscEnvironmentVariable] = request.EscEnvironment;
        }

        return environmentVariables;
    }

    private static async Task ApplyConfigAsync(
        WorkspaceStack stack,
        PulumiStackOperationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Config == null || request.Config.Count == 0)
        {
            return;
        }

        var secretKeys = request.SecretConfigKeys ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in request.Config)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var isSecret = secretKeys.Contains(key) || SensitiveConfigurationKeyClassifier.IsSensitive(key);
            await stack.SetConfigAsync(key, new ConfigValue(value, isSecret), cancellationToken);
        }
    }
}
