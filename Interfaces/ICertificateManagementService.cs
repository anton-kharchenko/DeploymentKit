using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service for managing SSL/TLS certificates (Managed, KeyVault, Upload)
/// </summary>
public interface ICertificateManagementService
{
    /// <summary>
    /// Provisions SSL certificate based on settings (Managed, KeyVault, or Upload)
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="resourceGroup">Resource group name</param>
    /// <param name="keyVaultId">Key Vault ID for storing certificates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Certificate outputs</returns>
    Task<CertificateOutputs> ProvisionCertificateAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        Input<string> keyVaultId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a .pfx certificate file to Key Vault
    /// </summary>
    /// <param name="certificatePath">Path to .pfx file</param>
    /// <param name="certificatePassword">Certificate password</param>
    /// <param name="certificateName">Name for the certificate in Key Vault</param>
    /// <param name="keyVaultId">Key Vault ID</param>
    /// <param name="resourceGroup">Resource group name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Certificate outputs</returns>
    Task<CertificateOutputs> UploadCertificateToKeyVaultAsync(
        string certificatePath,
        string? certificatePassword,
        string certificateName,
        Input<string> keyVaultId,
        Input<string> resourceGroup,
        CancellationToken cancellationToken = default);
}

