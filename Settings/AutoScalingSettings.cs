using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// Auto-scaling configuration settings for Container Apps
/// </summary>
public class AutoScalingSettings
{
    /// <summary>
    /// Minimum number of replicas
    /// </summary>
    [Range(0, 100, ErrorMessage = "Minimum replicas must be between 0 and 100")]
    public int MinReplicas { get; set; } = 1;

    /// <summary>
    /// Maximum number of replicas
    /// </summary>
    [Range(1, 300, ErrorMessage = "Maximum replicas must be between 1 and 300")]
    public int MaxReplicas { get; set; } = 10;

    /// <summary>
    /// CPU utilization threshold for scaling (percentage)
    /// </summary>
    [Range(1, 100, ErrorMessage = "CPU threshold must be between 1 and 100")]
    public int CpuThreshold { get; set; } = 70;

    /// <summary>
    /// Memory utilization threshold for scaling (percentage)
    /// </summary>
    [Range(1, 100, ErrorMessage = "Memory threshold must be between 1 and 100")]
    public int MemoryThreshold { get; set; } = 80;

    /// <summary>
    /// HTTP request count threshold for scaling
    /// </summary>
    [Range(1, 10000, ErrorMessage = "HTTP threshold must be between 1 and 10000")]
    public int HttpRequestThreshold { get; set; } = 100;

    /// <summary>
    /// Scale-out cooldown period in seconds
    /// </summary>
    [Range(60, 3600, ErrorMessage = "Scale-out cooldown must be between 60 and 3600 seconds")]
    public int ScaleOutCooldown { get; set; } = 300;

    /// <summary>
    /// Scale-in cooldown period in seconds
    /// </summary>
    [Range(60, 3600, ErrorMessage = "Scale-in cooldown must be between 60 and 3600 seconds")]
    public int ScaleInCooldown { get; set; } = 600;

    /// <summary>
    /// Enable automatic scaling based on CPU utilization
    /// </summary>
    public bool EnableCpuScaling { get; set; } = true;

    /// <summary>
    /// Enable automatic scaling based on memory utilization
    /// </summary>
    public bool EnableMemoryScaling { get; set; } = true;

    /// <summary>
    /// Enable automatic scaling based on HTTP request count
    /// </summary>
    public bool EnableHttpScaling { get; set; } = true;

    /// <summary>
    /// Enable predictive scaling based on historical patterns
    /// </summary>
    public bool EnablePredictiveScaling { get; set; }

    /// <summary>
    /// Health check configuration for scaling decisions
    /// </summary>
    public HealthCheckSettings HealthCheck { get; set; } = new();
}



