using DeploymentKit.Enums;

namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Represents the outputs from SSL certificate provisioning
/// </summary>
public class CertificateOutputs
{
    /// <summary>
    /// Certificate ID in Azure
    /// </summary>
    public Output<string> CertificateId { get; set; } = null!;

    /// <summary>
    /// Certificate name
    /// </summary>
    public Output<string> CertificateName { get; set; } = null!;

    /// <summary>
    /// Certificate thumbprint
    /// </summary>
    public Output<string>? Thumbprint { get; set; }

    /// <summary>
    /// Certificate source type
    /// </summary>
    public CertificateSourceType Source { get; set; }

    /// <summary>
    /// Key Vault ID where certificate is stored (for KeyVault and Upload sources)
    /// </summary>
    public Output<string>? KeyVaultId { get; set; }

    /// <summary>
    /// Certificate secret ID in Key Vault
    /// </summary>
    public Output<string>? KeyVaultSecretId { get; set; }

    /// <summary>
    /// Domain name the certificate is for
    /// </summary>
    public Output<string> DomainName { get; set; } = null!;

    /// <summary>
    /// Certificate expiration date
    /// </summary>
    public Output<string>? ExpirationDate { get; set; }

    /// <summary>
    /// Whether the certificate is ready for use
    /// </summary>
    public bool IsReady { get; set; }
}

