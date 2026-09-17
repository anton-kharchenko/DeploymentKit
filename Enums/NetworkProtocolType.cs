using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Represents network protocols used in the infrastructure.
/// </summary>
public enum NetworkProtocolType
{
    /// <summary>
    /// HTTP protocol.
    /// </summary>
    [Description("Http")]
    Http,

    /// <summary>
    /// HTTPS protocol.
    /// </summary>
    [Description("Https")]
    Https,

    /// <summary>
    /// TCP protocol.
    /// </summary>
    [Description("Tcp")]
    Tcp,

    /// <summary>
    /// UDP protocol.
    /// </summary>
    [Description("Udp")]
    Udp
}

