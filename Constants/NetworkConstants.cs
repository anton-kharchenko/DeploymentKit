namespace DeploymentKit.Constants;

public static class NetworkConstants
{
    public static class ErrorMessages
    {
        public const string DomainNameRequired = "Domain name cannot be null or empty";
        public const string CustomDomainNotConfigured = "Custom domain must be configured before specifying certificate source. Call SetCustomDomain() first.";
        public const string CertificateNameRequired = "Certificate name cannot be null or empty";
        public const string CertificateFilePathRequired = "Certificate file path cannot be null or empty";
        public const string CertificateFileNotFound = "Certificate file not found: {0}";
    }

    public static class Logs
    {
        public const string DefaultNetworkAdded = "Networking infrastructure added with default settings";
        public const string CustomNetworkAdded = "Networking infrastructure added with custom settings";
        public const string DefaultAppGatewayAdded = "Application Gateway added with default settings";
        public const string CustomAppGatewayAdded = "Application Gateway added with custom settings";
        public const string DomainOptimizationAdded = "Domain optimization (CDN, DNS, Traffic Manager) added";
        public const string VpnAdded = "VPN Gateway added";
        public const string CustomDomainConfigured = "Custom domain configured: Domain={Domain}";
        public const string CustomDomainAdvancedConfigured = "Custom domain configured with advanced settings: Domain={Domain}, Source={Source}";
        public const string KeyVaultCertConfigured = "Key Vault certificate configured: CertName={CertName}";
        public const string CertUploadConfigured = "Certificate upload configured: FilePath={FilePath}";
        public const string ManagedCertConfigured = "Azure Managed Certificate (Let's Encrypt) configured";
    }
}

