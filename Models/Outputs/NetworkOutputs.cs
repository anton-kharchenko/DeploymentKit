namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Represents the outputs from network infrastructure deployment
/// </summary>
public class NetworkOutputs
{
    /// <summary>
    /// Virtual Network ID
    /// </summary>
    public Output<string> VirtualNetworkId { get; set; } = null!;

    /// <summary>
    /// Virtual Network name
    /// </summary>
    public Output<string> VirtualNetworkName { get; set; } = null!;

    /// <summary>
    /// Container Apps subnet ID
    /// </summary>
    public Output<string> ContainerAppsSubnetId { get; set; } = null!;

    /// <summary>
    /// Database subnet ID
    /// </summary>
    public Output<string> DatabaseSubnetId { get; set; } = null!;

    /// <summary>
    /// Private endpoints subnet ID
    /// </summary>
    public Output<string> PrivateEndpointsSubnetId { get; set; } = null!;

    /// <summary>
    /// Application Gateway subnet ID
    /// </summary>
    public Output<string> ApplicationGatewaySubnetId { get; set; } = null!;

    /// <summary>
    /// Network Security Group ID for Container Apps
    /// </summary>
    public Output<string> ContainerAppsNsgId { get; set; } = null!;

    /// <summary>
    /// Network Security Group ID for Database
    /// </summary>
    public Output<string> DatabaseNsgId { get; set; } = null!;

    /// <summary>
    /// Private DNS Zone ID for Container Apps
    /// </summary>
    public Output<string> ContainerAppsPrivateDnsZoneId { get; set; } = null!;

    /// <summary>
    /// Private DNS Zone ID for Database
    /// </summary>
    public Output<string> DatabasePrivateDnsZoneId { get; set; } = null!;
}
