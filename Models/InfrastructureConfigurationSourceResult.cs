namespace DeploymentKit.Models;

/// <summary>
/// Represents configuration values loaded from a specific configuration source.
/// </summary>
public sealed class InfrastructureConfigurationSourceResult
{
    public IDictionary<string, string> Values { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public ISet<string> SecretKeys { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}

