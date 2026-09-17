namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Output information for a deployment slot
/// </summary>
public class SlotOutputs
{
    /// <summary>
    /// The slot name (green or blue)
    /// </summary>
    public string SlotName { get; set; } = string.Empty;

    /// <summary>
    /// Container app name for this slot
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// Container app resource ID
    /// </summary>
    public Output<string> AppId { get; set; } = Output.Create(string.Empty);

    /// <summary>
    /// Container app URL for this slot
    /// </summary>
    public Output<string> AppUrl { get; set; } = Output.Create(string.Empty);

    /// <summary>
    /// Internal FQDN for this slot
    /// </summary>
    public Output<string> InternalFqdn { get; set; } = Output.Create(string.Empty);

    /// <summary>
    /// Current traffic percentage for this slot
    /// </summary>
    public int TrafficPercentage { get; set; }

    /// <summary>
    /// Indicates if this slot is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Container image tag deployed to this slot
    /// </summary>
    public string ImageTag { get; set; } = string.Empty;

    /// <summary>
    /// Deployment timestamp
    /// </summary>
    public DateTime DeploymentTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Version identifier for this deployment
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Health check endpoint URL
    /// </summary>
    public Output<string> HealthCheckUrl { get; set; } = Output.Create(string.Empty);

    /// <summary>
    /// Current health status
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTime? LastHealthCheckTimestamp { get; set; }
}

