namespace DeploymentKit.Constants;

/// <summary>
/// Constants used by the InfrastructureBuilder.
/// </summary>
public static class BuilderConstants
{
    public const string DeploymentNameParam = "deploymentName";
    public const string EnvironmentParam = "environment";
    public const string LocationParam = "location";
    public const string SubscriptionIdParam = "subscriptionId";
    public const string ResourceGroupNameParam = "resourceGroupName";
    public const string NamingPrefixParam = "namingPrefix";

    public static class ErrorMessages
    {
        public const string DeploymentNameRequired = "Deployment name is required";
        public const string EnvironmentRequired = "Environment is required";
        public const string LocationRequired = "Location is required";
        public const string SubscriptionIdRequired = "Subscription ID is required";
        public const string ResourceGroupNameRequired = "Resource group name is required";
        public const string NamingPrefixRequired = "Naming prefix is required";

        public const string DeploymentNameInvalidFormat = "Deployment name must be 3-50 characters long and contain only alphanumeric characters and hyphens";
        public const string EnvironmentInvalid = "Environment must be one of: {0}, {1}";

        public const string DeploymentNameNotAvailable = "Deployment name '{0}' is not available: {1}";
        public const string BuildFailedValidation = "Build failed due to validation errors: {0}";
        public const string CannotBuildValidationErrors = "Cannot build infrastructure due to validation errors: {0}";

        public const string AtLeastOneResource = "At least one infrastructure resource must be configured";
        public const string KeyVaultEnvFileNotFound = "Key Vault .env file not found: {0}";

        public const string EnvFilePathRequired = "Environment file path cannot be null or empty";
        public const string EnvFileParseFailed = "Failed to parse .env file '{0}': {1}";
        public const string InvalidKeyVaultSecretNames = "Invalid Key Vault secret names in .env file: {0}";
    }

    public static class Logs
    {
        public const string DeploymentNameValidated = "Deployment name '{Name}' validated and set successfully";
        public const string DeploymentNameValidationFailed = "Failed to validate deployment name '{Name}': {Error}";
        public const string EnvironmentSet = "Environment set to '{Environment}'";
        public const string LocationSet = "Location set to '{Location}'";
        public const string LocationSetRegion = "Location set to '{Location}' ({Region})";
        public const string SubscriptionIdSet = "Subscription ID set";
        public const string ResourceGroupNameSet = "Resource group name set to '{ResourceGroupName}'";
        public const string NamingPrefixSet = "Naming prefix set to '{NamingPrefix}'";
        public const string UsingExistingSettings = "Using existing infrastructure settings";
        public const string BuildingWithExistingSettings = "Building infrastructure with existing settings";
        public const string InfrastructureBuilt = "Infrastructure settings built successfully for deployment '{DeploymentName}'";

        public const string ProcessingKeyVaultEnvSync = "Processing deferred Key Vault .env file (Sync): {EnvFilePath}";
        public const string ProcessingKeyVaultEnvAsync = "Processing deferred Key Vault .env file (Async): {EnvFilePath}";
        public const string NoSecretsFoundInEnv = "No valid secrets found in .env file: {EnvFilePath}";
        public const string KeyVaultSecretsAdded = "Added Key Vault with {SecretCount} secrets from .env file";
        public const string ConfiguringKeyVault = "Configuring Key Vault with deferred .env file parsing: {EnvFilePath}";
        public const string KeyVaultAddedCustom = "Key Vault added with custom settings";
        public const string KeyVaultAddedDefault = "Key Vault added with default settings";

        public const string ValidationModeSet = "Validation mode set to '{ValidationMode}'";
        public const string ValidationModeBasic = "Basic validation mode: Settings and naming checks only. Recommended for initial deployments.";
        public const string ValidationModeSkip = "Validation mode set to Skip - all validation will be bypassed. Use with caution!";
        public const string AzureAuthValidationSkipped = "Azure authentication validation will be skipped. Using Azure CLI or default authentication chain.";
        public const string AzureAuthValidationEnabled = "Azure authentication validation will be performed.";
    }

    public static class ValidationMessages
    {
        public const string DeploymentNameRequired = ErrorMessages.DeploymentNameRequired;
        public const string DeploymentNameFormat = ErrorMessages.DeploymentNameInvalidFormat;
        public const string DeploymentNameUnavailable = ErrorMessages.DeploymentNameNotAvailable;
        public const string EnvironmentRequired = ErrorMessages.EnvironmentRequired;
        public const string InvalidEnvironment = ErrorMessages.EnvironmentInvalid;
        public const string SubscriptionIdMissing = ErrorMessages.SubscriptionIdRequired;
        public const string NoResourcesConfigured = ErrorMessages.AtLeastOneResource;
        public const string KeyVaultEnvFileNotFound = ErrorMessages.KeyVaultEnvFileNotFound;
    }

    public static class LogMessages
    {
        public const string DeploymentNameValidated = Logs.DeploymentNameValidated;
        public const string DeploymentNameValidationFailed = Logs.DeploymentNameValidationFailed;
        public const string EnvironmentSet = Logs.EnvironmentSet;
    }

    public static class LoggingMessages
    {
        public const string DatabaseDefault = "Database added with default settings";
        public const string DatabaseCustom = "Database added with custom settings";
        public const string CosmosDbDefault = "Cosmos DB added with default settings";
        public const string CosmosDbCustom = "Cosmos DB added with custom settings";
        public const string EventHubsDefault = "Event Hubs added with default settings";
        public const string EventHubsCustom = "Event Hubs added with custom settings";
        public const string InsightsDefault = "Insights added with default settings";
        public const string InsightsCustom = "Insights added with custom settings";
        public const string ContainerRegistryAdded = "Container registry added";
        public const string ContainerAppsDefault = "Container Apps added with default settings";
        public const string ContainerAppsCustom = "Container Apps added with custom settings";
        public const string GreenBlueDefault = "Green-blue deployment enabled with default settings";
        public const string GreenBlueCustom = "Green-blue deployment enabled with custom settings";
        public const string RedisDefault = "Redis cache added with default settings";
        public const string RedisCustom = "Redis cache added with custom settings";
        public const string StorageDefault = "Storage added with default settings";
        public const string StorageCustom = "Storage added with custom settings";
        public const string BlobStorageDefault = "Blob storage added with default settings";
        public const string BlobStorageCustom = "Blob storage added with custom settings";
        public const string TableStorageDefault = "Table storage added with default settings";
        public const string TableStorageCustom = "Table storage added with custom settings";
        public const string NetworkingDefault = "Networking added with default settings";
        public const string NetworkingCustom = "Networking added with custom settings";
        public const string ApplicationGatewayDefault = "Application Gateway added with default settings";
        public const string ApplicationGatewayCustom = "Application Gateway added with custom settings";
        public const string DomainOptimizationAdded = "Domain optimization added";
        public const string VpnAdded = "VPN gateway added";
        public const string CustomDomainConfigured = "Custom domain configured: {DomainName}";
        public const string CustomDomainAdvanced = "Custom domain configured with advanced settings: {DomainName}, CertificateSource: {CertificateSource}";
        public const string CustomDomainRequiredBeforeCert = "Custom domain must be configured before certificate settings can be applied.";
        public const string KeyVaultCertConfigured = "Key Vault certificate configured: {CertificateName}";
        public const string CertUploadConfigured = "Certificate upload configured: {CertificateFilePath}";
        public const string ManagedCertConfigured = "Managed certificate configured";
    }
}

