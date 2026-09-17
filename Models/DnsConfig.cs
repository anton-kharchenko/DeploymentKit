using System.ComponentModel.DataAnnotations;
using Pulumi;

namespace DeploymentKit.Models;

/// <summary>
/// Configuration for DNS and custom domain binding
/// </summary>
public class DnsConfig
{
    /// <summary>
    /// Custom domain prefix (e.g., "api-dev" for "api-dev.example.com")
    /// </summary>
    [Required(ErrorMessage = "Custom domain is required")]
    [StringLength(63, MinimumLength = 1, ErrorMessage = "Custom domain must be between 1 and 63 characters")]
    public string CustomDomain { get; set; } = string.Empty;

    /// <summary>
    /// DNS zone name (e.g., "example.com")
    /// </summary>
    [Required(ErrorMessage = "Zone name is required")]
    [StringLength(253, MinimumLength = 1, ErrorMessage = "Zone name must be between 1 and 253 characters")]
    public string ZoneName { get; set; } = string.Empty;

    /// <summary>
    /// Container App name for domain binding
    /// </summary>
    [Required(ErrorMessage = "Container App name is required")]
    [StringLength(64, MinimumLength = 1, ErrorMessage = "Container App name must be between 1 and 64 characters")]
    public string ContainerAppName { get; set; } = string.Empty;

    /// <summary>
    /// Resource group name where DNS zone and Container App reside
    /// </summary>
    [Required(ErrorMessage = "Resource group name is required")]
    [StringLength(90, MinimumLength = 1, ErrorMessage = "Resource group name must be between 1 and 90 characters")]
    public string ResourceGroupName { get; set; } = string.Empty;

    /// <summary>
    /// Container Apps Environment name
    /// </summary>
    [Required(ErrorMessage = "Environment name is required")]
    [StringLength(64, MinimumLength = 1, ErrorMessage = "Environment name must be between 1 and 64 characters")]
    public string EnvironmentName { get; set; } = string.Empty;

    /// <summary>
    /// Fully qualified domain name of the Container App (e.g., "myapp.region.azurecontainerapps.io")
    /// </summary>
    [Required(ErrorMessage = "Container App FQDN is required")]
    public Input<string> ContainerAppFqdn { get; set; } = Output.Create(string.Empty);

    /// <summary>
    /// Gets the full domain name (CustomDomain.ZoneName)
    /// </summary>
    public string FullDomainName => $"{CustomDomain}.{ZoneName}";
}

