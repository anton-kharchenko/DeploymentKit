using DeploymentKit.Enums;
using DeploymentKit.Exceptions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using Pulumi.AzureNative.KeyVault.Inputs;
using System.Security.Cryptography.X509Certificates;
using PulumiKeyVault = Pulumi.AzureNative.KeyVault;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing SSL/TLS certificates (Managed, KeyVault, Upload)
/// </summary>
public class CertificateManagementService(
    ILogger<CertificateManagementService> logger,
    ICorrelationIdService correlationIdService) : ICertificateManagementService
{
    private readonly ILogger<CertificateManagementService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));

    public async Task<CertificateOutputs> ProvisionCertificateAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        Input<string> keyVaultId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(settings.CustomDomain);

        if (!settings.CustomDomain.Enabled)
        {
            _logger.LogInformation("Custom domain is disabled. Skipping certificate provisioning.");
            return CreateEmptyOutputs(settings.CustomDomain.Name);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
            _logger.LogInformation("Provisioning SSL certificate for domain: {Domain}, Source: {Source}, CorrelationId: {CorrelationId}",
                settings.CustomDomain.Name, settings.CustomDomain.CertificateSource, correlationId);

            return settings.CustomDomain.CertificateSource switch
            {
                CertificateSourceType.Managed => await ProvisionManagedCertificateAsync(settings),
                CertificateSourceType.KeyVault => await RetrieveCertificateFromKeyVaultAsync(settings, keyVaultId),
                CertificateSourceType.Upload => await UploadCustomCertificateAsync(settings, resourceGroup, keyVaultId, cancellationToken),
                _ => throw new InvalidOperationException($"Unsupported certificate source: {settings.CustomDomain.CertificateSource}")
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision certificate for domain: {Domain}", settings.CustomDomain?.Name);
            throw new ResourceCreationException(
                $"Failed to provision certificate for domain '{settings.CustomDomain?.Name}'",
                ex,
                "Certificate",
                settings.Environment,
                _correlationIdService.GetOrGenerateCorrelationId(),
                "CERTIFICATE_PROVISIONING_FAILED");
        }
    }

    private async Task<CertificateOutputs> ProvisionManagedCertificateAsync(InfrastructureSettings settings)
    {
        _logger.LogInformation("Provisioning Azure Managed Certificate (Let's Encrypt) for domain: {Domain}",
            settings.CustomDomain!.Name);

        var certName = $"{settings.NamingPrefix}-{settings.Environment}-managed-cert";

        // Managed Certificates: Azure only supports for Container Apps, not Application Gateway
        // For Application Gateway, users must upload certificates to Key Vault manually
        if (settings.CustomDomain.BindToApplicationGateway)
        {
            _logger.LogWarning("Managed Certificates (Let's Encrypt) for Application Gateway are not supported by Azure. " +
                              "Please obtain a certificate and use CertificateSource.Upload or CertificateSource.KeyVault. " +
                              "For free certificates, consider certbot (Let's Encrypt) or acme.sh.");
        }

        if (settings.CustomDomain.BindToContainerApps)
        {
            _logger.LogInformation("Managed Certificates for Container Apps require manual binding post-deployment. " +
                                  "Create certificate via Azure Portal: Container Apps → Certificates → Add certificate (Managed).");
        }

        return await Task.FromResult(new CertificateOutputs
        {
            CertificateId = Output.Create("managed-certificate-not-supported"),
            CertificateName = Output.Create(certName),
            Source = CertificateSourceType.Managed,
            DomainName = Output.Create(settings.CustomDomain.Name),
            IsReady = false // Not ready - requires manual certificate provisioning
        });
    }

    private async Task<CertificateOutputs> RetrieveCertificateFromKeyVaultAsync(
        InfrastructureSettings settings,
        Input<string> keyVaultId)
    {
        if (string.IsNullOrEmpty(settings.CustomDomain!.KeyVaultCertificateName))
        {
            throw new InvalidOperationException("KeyVaultCertificateName is required when CertificateSource is KeyVault");
        }

        _logger.LogInformation("Retrieving certificate reference from Key Vault: {CertName}",
            settings.CustomDomain.KeyVaultCertificateName);

        var certName = settings.CustomDomain.KeyVaultCertificateName;
        var vaultName = keyVaultId.Apply(ExtractVaultNameFromId);

        // Get reference to existing secret in Key Vault
        var secretUri = Output.Format($"https://{vaultName}.vault.azure.net/secrets/{certName}");
        var certificateId = keyVaultId.Apply(id => $"{id}/secrets/{certName}");

        _logger.LogInformation("Certificate reference retrieved from Key Vault: {CertName}", certName);

        return await Task.FromResult(new CertificateOutputs
        {
            CertificateId = certificateId,
            CertificateName = Output.Create(certName),
            Source = CertificateSourceType.KeyVault,
            KeyVaultId = keyVaultId,
            KeyVaultSecretId = secretUri,
            DomainName = Output.Create(settings.CustomDomain.Name),
            IsReady = true // Assumes certificate exists in Key Vault
        });
    }

    private async Task<CertificateOutputs> UploadCustomCertificateAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        Input<string> keyVaultId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(settings.CustomDomain!.CertificateFilePath))
        {
            throw new InvalidOperationException("CertificateFilePath is required when CertificateSource is Upload");
        }

        _logger.LogInformation("Uploading custom certificate from file: {FilePath}",
            settings.CustomDomain.CertificateFilePath);

        var certName = $"{settings.NamingPrefix}-{settings.Environment}-custom-cert";

        return await UploadCertificateToKeyVaultAsync(
            settings.CustomDomain.CertificateFilePath,
            settings.CustomDomain.CertificatePassword,
            certName,
            keyVaultId,
            resourceGroup,
            cancellationToken);
    }

    public async Task<CertificateOutputs> UploadCertificateToKeyVaultAsync(
        string certificatePath,
        string? certificatePassword,
        string certificateName,
        Input<string> keyVaultId,
        Input<string> resourceGroup,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading certificate to Key Vault: {CertName}", certificateName);

        try
        {
            // Read certificate file
            byte[] certBytes;
            try
            {
                certBytes = await File.ReadAllBytesAsync(certificatePath, cancellationToken).ConfigureAwait(false);
            }
            catch (FileNotFoundException ex)
            {
                 throw new FileNotFoundException($"Certificate file not found: {certificatePath}", ex);
            }

            var certBase64 = Convert.ToBase64String(certBytes);

            // Extract certificate details (thumbprint, expiration)
            using var cert = X509CertificateLoader.LoadPkcs12(certBytes, certificatePassword);
            var thumbprint = cert.Thumbprint;
            var expirationDate = cert.NotAfter.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            var subjectName = cert.GetNameInfo(X509NameType.DnsName, false);

            _logger.LogInformation("Certificate details - Thumbprint: {Thumbprint}, Expiration: {Expiration}, Subject: {Subject}",
                thumbprint, expirationDate, subjectName);

            // Extract vault name from ID for the secret resource
            var vaultName = keyVaultId.Apply(ExtractVaultNameFromId);

            // Upload certificate as a secret to Key Vault using Pulumi
            var secret = new PulumiKeyVault.Secret($"{certificateName}-secret", new PulumiKeyVault.SecretArgs
            {
                SecretName = certificateName,
                VaultName = vaultName,
                ResourceGroupName = resourceGroup,
                Properties = new SecretPropertiesArgs
                {
                    Value = certBase64,
                    ContentType = "application/x-pkcs12",
                    Attributes = new SecretAttributesArgs
                    {
                        Enabled = true
                    }
                }
            });

            _logger.LogInformation("Certificate uploaded successfully to Key Vault as secret: {CertName}", certificateName);

            return new CertificateOutputs
            {
                CertificateId = secret.Id,
                CertificateName = Output.Create(certificateName),
                Thumbprint = Output.Create(thumbprint),
                Source = CertificateSourceType.Upload,
                KeyVaultId = keyVaultId,
                KeyVaultSecretId = secret.Properties.Apply(p => p.SecretUri),
                DomainName = Output.Create(subjectName),
                ExpirationDate = Output.Create(expirationDate),
                IsReady = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload certificate to Key Vault: {CertName}", certificateName);
            throw new ResourceCreationException(
                $"Failed to upload certificate '{certificateName}' to Key Vault",
                ex,
                "Certificate",
                "KeyVault",
                _correlationIdService.GetOrGenerateCorrelationId(),
                "CERTIFICATE_UPLOAD_FAILED");
        }
    }

    private static string ExtractVaultNameFromId(string vaultId)
    {
        // Extract vault name from resource ID: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.KeyVault/vaults/{name}
        var parts = vaultId.Split('/');
        return parts.Length > 0 ? parts[^1] : vaultId;
    }

    private static CertificateOutputs CreateEmptyOutputs(string domainName)
    {
        return new CertificateOutputs
        {
            CertificateId = Output.Create(""),
            CertificateName = Output.Create(""),
            Source = CertificateSourceType.Managed,
            DomainName = Output.Create(domainName),
            IsReady = false
        };
    }
}

