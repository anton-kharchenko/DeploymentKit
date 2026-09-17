using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Domain ownership validation methods
/// </summary>
public enum DomainValidationType
{
    /// <summary>
    /// HTTP validation - Place file at .well-known/acme-challenge/
    /// </summary>
    [Description("A")]
    HTTP,

    /// <summary>
    /// DNS validation - Create TXT record for verification
    /// </summary>
    [Description("A")]
    DNS,

    /// <summary>
    /// CNAME validation - Create CNAME record pointing to verification endpoint
    /// </summary>
    [Description("A")]
    CNAME
}
