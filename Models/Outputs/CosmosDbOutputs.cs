namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from Cosmos DB service
/// </summary>
public class CosmosDbOutputs
{
    public string Name { get; set; } = string.Empty;
    public Output<string> AccountName { get; set; } = null!;
    public Output<string> Endpoint { get; set; } = null!;
    public Output<string> PrimaryKey { get; set; } = null!;
    public Output<string> ConnectionString { get; set; } = null!;
    public Output<string> DocumentEndpoint { get; set; } = null!;
    public string DatabaseName { get; set; } = string.Empty;
    public List<string> ContainerNames { get; set; } = new();
    public Dictionary<string, int> ContainerThroughput { get; set; } = new();
}
