using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// Configuration settings for green-blue deployment strategy
/// </summary>
public class GreenBlueDeploymentSettings
{
    /// <summary>
    /// Indicates whether green-blue deployment is enabled
    /// </summary>
    [Required]
    public bool Enabled { get; set; }

    /// <summary>
    /// Enable green-blue deployment (alias for Enabled property)
    /// </summary>
    public bool EnableGreenBlueDeployment { get; set; }

    /// <summary>
    /// Traffic split percentage for green-blue deployment
    /// </summary>
    [Range(0, 100, ErrorMessage = "Traffic split percentage must be between 0 and 100")]
    public int TrafficSplitPercentage { get; set; } = 100;

    /// <summary>
    /// Health check interval in seconds
    /// </summary>
    [Range(5, 300, ErrorMessage = "Health check interval must be between 5 and 300 seconds")]
    public int HealthCheckIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// The currently active deployment slot (green or blue)
    /// </summary>
    [Required(ErrorMessage = "Active slot is required when green-blue deployment is enabled")]
    [RegularExpression(@"^(green|blue)$", ErrorMessage = "Active slot must be either 'green' or 'blue'")]
    public string ActiveSlot { get; set; } = "green";

    /// <summary>
    /// The target slot for new deployments (opposite of active slot)
    /// </summary>
    [Required(ErrorMessage = "Target slot is required when green-blue deployment is enabled")]
    [RegularExpression(@"^(green|blue)$", ErrorMessage = "Target slot must be either 'green' or 'blue'")]
    public string TargetSlot { get; set; } = "blue";

    /// <summary>
    /// Traffic percentage to route to the active slot (0-100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Active slot traffic percentage must be between 0 and 100")]
    public int ActiveSlotTrafficPercentage { get; set; } = 100;

    /// <summary>
    /// Traffic percentage to route to the target slot (0-100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Target slot traffic percentage must be between 0 and 100")]
    public int TargetSlotTrafficPercentage { get; set; } = 0;

    /// <summary>
    /// Enable automatic slot switching after successful deployment
    /// </summary>
    public bool AutoSwitchSlots { get; set; }

    /// <summary>
    /// Enable health checks before slot switching
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;

    /// <summary>
    /// Health check endpoint path
    /// </summary>
    [StringLength(200, ErrorMessage = "Health check path must not exceed 200 characters")]
    public string HealthCheckPath { get; set; } = "/health";

    /// <summary>
    /// Health check timeout in seconds
    /// </summary>
    [Range(5, 300, ErrorMessage = "Health check timeout must be between 5 and 300 seconds")]
    public int HealthCheckTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Number of health check retries before considering deployment failed
    /// </summary>
    [Range(1, 10, ErrorMessage = "Health check retries must be between 1 and 10")]
    public int HealthCheckRetries { get; set; } = 3;

    /// <summary>
    /// Delay between health check retries in seconds
    /// </summary>
    [Range(1, 60, ErrorMessage = "Health check retry delay must be between 1 and 60 seconds")]
    public int HealthCheckRetryDelaySeconds { get; set; } = 10;

    /// <summary>
    /// Enable gradual traffic shifting (canary deployment)
    /// </summary>
    public bool EnableGradualTrafficShift { get; set; }

    /// <summary>
    /// Traffic shift increment percentage for gradual deployment
    /// </summary>
    [Range(1, 50, ErrorMessage = "Traffic shift increment must be between 1 and 50 percent")]
    public int TrafficShiftIncrementPercentage { get; set; } = 10;

    /// <summary>
    /// Delay between traffic shift increments in minutes
    /// </summary>
    [Range(1, 60, ErrorMessage = "Traffic shift delay must be between 1 and 60 minutes")]
    public int TrafficShiftDelayMinutes { get; set; } = 5;

    /// <summary>
    /// Maximum traffic shift percentage for gradual deployment
    /// </summary>
    [Range(1, 100, ErrorMessage = "Max traffic shift percentage must be between 1 and 100")]
    public int MaxTrafficShiftPercentage { get; set; } = 50;

    /// <summary>
    /// Rollback threshold percentage for automatic rollback
    /// </summary>
    [Range(0, 100, ErrorMessage = "Rollback threshold percentage must be between 0 and 100")]
    public int RollbackThresholdPercentage { get; set; } = 10;

    /// <summary>
    /// Health check timeout in seconds
    /// </summary>
    [Range(5, 300, ErrorMessage = "Health check timeout must be between 5 and 300 seconds")]
    public int HealthCheckTimeout { get; set; } = 30;

    /// <summary>
    /// Traffic shift interval in minutes
    /// </summary>
    [Range(1, 60, ErrorMessage = "Traffic shift interval must be between 1 and 60 minutes")]
    public int TrafficShiftInterval { get; set; } = 5;

    /// <summary>
    /// Validates that traffic percentages sum to 100
    /// </summary>
    public bool IsTrafficDistributionValid()
    {
        return ActiveSlotTrafficPercentage + TargetSlotTrafficPercentage == 100;
    }

    /// <summary>
    /// Validates that active and target slots are different
    /// </summary>
    public bool AreSlotsValid()
    {
        return !string.Equals(ActiveSlot, TargetSlot, StringComparison.OrdinalIgnoreCase);
    }
}


