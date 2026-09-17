namespace DeploymentKit.Models.Results;

/// <summary>
/// Result of a health check operation
/// </summary>
public class HealthCheckResult
{
    public string SlotName { get; set; } = string.Empty;
    public DateTime CheckTimestamp { get; set; }
    public bool IsHealthy { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public Dictionary<string, HealthCheckItem> Checks { get; set; } = new();
    public TimeSpan TotalCheckDuration { get; set; }
}

