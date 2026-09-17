namespace DeploymentKit.Constants;

/// <summary>
/// Constants for validation operations
/// </summary>
public static class ValidationConstants
{
    public const string DevelopmentEnvironment = "development";
    public const string ShortDevelopmentEnvironment = "dev";
    public const string ProductionEnvironment = "production";
    public const string ShortProductionEnvironment = "prod";
    public const string TestEnvironment = "test";
    public const string StagingEnvironment = "staging";

    public const string DomainNameRequired = NetworkConstants.ErrorMessages.DomainNameRequired;
    public const string CertificateNameRequired = NetworkConstants.ErrorMessages.CertificateNameRequired;
    public const string CertificateFilePathRequired = NetworkConstants.ErrorMessages.CertificateFilePathRequired;
    public const string CertificateFileNotFound = NetworkConstants.ErrorMessages.CertificateFileNotFound;

    public const string MigrationAssemblyRequired = MigrationConstants.ErrorMessages.MigrationAssemblyRequired;
    public const string DbContextTypeRequired = MigrationConstants.ErrorMessages.DbContextRequired;
    public const string SqlScriptPathRequired = MigrationConstants.ErrorMessages.SqlScriptPathRequired;

    public static class ContainerApps
    {
        public const string DatabasePasswordRequired = "Database password is required";
    }

    /// <summary>
    /// Validation error and warning messages
    /// </summary>
    public static class Messages
    {
        public const string StorageAccountNameRequired = "Storage account name is required";
        public const string StorageAccountTypeRequired = "Storage account type is required";
        public const string DeploymentNameFormatError = "Deployment name must be 3-50 characters long and contain only alphanumeric characters and hyphens";
        public const string DeploymentNameNullOrEmpty = "Deployment name cannot be null or empty";
        public const string DeploymentNameRequired = "Deployment name is required";
        public const string EnvironmentNullOrEmpty = "Environment cannot be null or empty";
        public const string EnvironmentRequired = "Environment is required";
        public const string EnvironmentInvalid = "Environment must be one of: {0}, {1}";
        public const string LocationNullOrEmpty = "Location cannot be null or empty";
        public const string LocationRequired = "Location is required";
        public const string SubscriptionIdNullOrEmpty = "Subscription ID cannot be null or empty";
        public const string SubscriptionIdRequired = "Subscription ID is required";
        public const string ResourceGroupNameNullOrEmpty = "Resource group name cannot be null or empty";
        public const string NamingPrefixNullOrEmpty = "Naming prefix cannot be null or empty";
        public const string InfrastructureResourceRequired = BuilderConstants.ErrorMessages.AtLeastOneResource;
        public const string KeyVaultEnvFileNotFound = BuilderConstants.ErrorMessages.KeyVaultEnvFileNotFound;
    }
}

