namespace DeploymentKit.Settings;

/// <summary>
/// Generic options for orchestrating Pulumi stack lifecycle operations.
/// </summary>
public class DeploymentOrchestratorOptions
{
    public required string StackName { get; init; }

    public required string ProjectName { get; init; }

    public required string WorkingDirectory { get; init; }

    public string? EscEnvironment { get; init; }

    public global::Pulumi.Config? PulumiConfig { get; init; }

    public IDictionary<string, string>? Config { get; init; }

    public ISet<string>? SecretConfigKeys { get; init; }

    public IDictionary<string, string[]>? EnvironmentFallbackMappings { get; init; }

    public ISet<string>? RequiredConfigKeys { get; init; }

    public IDictionary<string, string?>? EnvironmentVariables { get; init; }

    public bool RefreshBeforeUpdate { get; init; }
}
