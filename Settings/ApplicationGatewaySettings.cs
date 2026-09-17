using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// Configuration settings for Azure Application Gateway
/// </summary>
public class ApplicationGatewaySettings
{
    /// <summary>
    /// Whether Application Gateway is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Name of the Application Gateway
    /// </summary>
    [StringLength(80, MinimumLength = 1, ErrorMessage = "Application Gateway name must be between 1 and 80 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// Custom domain name for the application
    /// </summary>
    [StringLength(253, ErrorMessage = "Domain name cannot exceed 253 characters")]
    public string? CustomDomain { get; set; }

    /// <summary>
    /// SSL certificate name for HTTPS configuration
    /// </summary>
    [StringLength(80, ErrorMessage = "SSL certificate name cannot exceed 80 characters")]
    public string? SslCertificateName { get; set; }

    /// <summary>
    /// Enable Web Application Firewall (WAF)
    /// </summary>
    public bool EnableWaf { get; set; } = true;

    /// <summary>
    /// WAF mode (Detection or Prevention)
    /// </summary>
    [RegularExpression(@"^(Detection|Prevention)$", ErrorMessage = "WAF mode must be Detection or Prevention")]
    public string WafMode { get; set; } = "Prevention";

    /// <summary>
    /// Minimum capacity for autoscaling
    /// </summary>
    [Range(0, 125, ErrorMessage = "Minimum capacity must be between 0 and 125")]
    public int MinCapacity { get; set; } = 0;

    /// <summary>
    /// Maximum capacity for autoscaling
    /// </summary>
    [Range(2, 125, ErrorMessage = "Maximum capacity must be between 2 and 125")]
    public int MaxCapacity { get; set; } = 10;
}
