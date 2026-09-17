namespace DeploymentKit.Models;

/// <summary>
/// Individual health check item result
/// </summary>
public class HealthCheckItem
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

