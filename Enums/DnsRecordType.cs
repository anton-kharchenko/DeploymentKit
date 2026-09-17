using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// DNS record types supported by the SDK
/// </summary>
public enum DnsRecordType
{
    /// <summary>
    /// A record - Maps domain to IPv4 address
    /// </summary>
    [Description("A")]
    A,

    /// <summary>
    /// CNAME record - Alias for another domain
    /// </summary>
    [Description("CNAME")]
    CNAME,

    /// <summary>
    /// TXT record - Text records for verification
    /// </summary>
    [Description("TXT")]
    TXT,

    /// <summary>
    /// CAA record - Certificate Authority Authorization
    /// </summary>
    [Description("CAA")]
    CAA
}
