using DeploymentKit.Models;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Executes Pulumi stack operations through the Pulumi Automation API.
/// </summary>
public interface IPulumiAutomationService
{
    Task<UpResult> UpAsync(PulumiStackOperationRequest request, CancellationToken cancellationToken = default);

    Task<PreviewResult> PreviewAsync(PulumiStackOperationRequest request, CancellationToken cancellationToken = default);

    Task<UpdateResult> RefreshAsync(PulumiStackOperationRequest request, CancellationToken cancellationToken = default);

    Task<UpdateResult> DestroyAsync(PulumiStackOperationRequest request, CancellationToken cancellationToken = default);
}

