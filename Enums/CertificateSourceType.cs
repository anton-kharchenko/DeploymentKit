using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// SSL/TLS certificate source types
/// </summary>
public enum CertificateSourceType
{
    /// <summary>
    /// Azure-managed Let's Encrypt certificate (free, auto-renewal)
    /// </summary>
    [Description("Managed")]
    Managed,

    /// <summary>
    /// Existing certificate stored in Azure Key Vault
    /// </summary>
    [Description("KeyVault")]
    KeyVault,

    /// <summary>
    /// Upload custom certificate from local file (.pfx)
    /// </summary>
    [Description("Upload")]
    Upload
}
