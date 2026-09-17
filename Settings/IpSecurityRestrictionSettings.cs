using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// IP-based access restriction rules for Container App ingress
/// </summary>
public class IpSecurityRestrictionSettings
{
    /// <summary>
    /// Friendly name for the restriction rule
    /// </summary>
    [Required(ErrorMessage = "Restriction name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// IP address range in CIDR notation (e.g., "192.168.1.0/24" or "10.0.0.5/32")
    /// </summary>
    [Required(ErrorMessage = "IP address range is required")]
    [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}\/\d{1,2}$",
        ErrorMessage = "IP address must be in CIDR notation (e.g., '192.168.1.0/24')")]
    public string IpAddressRange { get; set; } = string.Empty;

    /// <summary>
    /// Action to take for matching IPs: "Allow" or "Deny"
    /// </summary>
    [Required(ErrorMessage = "Action is required")]
    [RegularExpression("^(Allow|Deny)$", ErrorMessage = "Action must be 'Allow' or 'Deny'")]
    public string Action { get; set; } = "Allow";

    /// <summary>
    /// Description of the restriction rule
    /// </summary>
    [StringLength(500, ErrorMessage = "Description must be less than 500 characters")]
    public string? Description { get; set; }
}
