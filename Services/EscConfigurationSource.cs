using DeploymentKit.Interfaces;
using DeploymentKit.Models;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Loads deployment configuration from ESC-backed values and Pulumi config.
/// </summary>
public sealed class EscConfigurationSource(ILogger<EscConfigurationSource> logger) : IInfrastructureConfigurationSource
{
    private readonly ILogger<EscConfigurationSource> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public string SourceName => "esc";

    public Task<InfrastructureConfigurationSourceResult> LoadAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var secretKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (options.Config != null)
        {
            foreach (var (key, value) in options.Config)
            {
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                values[key] = value;
                if (options.SecretConfigKeys?.Contains(key) == true)
                {
                    secretKeys.Add(key);
                }
            }
        }

        if (options.PulumiConfig != null)
        {
            var keysToLoad = ResolveKeysToLoad(options);
            foreach (var key in keysToLoad)
            {
                if (values.ContainsKey(key))
                {
                    continue;
                }

                var value = options.PulumiConfig.Get(key);
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                values[key] = value;
                secretKeys.Add(key);
            }
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

    private static IReadOnlyCollection<string> ResolveKeysToLoad(DeploymentOrchestratorOptions options)
    {
        if (options.RequiredConfigKeys is { Count: > 0 })
        {
            return options.RequiredConfigKeys.ToArray();
        }

        if (options.EnvironmentFallbackMappings is { Count: > 0 })
        {
            return options.EnvironmentFallbackMappings.Keys.ToArray();
        }

        return Array.Empty<string>();
    }
}

