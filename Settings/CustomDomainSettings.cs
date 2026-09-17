using DeploymentKit.Enums;
using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// Configuration for custom domain automation including DNS, SSL certificates, and domain validation
/// </summary>
public class CustomDomainSettings
{
    /// <summary>
    /// The custom domain name (e.g., api-dev.example.com, api.example.com)
    /// </summary>
    [Required(ErrorMessage = "Domain name is required")]
    [StringLength(253, MinimumLength = 4, ErrorMessage = "Domain name must be between 4 and 253 characters")]
    [RegularExpression(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)*$", ErrorMessage = "Invalid domain name format")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Enable custom domain configuration (default: false)
    /// When enabled, DNS records and SSL certificates will be automatically provisioned
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// SSL/TLS certificate source type
    /// </summary>
    public CertificateSourceType CertificateSource { get; set; } = CertificateSourceType.Managed;

    /// <summary>
    /// Certificate mode: "Managed" for Azure-managed Let's Encrypt certificates, or provide certificate ID for custom certificates
    /// (Deprecated: Use CertificateSource instead)
    /// </summary>
    [StringLength(500, ErrorMessage = "Certificate mode or ID must be less than 500 characters")]
    public string CertificateMode { get; set; } = "Managed";

    /// <summary>
    /// Domain validation method for certificate issuance
    /// </summary>
    public DomainValidationType ValidationType { get; set; } = DomainValidationType.HTTP;

    /// <summary>
    /// Validation method for managed certificates: "HTTP" or "CNAME"
    /// (Deprecated: Use ValidationType instead)
    /// </summary>
    [StringLength(10, ErrorMessage = "Validation method must be 'HTTP' or 'CNAME'")]
    public string ValidationMethod { get; set; } = "HTTP";

    /// <summary>
    /// Azure Key Vault certificate name (required when CertificateSource is KeyVault)
    /// </summary>
    [StringLength(127, ErrorMessage = "Certificate name cannot exceed 127 characters")]
    public string? KeyVaultCertificateName { get; set; }

    /// <summary>
    /// Local path to .pfx certificate file (required when CertificateSource is Upload)
    /// </summary>
    [StringLength(500, ErrorMessage = "Certificate path cannot exceed 500 characters")]
    public string? CertificateFilePath { get; set; }

    /// <summary>
    /// Password for .pfx certificate file (required when CertificateSource is Upload and cert is password-protected)
    /// </summary>
    [StringLength(256, ErrorMessage = "Certificate password cannot exceed 256 characters")]
    public string? CertificatePassword { get; set; }

    /// <summary>
    /// Enable automatic DNS record creation (A, CNAME, TXT, CAA)
    /// Default: true when Enabled is true
    /// </summary>
    public bool CreateDnsRecords { get; set; } = true;

    /// <summary>
    /// Create A record pointing to Application Gateway public IP
    /// Default: true
    /// </summary>
    public bool CreateARecord { get; set; } = true;

    /// <summary>
    /// Create CNAME record for www subdomain (www.yourdomain.com -> yourdomain.com)
    /// Default: false
    /// </summary>
    public bool CreateWwwCname { get; set; }

    /// <summary>
    /// Create CAA records to restrict certificate issuance to specific authorities
    /// Default: true
    /// </summary>
    public bool CreateCaaRecords { get; set; } = true;

    /// <summary>
    /// Certificate authorities allowed to issue certificates (for CAA records)
    /// Default: letsencrypt.org (Azure Managed Certificates uses Let's Encrypt)
    /// </summary>
    public List<string> AllowedCertificateAuthorities { get; set; } = new() { "letsencrypt.org" };

    /// <summary>
    /// TTL (Time To Live) for DNS records in seconds
    /// Default: 3600 (1 hour)
    /// </summary>
    [Range(60, 86400, ErrorMessage = "TTL must be between 60 seconds and 86400 seconds (24 hours)")]
    public int DnsRecordTtl { get; set; } = 3600;

    /// <summary>
    /// Wait for DNS propagation before proceeding with certificate issuance
    /// Default: true
    /// </summary>
    public bool WaitForDnsPropagation { get; set; } = true;

    /// <summary>
    /// Maximum wait time for DNS propagation in seconds
    /// Default: 300 (5 minutes)
    /// </summary>
    [Range(30, 1800, ErrorMessage = "DNS propagation timeout must be between 30 seconds and 1800 seconds (30 minutes)")]
    public int DnsPropagationTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Bind domain to Application Gateway
    /// Default: true
    /// </summary>
    public bool BindToApplicationGateway { get; set; } = true;

    /// <summary>
    /// Bind domain to Container Apps
    /// Default: false (requires post-deployment manual binding due to Pulumi limitations)
    /// </summary>
    public bool BindToContainerApps { get; set; }

    /// <summary>
    /// Enable HTTP to HTTPS redirect
    /// Default: true
    /// </summary>
    public bool EnableHttpsRedirect { get; set; } = true;

    /// <summary>
    /// DNS Zone resource group name (if different from infrastructure resource group)
    /// Leave null to use the same resource group as infrastructure
    /// </summary>
    [StringLength(90, ErrorMessage = "Resource group name must be less than 90 characters")]
    public string? DnsZoneResourceGroup { get; set; }
}



