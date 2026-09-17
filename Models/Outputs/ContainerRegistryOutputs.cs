namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from Container Registry service
/// </summary>
public class ContainerRegistryOutputs
{
    public Output<string> LoginServer { get; set; } = null!;
    public Output<string> Username { get; set; } = null!;
    public Output<string> Password { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public Output<string> ResourceId { get; set; } = null!;
}

