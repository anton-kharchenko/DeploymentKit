using DeploymentKit.Constants;
using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

public class ContainerSettings
{
    [Required(ErrorMessage = "API image tag is required")]
    [StringLength(256, MinimumLength = 1, ErrorMessage = "API image tag must be between 1 and 256 characters")]
    public string ApiImageTag { get; set; } = InfrastructureConstants.Defaults.ApiImageTag;

    [Required(ErrorMessage = "Jobs image tag is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Jobs image tag must be between 1 and 200 characters")]
    public string JobsImageTag { get; set; } = InfrastructureConstants.Defaults.JobsImageTag;

    [StringLength(200, MinimumLength = 1, ErrorMessage = "Bot image tag must be between 1 and 200 characters")]
    public string BotImageTag { get; set; } = string.Empty;

    [Required(ErrorMessage = "Auto-scaling settings are required")]
    public AutoScalingSettings AutoScaling { get; set; } = new();

    /// <summary>
    /// Minimum number of replicas
    /// </summary>
    [Range(1, 100, ErrorMessage = "Minimum replicas must be between 1 and 100")]
    public int MinReplicas { get; set; } = 1;

    /// <summary>
    /// Maximum number of replicas
    /// </summary>
    [Range(1, 100, ErrorMessage = "Maximum replicas must be between 1 and 100")]
    public int MaxReplicas { get; set; } = 10;

    /// <summary>
    /// Enable Dapr integration
    /// </summary>
    public bool EnableDapr { get; set; }

    /// <summary>
    /// CPU limit for containers (in cores)
    /// </summary>
    [Range(0.1, 4.0, ErrorMessage = "CPU limit must be between 0.1 and 4.0 cores")]
    public double CpuLimit { get; set; } = 1.0;

    /// <summary>
    /// Memory limit for containers (in GB)
    /// </summary>
    [Range(0.5, 8.0, ErrorMessage = "Memory limit must be between 0.5 and 8.0 GB")]
    public double MemoryLimit { get; set; } = 2.0;

    /// <summary>
    /// Memory limit for containers (as string for Kubernetes format)
    /// </summary>
    [Required(ErrorMessage = "Memory limit string is required")]
    public string MemoryLimitString { get; set; } = "2Gi";

    /// <summary>
    /// CPU limit for API containers (in cores)
    /// </summary>
    [Range(0.1, 4.0, ErrorMessage = "API CPU limit must be between 0.1 and 4.0 cores")]
    public double ApiCpuLimit { get; set; } = 1.0;

    /// <summary>
    /// Memory limit for API containers (in GB)
    /// </summary>
    [Range(0.5, 8.0, ErrorMessage = "API Memory limit must be between 0.5 and 8.0 GB")]
    public double ApiMemoryLimit { get; set; } = 2.0;

    /// <summary>
    /// Optional override for the container registry name
    /// </summary>
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Registry name must be between 5 and 50 characters")]
    public string? RegistryNameOverride { get; set; }

    /// <summary>
    /// Use placeholder images when specified images are not available in ACR.
    /// When true, uses mcr.microsoft.com/azuredocs/containerapps-helloworld:latest as a fallback.
    /// This is useful for initial infrastructure deployment before application images are pushed.
    /// </summary>
    public bool UsePlaceholderImages { get; set; } = true;

    /// <summary>
    /// Ingress configuration for Container Apps (external access, custom domains, IP restrictions)
    /// </summary>
    public IngressSettings? IngressSettings { get; set; }
}



