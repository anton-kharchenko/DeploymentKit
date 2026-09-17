using DeploymentKit.Enums;
using DeploymentKit.Extensions;

namespace DeploymentKit.Constants;

/// <summary>
/// Constants used throughout the infrastructure deployment
/// </summary>
public static class InfrastructureConstants
{
    /// <summary>
    /// Default configuration values
    /// </summary>
    public static class Defaults
    {
        public const string Environment = "dev";
        public const string Location = "westeurope";
        public const string NamingPrefix = "app";
        public const string DatabaseAdminUser = "postgres";
        public const string DatabaseVersion = "16";
        public const int DatabaseStorageSizeGb = 32;
        public const string DatabaseSkuName = "Standard_B1ms";
        public const string DatabaseAvailabilityZone = "1";
        public const string ApiImageTag = "api:latest";
        public const string JobsImageTag = "jobs:latest";
        public const int RedisSkuCapacity = 0;

        /// <summary>
        /// Placeholder database password for local development only.
        /// Callers must replace it with a real secret before deploying to any shared or production environment.
        /// </summary>
        public const string LocalDevelopmentPassword = "local-dev-only-default-password-change-me!";
    }

    /// <summary>
    /// Database configuration constants
    /// </summary>
    public static class Database
    {
        // Default values
        public const string DefaultAdminUser = "postgres";
        public const string DefaultPostgreSqlVersion = "16";
        public const string DefaultSkuName = "Standard_B1ms";

        // Storage and availability - Azure PostgreSQL Flexible Server valid sizes
        public const int MinStorageSizeGb = 32;
        public const int MaxStorageSizeGb = 32767;
        public const string DefaultAvailabilityZone = "1";

        // Valid storage sizes for Azure PostgreSQL Flexible Server
        public static readonly int[] ValidStorageSizesGb =
        [
            32, 64, 128, 256, 512, 1024, 2048, 4095, 4096, 8192, 16384, 32767
        ];
    }

    /// <summary>
    /// Network configuration constants
    /// </summary>
    public static class Network
    {
        public const string DefaultVNetAddressSpace = "10.0.0.0/16";
        // For /23 subnets, the third octet must be even (0, 2, 4, 6, 8, etc.) for valid CIDR
        public const string DefaultContainerAppsSubnet = "10.0.1.0/24";  // Updated to match usage in InfrastructureBuilder
        public const string DefaultDatabaseSubnet = "10.0.2.0/24";        // 10.0.2.0 - 10.0.2.255
        public const string DefaultPrivateEndpointsSubnet = "10.0.3.0/24"; // 10.0.3.0 - 10.0.3.255
        public const string DefaultApplicationGatewaySubnet = "10.0.4.0/24"; // 10.0.4.0 - 10.0.4.255
        public const string DefaultCacheSubnet = "10.0.5.0/24";           // 10.0.5.0 - 10.0.5.255
    }

    /// <summary>
    /// Naming patterns for Azure resources
    /// </summary>
    public static class NamingPatterns
    {
        public const string ResourceGroup = "{0}-rg-{1}";
        public const string ContainerRegistry = "{0}acr{1}";
        public const string PostgreSqlServer = "{0}-pg-{1}";
        public const string RedisCache = "{0}-redis-{1}";
        public const string LogAnalyticsWorkspace = "{0}-log-{1}";
        public const string ApplicationInsights = "{0}-ai-{1}";
        public const string StorageAccount = "{0}st{1}";
        public const string ContainerAppsEnvironment = "{0}-cae-{1}";
        public const string ContainerApp = "{0}-{1}";
        public const string PostgreSqlDatabase = "{0}-db-{1}";
        public const string KeyVault = "{0}kv{1}";
        public const string VirtualNetwork = "{0}-vnet-{1}";
        public const string NetworkSecurityGroup = "{0}-nsg-{1}";
        public const string ApplicationGateway = "{0}-agw-{1}";
        public const string PublicIp = "{0}-pip-{1}";
        public const string VpnGateway = "{0}-vpngw-{1}";
        public const string EventHubsNamespace = "{0}-evhns-{1}";
        public const string EventHub = "{0}-evh-{1}";

        public const int MaxStorageAccountNameLength = 24;
        public const int MaxContainerRegistryNameLength = 50;
        public const int MaxKeyVaultNameLength = 24;
    }

    /// <summary>
    /// Standard resource tags
    /// </summary>
    public static class Tags
    {
        public const string Project = "Application";
        public const string ManagedBy = "Pulumi";
        public const string Owner = "Application-DevOps";
    }

    /// <summary>
    /// Redis cache configuration constants
    /// </summary>
    public static class Redis
    {
        // SKU Names
        public const string BasicSku = "Basic";

        // SKU Families
        public const string BasicStandardFamily = "C";

        // Capacity ranges
        public const int MinCapacity = 0;
        public const int MaxCapacity = 6;

        public const bool DefaultEnableNonSslPort = true;
    }

    /// <summary>
    /// Cache configuration constants
    /// </summary>
    public static class Cache
    {
        public const int DefaultSkuCapacity = 0;
        public const string DefaultMinimumTlsVersion = "1.2";
    }

    /// <summary>
    /// Storage configuration constants
    /// </summary>
    public static class Storage
    {
        public const string DefaultContentType = "application/octet-stream";
    }

    /// <summary>
    /// Event Hubs configuration constants
    /// </summary>
    public static class EventHubs
    {
        public const int DefaultCapacity = 1;
        public const int DefaultMessageRetentionDays = 1;
        public const int DefaultPartitionCount = 2;
    }

    /// <summary>
    /// Monitoring configuration constants
    /// </summary>
    public static class Monitoring
    {
        // Default values
        public const int DefaultLogRetentionDays = 30;
        public const string DefaultApplicationInsightsType = "web";
    }

    /// <summary>
    /// Environment-specific naming conventions
    /// </summary>
    public static class EnvironmentNaming
    {
        private const string DevSuffix = ".DEV";
        private const string ProdSuffix = ".PROD";
        private static string GetEnvironmentSuffix(string environment)
        {
            return environment.ToLowerInvariant() switch
            {
                "dev" or "development" => DevSuffix,
                "prod" or "production" => ProdSuffix,
                _ => string.Empty
            };
        }

        public static string ApplyEnvironmentNaming(string baseName, string environment)
        {
            var suffix = GetEnvironmentSuffix(environment);
            return string.IsNullOrEmpty(suffix) ? baseName : $"{baseName}{suffix}";
        }
    }

    /// <summary>
    /// Validation constants
    /// </summary>
    public static class Validation
    {
        public static readonly string[] ValidEnvironments = [
            EnvironmentType.Development.ToStringValue().ToLowerInvariant(),
            EnvironmentType.Production.ToStringValue().ToLowerInvariant()
        ];

        public static readonly string[] ValidAzureLocations =
        [
            "eastus", "eastus2", "westus", "westus2", "westus3", "centralus", "northcentralus", "southcentralus",
            "westeurope", "northeurope", "uksouth", "ukwest", "francecentral", "germanywestcentral", "norwayeast",
            "switzerlandnorth", "swedencentral", "southeastasia", "eastasia", "australiaeast", "australiasoutheast",
            "japaneast", "japanwest", "koreacentral", "koreasouth", "southafricanorth", "uaenorth", "brazilsouth",
            "canadacentral", "canadaeast"
        ];
    }

    /// <summary>
    /// Log messages
    /// </summary>
    public static class LogMessages
    {
        public const string StartingDeployment = "Starting DeploymentKit infrastructure deployment...";
        public const string ServiceRegistrationFailed = "Failed to validate infrastructure service registration";
        public const string ConfigurationValidationFailed = "Infrastructure configuration validation failed";
        public const string NamingValidationWarning = "Naming convention validation failed, but continuing with deployment";
        public const string LocationValidationWarning = "Azure location validation failed, but continuing with deployment";
        public const string DeploymentCompleted = "Successfully completed DeploymentKit infrastructure deployment";
    }

    /// <summary>
    /// Key Vault constants
    /// </summary>
    public static class KeyVault
    {
        public const string StandardSku = "Standard";
    }
}

