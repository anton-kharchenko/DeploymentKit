namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from Storage service
/// </summary>
public class StorageOutputs
{
    public Output<string> AccountName { get; set; } = null!;
    public Output<string> PrimaryKey { get; set; } = null!;
    public Output<string> ConnectionString { get; set; } = null!;
    public Output<string> WebsiteAccountName { get; set; } = null!;
    public Output<string> WebsitePrimaryEndpoint { get; set; } = null!;
    public Output<string> MiniAppAccountName { get; set; } = null!;
    public Output<string> MiniAppPrimaryEndpoint { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
}

