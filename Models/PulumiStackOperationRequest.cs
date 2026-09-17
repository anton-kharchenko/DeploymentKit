namespace DeploymentKit.Models;

/// <summary>
/// Request model for executing Pulumi Automation API operations against a local Pulumi program.
/// </summary>
public sealed class PulumiStackOperationRequest
{
    public required string StackName { get; init; }
    public required string ProjectName { get; init; }
    public required string WorkingDirectory { get; init; }
    public string? EscEnvironment { get; init; }
    public IDictionary<string, string>? Config { get; init; }
    public ISet<string>? SecretConfigKeys { get; init; }
    public IDictionary<string, string?>? EnvironmentVariables { get; init; }
    public bool RefreshBeforeUpdate { get; init; }
}
