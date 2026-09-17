using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// Health check settings for auto-scaling
/// </summary>
public class HealthCheckSettings
{
    /// <summary>
    /// Health check endpoint path
    /// </summary>
    [StringLength(200, ErrorMessage = "Health check path cannot exceed 200 characters")]
    public string Path { get; set; } = "/health";

    /// <summary>
    /// Health check interval in seconds
    /// </summary>
    [Range(10, 300, ErrorMessage = "Health check interval must be between 10 and 300 seconds")]
    public int IntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Health check timeout in seconds
    /// </summary>
    [Range(5, 60, ErrorMessage = "Health check timeout must be between 5 and 60 seconds")]
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Number of consecutive failures before marking unhealthy
    /// </summary>
    [Range(1, 10, ErrorMessage = "Failure threshold must be between 1 and 10")]
    public int FailureThreshold { get; set; } = 3;

    /// <summary>
    /// Number of consecutive successes before marking healthy
    /// </summary>
    [Range(1, 10, ErrorMessage = "Success threshold must be between 1 and 10")]
    public int SuccessThreshold { get; set; } = 1;
}

