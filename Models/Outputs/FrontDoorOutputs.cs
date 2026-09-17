namespace DeploymentKit.Models.Outputs;

public class FrontDoorOutputs
{
    public Output<string> ProfileName { get; set; } = null!;
    public Output<string> ProfileResourceId { get; set; } = null!;

    /// <summary>
    /// Value expected to match Front Door's `x-azure-fdid` header when available.
    /// Falls back to the Profile ARM resource ID if the GUID is not exposed by the provider.
    /// </summary>
    public Output<string> FrontDoorId { get; set; } = null!;

    public Output<string>? EndpointName { get; set; }
    public Output<string>? EndpointHostName { get; set; }
    public Output<string>? WafPolicyResourceId { get; set; }
    public Output<string>? CustomDomainHostName { get; set; }
    public Output<string>? CustomDomainResourceId { get; set; }
    public Output<string>? WebsiteCustomDomainHostName { get; set; }
    public Output<string>? MiniAppCustomDomainHostName { get; set; }
    public Output<string>? ApiCustomDomainHostName { get; set; }
}


