namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from Blob Storage service
/// </summary>
public class BlobStorageOutputs
{
    public string Name { get; set; } = string.Empty;
    public Output<string> AccountName { get; set; } = null!;
    public Output<string> PrimaryKey { get; set; } = null!;
    public Output<string> ConnectionString { get; set; } = null!;
    public Output<string> BlobEndpoint { get; set; } = null!;
    public Output<string> PrimaryBlobEndpoint { get; set; } = null!;
    public List<string> ContainerNames { get; set; } = new();
    public Dictionary<string, Output<string>> ContainerUrls { get; set; } = new();
}
