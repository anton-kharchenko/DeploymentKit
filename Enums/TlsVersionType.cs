using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available TLS version types for Azure services
/// </summary>
public enum TlsVersionType
{
    /// <summary>
    /// TLS version 1.0 (deprecated, not recommended)
    /// </summary>
    [Description("TLS1_0")]
    Tls10,

    /// <summary>
    /// TLS version 1.1 (deprecated, not recommended)
    /// </summary>
    [Description("TLS1_1")]
    Tls11,

    /// <summary>
    /// TLS version 1.2 (recommended minimum)
    /// </summary>
    [Description("TLS1_2")]
    Tls12,

    /// <summary>
    /// TLS version 1.3 (latest and most secure)
    /// </summary>
    [Description("TLS1_3")]
    Tls13
}
