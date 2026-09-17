using DeploymentKit.Models;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Loads configuration for deployment orchestration from a specific source.
/// </summary>
public interface IInfrastructureConfigurationSource
{
    string SourceName { get; }

    Task<InfrastructureConfigurationSourceResult> LoadAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default);
}

