using DeploymentKit.Enums;

namespace DeploymentKit.Settings
{
    /// <summary>
    /// Configuration settings for Azure Key Vault with environment-specific configurations
    /// </summary>
    public class KeyVaultSettings
    {
        /// <summary>
        /// Enable Key Vault creation and management
        /// </summary>
        public bool EnableKeyVault { get; set; } = true;

        /// <summary>
        /// Key Vault SKU name
        /// </summary>
        public KeyVaultSkuType SkuName { get; set; } = KeyVaultSkuType.Standard;

        /// <summary>
        /// Enable soft delete for Key Vault (recommended for production)
        /// </summary>
        public bool EnableSoftDelete { get; set; } = true;

        /// <summary>
        /// Soft delete retention period in days (7-90 days)
        /// </summary>
        public int SoftDeleteRetentionDays { get; set; } = 90;

        /// <summary>
        /// Enable purge protection (prevents permanent deletion)
        /// </summary>
        public bool EnablePurgeProtection { get; set; }

        /// <summary>
        /// Enable RBAC authorization instead of access policies
        /// </summary>
        public bool EnableRbacAuthorization { get; set; } = true;

        /// <summary>
        /// Enable public network access (false for private endpoint only)
        /// </summary>
        public bool EnablePublicNetworkAccess { get; set; } = true;

        /// <summary>
        /// Enable private endpoints for Key Vault
        /// </summary>
        public bool EnablePrivateEndpoints { get; set; }

        /// <summary>
        /// Environment-specific access policies
        /// </summary>
        public DevAccessPoliciesSettings DevAccessPolicies { get; set; } = new();

        /// <summary>
        /// Production access policies
        /// </summary>
        public ProdAccessPoliciesSettings ProdAccessPolicies { get; set; } = new();

        /// <summary>
        /// Environment access policies
        /// </summary>
        public EnvironmentAccessPoliciesSettings EnvironmentAccessPolicies { get; set; } = new();

        /// <summary>
        /// Development secrets configuration
        /// </summary>
        public DevSecretsSettings DevSecrets { get; set; } = new();

        /// <summary>
        /// Production secrets configuration
        /// </summary>
        public ProdSecretsSettings ProdSecrets { get; set; } = new();

        /// <summary>
        /// Environment secrets configuration
        /// </summary>
        public EnvironmentSecretsSettings EnvironmentSecrets { get; set; } = new();

        /// <summary>
        /// Network access rules for Key Vault
        /// </summary>
        public NetworkAccessRulesSettings NetworkAccess { get; set; } = new();

        /// <summary>
        /// Secrets to be stored in Key Vault
        /// </summary>
        public Dictionary<string, string> Secrets { get; set; } = new();

        /// <summary>
        /// Whether Key Vault is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Automatically apply all Key Vault secrets as environment variables to Container Apps.
        /// When enabled, all secrets stored in Key Vault will be configured as environment variables
        /// in the Container Apps, with values referenced from Key Vault secrets.
        /// </summary>
        public bool ApplyToContainerApps { get; set; }

        // String properties for backward compatibility and Pulumi integration
        internal string SkuNameString { get; set; } = string.Empty;
    }
}

