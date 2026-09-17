using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// Configuration settings for Container App ingress
/// </summary>
public class IngressSettings
{
    /// <summary>
    /// Whether ingress is externally accessible (true) or internal only (false)
    /// </summary>
    public bool External { get; set; }

    /// <summary>
    /// Target port that the container listens on
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Target port must be between 1 and 65535")]
    public int TargetPort { get; set; } = 8080;

    /// <summary>
    /// Allow insecure connections (HTTP). When false, enforces HTTPS only.
    /// </summary>
    public bool AllowInsecure { get; set; }

    /// <summary>
    /// Custom domains to bind to this Container App
    /// </summary>
    public List<CustomDomainSettings>? CustomDomains { get; set; }

    /// <summary>
    /// IP-based access restrictions for ingress
    /// </summary>
    public List<IpSecurityRestrictionSettings>? IpSecurityRestrictions { get; set; }

    /// <summary>
    /// Transport protocol (Http or Tcp)
    /// </summary>
    [StringLength(10, ErrorMessage = "Transport must be 'Http' or 'Tcp'")]
    public string Transport { get; set; } = "Http";

    /// <summary>
    /// Enable sticky sessions for load balancing
    /// </summary>
    public bool EnableStickySessions { get; set; }
}


