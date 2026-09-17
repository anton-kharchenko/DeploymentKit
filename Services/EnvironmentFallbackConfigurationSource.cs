using DeploymentKit.Interfaces;
using DeploymentKit.Models;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Loads deployment configuration from environment variables as a fallback path.
/// </summary>
public sealed class EnvironmentFallbackConfigurationSource(ILogger<EnvironmentFallbackConfigurationSource> logger) : IInfrastructureConfigurationSource
{
    private readonly ILogger<EnvironmentFallbackConfigurationSource> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public string SourceName => "environment-fallback";

    public Task<InfrastructureConfigurationSourceResult> LoadAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var secretKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (options.EnvironmentFallbackMappings == null || options.EnvironmentFallbackMappings.Count == 0)
        {
            _logger.LogInformation("Configuration source '{SourceName}' skipped because no fallback mappings were configured", SourceName);
            return Task.FromResult(new InfrastructureConfigurationSourceResult
            {
                Values = values,
                SecretKeys = secretKeys
            });
        }

        foreach (var (configKey, environmentVariableNames) in options.EnvironmentFallbackMappings)
        {
            if (string.IsNullOrWhiteSpace(configKey) || environmentVariableNames.Length == 0)
            {
                continue;
            }

            var resolvedValue = environmentVariableNames
                .Select(Environment.GetEnvironmentVariable)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

            if (string.IsNullOrWhiteSpace(resolvedValue))
            {
                continue;
            }

            values[configKey] = resolvedValue;
            secretKeys.Add(configKey);
        }

        _logger.LogInformation(
            "Configuration source '{SourceName}' loaded {Count} key(s)",
            SourceName,
            values.Count);

        return Task.FromResult(new InfrastructureConfigurationSourceResult
        {
            Values = values,
            SecretKeys = secretKeys
        });
    }
}

