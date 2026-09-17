using System.Collections.Immutable;

namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Represents the outputs from DNS record creation
/// </summary>
public class DnsRecordOutputs
{
    /// <summary>
    /// DNS Zone ID
    /// </summary>
    public Output<string> DnsZoneId { get; set; } = null!;

    /// <summary>
    /// DNS Zone name
    /// </summary>
    public Output<string> DnsZoneName { get; set; } = null!;

    /// <summary>
    /// A record ID (domain -> IP)
    /// </summary>
    public Output<string>? ARecordId { get; set; }

    /// <summary>
    /// CNAME record ID (www subdomain)
    /// </summary>
    public Output<string>? CnameRecordId { get; set; }

    /// <summary>
    /// CAA record ID (certificate authority authorization)
    /// </summary>
    public Output<string>? CaaRecordId { get; set; }

    /// <summary>
    /// Domain name configured
    /// </summary>
    public Output<string> DomainName { get; set; } = null!;

    /// <summary>
    /// Name servers for the DNS zone
    /// </summary>
    public Output<ImmutableArray<string>> NameServers { get; set; } = null!;
}

