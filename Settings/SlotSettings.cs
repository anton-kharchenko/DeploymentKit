using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// Configuration settings for individual deployment slots (green/blue)
/// </summary>
public class SlotSettings
{
    /// <summary>
    /// The slot name (green or blue)
    /// </summary>
    [Required(ErrorMessage = "Slot name is required")]
    [RegularExpression("^(green|blue)$", ErrorMessage = "Slot name must be either 'green' or 'blue'")]
    public string SlotName { get; set; } = string.Empty;

    /// <summary>
    /// Container image tag for this slot
    /// </summary>
    [Required(ErrorMessage = "Image tag is required")]
    [StringLength(256, MinimumLength = 1, ErrorMessage = "Image tag must be between 1 and 256 characters")]
    public string ImageTag { get; set; } = "latest";

    /// <summary>
    /// Environment variables specific to this slot
    /// </summary>
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();

    /// <summary>
    /// CPU allocation for this slot
    /// </summary>
    [Range(0.1, 4.0, ErrorMessage = "CPU allocation must be between 0.1 and 4.0 cores")]
    public double CpuAllocation { get; set; } = 1.0;

    /// <summary>
    /// Memory allocation for this slot
    /// </summary>
    [Required(ErrorMessage = "Memory allocation is required")]
    [RegularExpression(@"^\d+(\.\d+)?(Mi|Gi)$", ErrorMessage = "Memory allocation must be in format like '512Mi' or '1Gi'")]
    public string MemoryAllocation { get; set; } = "1Gi";

    /// <summary>
    /// CPU limit for this slot (for validation purposes)
    /// </summary>
    [Range(0.1, 4.0, ErrorMessage = "CPU limit must be between 0.1 and 4.0 cores")]
    public double CpuLimit { get; set; } = 1.0;

    /// <summary>
    /// Memory limit for this slot (for validation purposes)
    /// </summary>
    [Range(0.1, 8.0, ErrorMessage = "Memory limit must be between 0.1 and 8.0 GB")]
    public double MemoryLimit { get; set; } = 1.0;

    /// <summary>
    /// Memory limit for this slot (as string for Kubernetes format)
    /// </summary>
    [Required(ErrorMessage = "Memory limit string is required")]
    [RegularExpression(@"^\d+(\.\d+)?(Mi|Gi)$", ErrorMessage = "Memory limit must be in format like '512Mi' or '1Gi'")]
    public string MemoryLimitString { get; set; } = "1Gi";

    /// <summary>
    /// Minimum number of replicas for this slot
    /// </summary>
    [Range(0, 100, ErrorMessage = "Minimum replicas must be between 0 and 100")]
    public int MinReplicas { get; set; } = 1;

    /// <summary>
    /// Maximum number of replicas for this slot
    /// </summary>
    [Range(1, 100, ErrorMessage = "Maximum replicas must be between 1 and 100")]
    public int MaxReplicas { get; set; } = 10;

    /// <summary>
    /// Indicates if this slot is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Traffic percentage allocated to this slot
    /// </summary>
    [Range(0, 100, ErrorMessage = "Traffic percentage must be between 0 and 100")]
    public int TrafficPercentage { get; set; } = 0;

    /// <summary>
    /// Custom configuration overrides for this slot
    /// </summary>
    public Dictionary<string, object> ConfigurationOverrides { get; set; } = new();

    /// <summary>
    /// Deployment timestamp for this slot
    /// </summary>
    public DateTime? DeploymentTimestamp { get; set; }

    /// <summary>
    /// Version identifier for this slot deployment
    /// </summary>
    [StringLength(100, ErrorMessage = "Version must not exceed 100 characters")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Version identifier for this slot deployment (string property for consistency)
    /// </summary>
    [StringLength(100, ErrorMessage = "VersionString must not exceed 100 characters")]
    public string VersionString { get; set; } = string.Empty;

    /// <summary>
    /// Health check status for this slot
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTime? LastHealthCheckTimestamp { get; set; }

    /// <summary>
    /// Validates that min replicas is not greater than max replicas
    /// </summary>
    public bool IsReplicaConfigurationValid()
    {
        return MinReplicas <= MaxReplicas;
    }
}



