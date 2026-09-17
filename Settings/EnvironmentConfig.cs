using DeploymentKit.Enums;
using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// Comprehensive environment configuration for infrastructure deployment
/// </summary>
public class EnvironmentConfig
{
    /// <summary>
    /// Deployment name
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(64, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 64 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Environment type (Development, Staging, Production)
    /// </summary>
    [Required(ErrorMessage = "Environment type is required")]
    public EnvironmentType Type { get; set; }

    /// <summary>
    /// Azure resource group name
    /// </summary>
    [Required(ErrorMessage = "Resource group name is required")]
    [StringLength(90, MinimumLength = 1, ErrorMessage = "Resource group name must be between 1 and 90 characters")]
    public string ResourceGroupName { get; set; } = string.Empty;

    /// <summary>
    /// Naming prefix for Azure resources
    /// </summary>
    [Required(ErrorMessage = "Naming prefix is required")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "Naming prefix must be between 1 and 10 characters")]
    public string NamingPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Path to environment file (.env.development, .env.production)
    /// </summary>
    [Required(ErrorMessage = "Environment file path is required")]
    public string EnvFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Custom domain for the deployment (e.g., "api.example.com")
    /// </summary>
    [Required(ErrorMessage = "Custom domain is required")]
    [StringLength(253, MinimumLength = 1, ErrorMessage = "Custom domain must be between 1 and 253 characters")]
    public string CustomDomain { get; set; } = string.Empty;

    /// <summary>
    /// Patterns to exclude when loading environment variables
    /// </summary>
    [Required(ErrorMessage = "Exclude patterns are required")]
    public string[] ExcludePatterns { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Key Vault configuration
    /// </summary>
    [Required(ErrorMessage = "KeyVault settings are required")]
    public KeyVaultSettings KeyVaultSettings { get; set; } = new();

    /// <summary>
    /// Database configuration
    /// </summary>
    [Required(ErrorMessage = "Database settings are required")]
    public DatabaseSettings DatabaseSettings { get; set; } = new();

    /// <summary>
    /// Storage account configuration
    /// </summary>
    [Required(ErrorMessage = "Storage settings are required")]
    public StorageSettings StorageSettings { get; set; } = new();

    /// <summary>
    /// Blob storage configuration
    /// </summary>
    [Required(ErrorMessage = "Blob storage settings are required")]
    public BlobStorageSettings BlobStorageSettings { get; set; } = new();

    /// <summary>
    /// Event Hubs (message broker) configuration
    /// </summary>
    [Required(ErrorMessage = "Event Hubs settings are required")]
    public EventHubsSettings EventHubsSettings { get; set; } = new();

    /// <summary>
    /// Application Insights and monitoring configuration
    /// </summary>
    [Required(ErrorMessage = "Monitoring settings are required")]
    public MonitoringSettings MonitoringSettings { get; set; } = new();

    /// <summary>
    /// Virtual network and networking configuration
    /// </summary>
    [Required(ErrorMessage = "Network settings are required")]
    public NetworkSettings NetworkSettings { get; set; } = new();

    /// <summary>
    /// Container Apps configuration
    /// </summary>
    [Required(ErrorMessage = "Container settings are required")]
    public ContainerSettings ContainerSettings { get; set; } = new();
}

