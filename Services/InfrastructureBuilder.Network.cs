using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Helpers;
using DeploymentKit.Interfaces;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Partial class for InfrastructureBuilder containing network-related resource configuration methods.
/// </summary>
public partial class InfrastructureBuilder
{
    private bool _addNetworking;
    private NetworkSettings? _networkSettings;

    private bool _addApplicationGateway;
    private ApplicationGatewaySettings? _applicationGatewaySettings;

    private bool _addDomainOptimization;
    private bool _addVpn;

    private bool _configureCustomDomain;
    private CustomDomainSettings? _customDomainSettings;

    /// <summary>
    /// Adds networking infrastructure with default settings.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddNetworking()
    {
        _addNetworking = true;
        _networkSettings = InfrastructureDefaultSettingsFactory.GetDefaultNetworkSettings();
        _logger.LogInformation(BuilderConstants.LoggingMessages.NetworkingDefault);
        return this;
    }

    /// <summary>
    /// Adds networking infrastructure with custom settings.
    /// </summary>
    /// <param name="networkSettings">Custom network settings.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddNetworking(NetworkSettings networkSettings)
    {
        _addNetworking = true;
        _networkSettings = networkSettings ?? throw new ArgumentNullException(nameof(networkSettings));
        _logger.LogInformation(BuilderConstants.LoggingMessages.NetworkingCustom);
        return this;
    }

    /// <summary>
    /// Adds Application Gateway with default settings.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddApplicationGateway()
    {
        _addApplicationGateway = true;
        _applicationGatewaySettings = InfrastructureDefaultSettingsFactory.GetDefaultApplicationGatewaySettings();
        _logger.LogInformation(BuilderConstants.LoggingMessages.ApplicationGatewayDefault);
        return this;
    }

    /// <summary>
    /// Adds Application Gateway with custom settings.
    /// </summary>
    /// <param name="applicationGatewaySettings">Custom Application Gateway settings.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddApplicationGateway(ApplicationGatewaySettings applicationGatewaySettings)
    {
        _addApplicationGateway = true;
        _applicationGatewaySettings = applicationGatewaySettings ?? throw new ArgumentNullException(nameof(applicationGatewaySettings));
        _logger.LogInformation(BuilderConstants.LoggingMessages.ApplicationGatewayCustom);
        return this;
    }

    /// <summary>
    /// Adds domain optimization (CDN, DNS, Traffic Manager).
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddDomainOptimization()
    {
        _addDomainOptimization = true;
        _logger.LogInformation(BuilderConstants.LoggingMessages.DomainOptimizationAdded);
        return this;
    }

    /// <summary>
    /// Adds VPN Gateway.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddVpn()
    {
        _addVpn = true;
        _logger.LogInformation(BuilderConstants.LoggingMessages.VpnAdded);
        return this;
    }

    /// <summary>
    /// Sets a custom domain name.
    /// </summary>
    /// <param name="domainName">The custom domain name.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder SetCustomDomain(string domainName)
    {
        if (string.IsNullOrWhiteSpace(domainName))
        {
            throw new ArgumentException(ValidationConstants.DomainNameRequired, nameof(domainName));
        }

        _configureCustomDomain = true;
        _customDomainSettings = new CustomDomainSettings
        {
            Name = domainName,
            Enabled = true,
            CertificateSource = CertificateSourceType.Managed,
            CreateDnsRecords = true,
            CreateARecord = true,
            CreateCaaRecords = true,
            BindToApplicationGateway = true
        };

        _logger.LogInformation(BuilderConstants.LoggingMessages.CustomDomainConfigured, domainName);
        return this;
    }

    /// <summary>
    /// Configures custom domain with advanced settings.
    /// </summary>
    /// <param name="customDomainSettings">Custom domain settings.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder ConfigureCustomDomain(CustomDomainSettings customDomainSettings)
    {
        _configureCustomDomain = true;
        _customDomainSettings = customDomainSettings ?? throw new ArgumentNullException(nameof(customDomainSettings));
        _logger.LogInformation(BuilderConstants.LoggingMessages.CustomDomainAdvanced,
            customDomainSettings.Name, customDomainSettings.CertificateSource);
        return this;
    }

    /// <summary>
    /// Configures Key Vault certificate for custom domain.
    /// </summary>
    /// <param name="keyVaultCertificateName">The name of the certificate in Key Vault.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder UseKeyVaultCertificate(string keyVaultCertificateName)
    {
        if (string.IsNullOrWhiteSpace(keyVaultCertificateName))
        {
            throw new ArgumentException(ValidationConstants.CertificateNameRequired, nameof(keyVaultCertificateName));
        }

        if (_customDomainSettings == null)
        {
            throw new InvalidOperationException(BuilderConstants.LoggingMessages.CustomDomainRequiredBeforeCert);
        }

        _customDomainSettings.CertificateSource = CertificateSourceType.KeyVault;
        _customDomainSettings.KeyVaultCertificateName = keyVaultCertificateName;
        _logger.LogInformation(BuilderConstants.LoggingMessages.KeyVaultCertConfigured, keyVaultCertificateName);
        return this;
    }

    /// <summary>
    /// Configures uploaded certificate for custom domain.
    /// </summary>
    /// <param name="certificateFilePath">Path to the certificate file.</param>
    /// <param name="certificatePassword">Password for the certificate.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder UploadCertificate(string certificateFilePath, string? certificatePassword = null)
    {
        if (string.IsNullOrWhiteSpace(certificateFilePath))
        {
            throw new ArgumentException(ValidationConstants.CertificateFilePathRequired, nameof(certificateFilePath));
        }

        if (!File.Exists(certificateFilePath))
        {
            throw new FileNotFoundException(string.Format(System.Globalization.CultureInfo.InvariantCulture, ValidationConstants.CertificateFileNotFound, certificateFilePath));
        }

        if (_customDomainSettings == null)
        {
            throw new InvalidOperationException(BuilderConstants.LoggingMessages.CustomDomainRequiredBeforeCert);
        }

        _customDomainSettings.CertificateSource = CertificateSourceType.Upload;
        _customDomainSettings.CertificateFilePath = certificateFilePath;
        _customDomainSettings.CertificatePassword = certificatePassword;
        _logger.LogInformation(BuilderConstants.LoggingMessages.CertUploadConfigured, certificateFilePath);
        return this;
    }

    /// <summary>
    /// Configures Azure Managed Certificate for custom domain.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder UseManagedCertificate()
    {
        if (_customDomainSettings == null)
        {
            throw new InvalidOperationException(BuilderConstants.LoggingMessages.CustomDomainRequiredBeforeCert);
        }

        _customDomainSettings.CertificateSource = CertificateSourceType.Managed;
        _logger.LogInformation(BuilderConstants.LoggingMessages.ManagedCertConfigured);
        return this;
    }
}


