namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from Table Storage service
/// </summary>
public class TableStorageOutputs
{
    public string Name { get; set; } = string.Empty;
    public Output<string> AccountName { get; set; } = null!;
    public Output<string> PrimaryKey { get; set; } = null!;
    public Output<string> ConnectionString { get; set; } = null!;
    public Output<string> TableEndpoint { get; set; } = null!;
    public Output<string> PrimaryTableEndpoint { get; set; } = null!;
    public List<string> TableNames { get; set; } = new();
    public Dictionary<string, Output<string>> TableUrls { get; set; } = new();
}
