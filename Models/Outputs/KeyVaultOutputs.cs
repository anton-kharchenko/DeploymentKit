namespace DeploymentKit.Models.Outputs;

public record KeyVaultOutputs
{
    public required Output<string> VaultUri { get; init; }
    public required Output<string> VaultName { get; init; }
    public required Output<string> ResourceId { get; init; }
    public required Output<string> TenantId { get; init; }
}

