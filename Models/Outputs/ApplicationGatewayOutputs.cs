namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Represents the outputs from Application Gateway infrastructure deployment
/// </summary>
public class ApplicationGatewayOutputs
{
    /// <summary>
    /// Application Gateway ID
    /// </summary>
    public Output<string> ApplicationGatewayId { get; set; } = null!;

    /// <summary>
    /// Application Gateway name
    /// </summary>
    public Output<string> ApplicationGatewayName { get; set; } = null!;

    /// <summary>
    /// Public IP address of the Application Gateway
    /// </summary>
    public Input<string> PublicIpAddress { get; set; } = null!;

    /// <summary>
    /// Frontend URL for external access
    /// </summary>
    public Output<string> FrontendUrl { get; set; } = null!;
}
