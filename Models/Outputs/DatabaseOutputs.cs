namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from Database service
/// </summary>
public class DatabaseOutputs
{
    public Output<string> HostName { get; set; } = null!;
    public Output<string> FullyQualifiedDomainName { get; set; } = null!;
    public Output<string> ConnectionString { get; set; } = null!;
    public Output<string> Password { get; set; } = null!;
    public string ServerName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
}

