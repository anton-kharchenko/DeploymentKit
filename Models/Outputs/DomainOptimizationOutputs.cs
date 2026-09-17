using System.Collections.Immutable;

namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Represents the outputs from domain optimization infrastructure deployment
/// </summary>
public class DomainOptimizationOutputs
{
    /// <summary>
    /// CDN Profile ID
    /// </summary>
    public Output<string> CdnProfileId { get; set; } = null!;

    /// <summary>
    /// CDN Endpoint ID
    /// </summary>
    public Output<string> CdnEndpointId { get; set; } = null!;

    /// <summary>
    /// CDN Endpoint hostname
    /// </summary>
    public Output<string> CdnEndpointHostname { get; set; } = null!;

    /// <summary>
    /// Traffic Manager Profile ID
    /// </summary>
    public Output<string> TrafficManagerProfileId { get; set; } = null!;

    /// <summary>
    /// Traffic Manager DNS name
    /// </summary>
    public Output<string> TrafficManagerDnsName { get; set; } = null!;

    /// <summary>
    /// DNS Zone ID
    /// </summary>
    public Output<string> DnsZoneId { get; set; } = null!;

    /// <summary>
    /// DNS Zone name servers - Configure these at your domain registrar to point your domain to Azure
    /// </summary>
    public Output<ImmutableArray<string>> NameServers { get; set; } = Output.Create(ImmutableArray<string>.Empty);

    /// <summary>
    /// Optimized domain URL for external access
    /// </summary>
    public Output<string> OptimizedDomainUrl { get; set; } = null!;
}
