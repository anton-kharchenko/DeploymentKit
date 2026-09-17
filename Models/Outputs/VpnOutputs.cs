namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from VPN Gateway infrastructure deployment
/// </summary>
public class VpnOutputs
{
    /// <summary>
    /// VPN Gateway resource ID
    /// </summary>
    public Output<string> VpnGatewayId { get; set; } = null!;

    /// <summary>
    /// VPN Gateway name
    /// </summary>
    public Output<string> VpnGatewayName { get; set; } = null!;

    /// <summary>
    /// VPN Gateway public IP address
    /// </summary>
    public Output<string> VpnGatewayPublicIp { get; set; } = null!;

    /// <summary>
    /// VPN Gateway subnet ID
    /// </summary>
    public Output<string> VpnGatewaySubnetId { get; set; } = null!;

    /// <summary>
    /// Point-to-Site VPN configuration name
    /// </summary>
    public Output<string> P2SVpnConfigurationName { get; set; } = null!;

    /// <summary>
    /// VPN client address pool
    /// </summary>
    public string VpnClientAddressPool { get; set; } = null!;

    /// <summary>
    /// VPN authentication type
    /// </summary>
    public string VpnAuthenticationType { get; set; } = null!;

    /// <summary>
    /// VPN tunnel type
    /// </summary>
    public string VpnTunnelType { get; set; } = null!;

    /// <summary>
    /// Root certificate name for authentication
    /// </summary>
    public string? RootCertificateName { get; set; }

    /// <summary>
    /// Root certificate public key data
    /// </summary>
    public string? RootCertificateData { get; set; }
}
