namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from Container Registry service
/// </summary>
public class ContainerRegistryOutputs
{
    public Output<string> LoginServer { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public Output<string> ResourceId { get; set; } = null!;
}

