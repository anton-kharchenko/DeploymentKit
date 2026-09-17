using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// Network configuration settings for private networking
/// </summary>
public class NetworkSettings
{
    /// <summary>
    /// Virtual Network address space (CIDR)
    /// </summary>
    [Required(ErrorMessage = "VNet address space is required")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}\/[0-9]{1,2}$", ErrorMessage = "Invalid CIDR format")]
    public string VNetAddressSpace { get; set; } = "10.0.0.0/16";

    /// <summary>
    /// Virtual Network address space (alias for backward compatibility)
    /// </summary>
    [Required(ErrorMessage = "Virtual Network address space is required")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}\/[0-9]{1,2}$", ErrorMessage = "Invalid CIDR format")]
    public string VirtualNetworkAddressSpace { get; set; } = "10.0.0.0/16";

    /// <summary>
    /// Container Apps subnet address space
    /// </summary>
    [Required(ErrorMessage = "Container Apps subnet is required")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}\/[0-9]{1,2}$", ErrorMessage = "Invalid CIDR format")]
    public string ContainerAppsSubnet { get; set; } = "10.0.0.0/23";

    /// <summary>
    /// Container Apps subnet address space (alias for backward compatibility)
    /// </summary>
    [Required(ErrorMessage = "Container Apps subnet address space is required")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}\/[0-9]{1,2}$", ErrorMessage = "Invalid CIDR format")]
    public string ContainerAppsSubnetAddressSpace { get; set; } = "10.0.0.0/23";

    /// <summary>
    /// Database subnet address space
    /// </summary>
    [Required(ErrorMessage = "Database subnet is required")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}\/[0-9]{1,2}$", ErrorMessage = "Invalid CIDR format")]
    public string DatabaseSubnet { get; set; } = "10.0.2.0/24";

    /// <summary>
    /// Database subnet address space (alias for backward compatibility)
    /// </summary>
    [Required(ErrorMessage = "Database subnet address space is required")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}\/[0-9]{1,2}$", ErrorMessage = "Invalid CIDR format")]
    public string DatabaseSubnetAddressSpace { get; set; } = "10.0.2.0/24";

    /// <summary>
    /// Cache subnet address space
    /// </summary>
    [Required(ErrorMessage = "Cache subnet address space is required")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}\/[0-9]{1,2}$", ErrorMessage = "Invalid CIDR format")]
    public string CacheSubnetAddressSpace { get; set; } = "10.0.5.0/24";

    /// <summary>
    /// Private endpoints subnet address space
    /// </summary>
    [Required(ErrorMessage = "Private endpoints subnet is required")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}\/[0-9]{1,2}$", ErrorMessage = "Invalid CIDR format")]
    public string PrivateEndpointsSubnet { get; set; } = "10.0.3.0/24";

    /// <summary>
    /// Application Gateway subnet address space
    /// </summary>
    [Required(ErrorMessage = "Application Gateway subnet is required")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}\/[0-9]{1,2}$", ErrorMessage = "Invalid CIDR format")]
    public string ApplicationGatewaySubnet { get; set; } = "10.0.4.0/24";

    /// <summary>
    /// Application Gateway subnet address space (alias for backward compatibility)
    /// </summary>
    [Required(ErrorMessage = "Application Gateway subnet address space is required")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}\/[0-9]{1,2}$", ErrorMessage = "Invalid CIDR format")]
    public string ApplicationGatewaySubnetAddressSpace { get; set; } = "10.0.4.0/24";

    /// <summary>
    /// Enable private endpoints for all services
    /// </summary>
    public bool EnablePrivateEndpoints { get; set; } = true;

    /// <summary>
    /// Enable DDoS protection for the virtual network
    /// </summary>
    public bool EnableDdosProtection { get; set; }

    /// <summary>
    /// Enable network security groups
    /// </summary>
    public bool EnableNetworkSecurityGroups { get; set; } = true;

    /// <summary>
    /// Enable private DNS zones
    /// </summary>
    public bool EnablePrivateDnsZones { get; set; } = true;

    /// <summary>
    /// Custom domain name for the application
    /// </summary>
    [StringLength(253, ErrorMessage = "Domain name cannot exceed 253 characters")]
    public string? CustomDomain { get; set; } = "api.example.com";

    /// <summary>
    /// Enable Application Gateway for external access
    /// </summary>
    public bool EnableApplicationGateway { get; set; } = true;

    /// <summary>
    /// Enable CDN Profile for content delivery optimization
    /// Default is false due to Azure deprecating classic CDN SKUs
    /// </summary>
    public bool EnableCdn { get; set; }

    /// <summary>
    /// Virtual Network name (alias for backward compatibility)
    /// </summary>
    [StringLength(80, ErrorMessage = "VNet name cannot exceed 80 characters")]
    public string? VNetName { get; set; }

    /// <summary>
    /// SSL certificate name for Application Gateway
    /// </summary>
    [StringLength(80, ErrorMessage = "Certificate name cannot exceed 80 characters")]
    public string? SslCertificateName { get; set; } = "app-ssl-cert";

    /// <summary>
    /// VPN Gateway subnet address space
    /// </summary>
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}\/[0-9]{1,2}$", ErrorMessage = "Invalid CIDR format")]
    public string VpnGatewaySubnet { get; set; } = "10.0.6.0/24";

    /// <summary>
    /// Enable VPN Gateway for secure connectivity
    /// </summary>
    public bool EnableVpnGateway { get; set; } = true;

    /// <summary>
    /// VPN client address pool for Point-to-Site connections
    /// </summary>
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}\/[0-9]{1,2}$", ErrorMessage = "Invalid CIDR format")]
    public string VpnClientAddressPool { get; set; } = "172.16.0.0/24";

    /// <summary>
    /// VPN authentication type (Certificate or AAD)
    /// </summary>
    [RegularExpression(@"^(Certificate|AAD)$", ErrorMessage = "VPN authentication must be Certificate or AAD")]
    public string VpnAuthenticationType { get; set; } = "Certificate";

    /// <summary>
    /// VPN tunnel type (OpenVPN or IkeV2)
    /// </summary>
    [RegularExpression(@"^(OpenVPN|IkeV2)$", ErrorMessage = "VPN tunnel type must be OpenVPN or IkeV2")]
    public string VpnTunnelType { get; set; } = "OpenVPN";

    /// <summary>
    /// Determines if Container App Environment is internal-only (VNet-only) or allows public access.
    /// When false (default): Environment supports both VNet integration AND public accessibility with IP-based access control.
    /// When true: Environment is VNet-only and cannot be accessed from the internet, even with external ingress configured.
    /// IMPORTANT: This property is IMMUTABLE after environment creation. Changing it requires recreating the environment.
    /// Recommended: false for team access with IP restrictions, true for fully private internal environments.
    /// </summary>
    public bool IsInternalEnvironment { get; set; }
}


