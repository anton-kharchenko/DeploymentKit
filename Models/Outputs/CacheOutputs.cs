namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from Cache service
/// </summary>
public class CacheOutputs
{
    public Output<string> HostName { get; set; } = null!;
    public Output<string> ConnectionString { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public Output<string> ResourceId { get; set; } = null!;
    public Output<int> RedisPort { get; set; } = null!;
    // Alias properties for backward compatibility
    public Output<int> Port => RedisPort;
}

