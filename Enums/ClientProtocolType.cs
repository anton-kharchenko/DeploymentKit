using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the client protocol types for Azure Managed Redis
/// </summary>
public enum ClientProtocolType
{
    /// <summary>
    /// Encrypted - TLS encrypted connection (default, recommended)
    /// </summary>
    [Description("Encrypted")]
    Encrypted,

    /// <summary>
    /// Plaintext - Non-encrypted connection (not recommended for production)
    /// </summary>
    [Description("Plaintext")]
    Plaintext
}

