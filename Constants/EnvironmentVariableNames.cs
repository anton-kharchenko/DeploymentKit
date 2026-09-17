namespace DeploymentKit.Constants;

/// <summary>
/// Environment variable names used by InfrastructureSettingsHelper
/// </summary>
public static class EnvironmentVariableNames
{
    /// <summary>
    /// Core infrastructure settings
    /// </summary>
    public static class Core
    {
        public const string Environment = "INFRA_ENVIRONMENT";
        public const string Location = "INFRA_LOCATION";
        public const string NamingPrefix = "INFRA_NAMING_PREFIX";
        public const string SubscriptionId = "INFRA_SUBSCRIPTION_ID";
        public const string ResourceGroupName = "INFRA_RESOURCE_GROUP_NAME";
    }

    /// <summary>
    /// Database settings
    /// </summary>
    public static class Database
    {
        public const string AdminUsername = "INFRA_DB_ADMIN_USERNAME";
        public const string AdminPassword = "INFRA_DB_ADMIN_PASSWORD";
        public const string Version = "INFRA_DB_VERSION";
        public const string StorageSizeGb = "INFRA_DB_STORAGE_SIZE_GB";
        public const string SkuName = "INFRA_DB_SKU_NAME";
        public const string AvailabilityZone = "INFRA_DB_AVAILABILITY_ZONE";
        public const string EnableHighAvailability = "INFRA_DB_ENABLE_HIGH_AVAILABILITY";
        public const string BackupRetentionDays = "INFRA_DB_BACKUP_RETENTION_DAYS";
        public const string EnableGeoRedundantBackup = "INFRA_DB_ENABLE_GEO_REDUNDANT_BACKUP";
    }

    /// <summary>
    /// Container settings
    /// </summary>
    public static class Container
    {
        public const string ApiImageTag = "INFRA_CONTAINER_API_IMAGE_TAG";
        public const string JobsImageTag = "INFRA_CONTAINER_JOBS_IMAGE_TAG";
        public const string CpuLimit = "INFRA_CONTAINER_CPU_LIMIT";
        public const string MemoryLimit = "INFRA_CONTAINER_MEMORY_LIMIT";
        public const string MinReplicas = "INFRA_CONTAINER_MIN_REPLICAS";
        public const string MaxReplicas = "INFRA_CONTAINER_MAX_REPLICAS";
        public const string EnableDapr = "INFRA_CONTAINER_ENABLE_DAPR";
        public const string RegistryNameOverride = "INFRA_CONTAINER_REGISTRY_NAME_OVERRIDE";
        public const string IngressIpRestrictions = "INGRESS_IP_RESTRICTIONS";
        public const string IngressExternal = "INFRA_CONTAINER_INGRESS_EXTERNAL";
        public const string IngressTargetPort = "INFRA_CONTAINER_INGRESS_TARGET_PORT";
        public const string IngressAllowInsecure = "INFRA_CONTAINER_INGRESS_ALLOW_INSECURE";
    }

    /// <summary>
    /// Monitoring settings
    /// </summary>
    public static class Monitoring
    {
        public const string LogRetentionDays = "INFRA_MONITORING_LOG_RETENTION_DAYS";
        public const string ApplicationInsightsType = "INFRA_MONITORING_APP_INSIGHTS_TYPE";
        public const string EnableDetailedMetrics = "INFRA_MONITORING_ENABLE_DETAILED_METRICS";
        public const string EnableLiveMetrics = "INFRA_MONITORING_ENABLE_LIVE_METRICS";
    }

    /// <summary>
    /// Cache settings
    /// </summary>
    public static class Cache
    {
        public const string SkuName = "INFRA_CACHE_SKU_NAME";
        public const string SkuFamily = "INFRA_CACHE_SKU_FAMILY";
        public const string SkuCapacity = "INFRA_CACHE_SKU_CAPACITY";
        public const string EnableNonSslPort = "INFRA_CACHE_ENABLE_NON_SSL_PORT";
        public const string MaxMemoryPolicy = "INFRA_CACHE_MAX_MEMORY_POLICY";
    }

    /// <summary>
    /// Event Hubs settings
    /// </summary>
    public static class EventHubs
    {
        public const string SkuName = "INFRA_EVENTHUBS_SKU_NAME";
        public const string SkuTier = "INFRA_EVENTHUBS_SKU_TIER";
        public const string SkuCapacity = "INFRA_EVENTHUBS_SKU_CAPACITY";
        public const string EnableAutoInflate = "INFRA_EVENTHUBS_ENABLE_AUTO_INFLATE";
        public const string MaximumThroughputUnits = "INFRA_EVENTHUBS_MAX_THROUGHPUT_UNITS";
        public const string MessageRetentionDays = "INFRA_EVENTHUBS_MESSAGE_RETENTION_DAYS";
        public const string PartitionCount = "INFRA_EVENTHUBS_PARTITION_COUNT";
    }

    /// <summary>
    /// Network settings
    /// </summary>
    public static class Network
    {
        public const string VirtualNetworkAddressSpace = "INFRA_NETWORK_VNET_ADDRESS_SPACE";
        public const string ContainerAppsSubnetAddressSpace = "INFRA_NETWORK_CONTAINER_APPS_SUBNET";
        public const string DatabaseSubnetAddressSpace = "INFRA_NETWORK_DATABASE_SUBNET";
        public const string CacheSubnetAddressSpace = "INFRA_NETWORK_CACHE_SUBNET";
        public const string PrivateEndpointsSubnetAddressSpace = "INFRA_NETWORK_PRIVATE_ENDPOINTS_SUBNET";
        public const string ApplicationGatewaySubnetAddressSpace = "INFRA_NETWORK_APP_GATEWAY_SUBNET";
        public const string EnableDdosProtection = "INFRA_NETWORK_ENABLE_DDOS_PROTECTION";
        public const string CustomDomain = "INFRA_NETWORK_CUSTOM_DOMAIN";
        public const string SslCertificateName = "INFRA_NETWORK_SSL_CERT_NAME";
    }

    /// <summary>
    /// Key Vault settings
    /// </summary>
    public static class KeyVault
    {
        public const string SkuName = "INFRA_KEYVAULT_SKU_NAME";
        public const string EnableSoftDelete = "INFRA_KEYVAULT_ENABLE_SOFT_DELETE";
        public const string SoftDeleteRetentionDays = "INFRA_KEYVAULT_SOFT_DELETE_RETENTION_DAYS";
        public const string EnablePurgeProtection = "INFRA_KEYVAULT_ENABLE_PURGE_PROTECTION";
        public const string EnableRbacAuthorization = "INFRA_KEYVAULT_ENABLE_RBAC_AUTHORIZATION";

        /// <summary>
        /// Environment variable name for secret prefixes (comma-separated).
        /// Example: KEYVAULT_SECRET_PREFIXES=STRIPE_,SENDGRID_,CUSTOM_
        /// Client must explicitly set this or pass customPrefixes parameter.
        /// </summary>
        public const string SecretPrefixes = "KEYVAULT_SECRET_PREFIXES";
    }

    /// <summary>
    /// API settings
    /// </summary>
    public static class Api
    {
        public const string EnableHttps = "INFRA_API_ENABLE_HTTPS";
        public const string EnableSwagger = "INFRA_API_ENABLE_SWAGGER";
        public const string CorsOrigins = "INFRA_API_CORS_ORIGINS";
        public const string RateLimitRequestsPerMinute = "INFRA_API_RATE_LIMIT_REQUESTS_PER_MINUTE";
        public const string EnableDetailedErrors = "INFRA_API_ENABLE_DETAILED_ERRORS";
    }

    /// <summary>
    /// Green-Blue deployment settings
    /// </summary>
    public static class GreenBlueDeployment
    {
        public const string Enable = "INFRA_GREENBLUE_ENABLE";
        public const string TrafficSplitPercentage = "INFRA_GREENBLUE_TRAFFIC_SPLIT_PERCENTAGE";
        public const string HealthCheckPath = "INFRA_GREENBLUE_HEALTH_CHECK_PATH";
        public const string HealthCheckIntervalSeconds = "INFRA_GREENBLUE_HEALTH_CHECK_INTERVAL_SECONDS";
        public const string HealthCheckTimeoutSeconds = "INFRA_GREENBLUE_HEALTH_CHECK_TIMEOUT_SECONDS";
    }

    /// <summary>
    /// Storage settings
    /// </summary>
    public static class Storage
    {
        public const string ReplicationType = "INFRA_STORAGE_REPLICATION_TYPE";
        public const string EnableBlobPublicAccess = "INFRA_STORAGE_ENABLE_BLOB_PUBLIC_ACCESS";
        public const string EnableHttpsTrafficOnly = "INFRA_STORAGE_ENABLE_HTTPS_TRAFFIC_ONLY";
        public const string MinimumTlsVersion = "INFRA_STORAGE_MINIMUM_TLS_VERSION";
        public const string EnableVersioning = "INFRA_STORAGE_ENABLE_VERSIONING";
    }

    /// <summary>
    /// Azure Front Door settings
    /// </summary>
    public static class FrontDoor
    {
        public const string Enabled = "INFRA_FRONTDOOR_ENABLED";
        public const string SkuName = "INFRA_FRONTDOOR_SKU_NAME";
        public const string EndpointNameOverride = "INFRA_FRONTDOOR_ENDPOINT_NAME_OVERRIDE";
        public const string HealthProbePath = "INFRA_FRONTDOOR_HEALTH_PROBE_PATH";
        public const string EnableCustomDomain = "INFRA_FRONTDOOR_ENABLE_CUSTOM_DOMAIN";
        public const string CustomDomainHostName = "INFRA_FRONTDOOR_CUSTOM_DOMAIN_HOSTNAME";
        public const string EnableWaf = "INFRA_FRONTDOOR_ENABLE_WAF";
        public const string AllowedIpRanges = "INFRA_FRONTDOOR_ALLOWED_IP_RANGES";
        public const string WafBypassPathPrefixes = "INFRA_FRONTDOOR_WAF_BYPASS_PATH_PREFIXES";
    }

    /// <summary>
    /// Slot-specific settings (Green/Blue slots)
    /// </summary>
    public static class Slot
    {
        public const string GreenPrefix = "INFRA_GREEN_SLOT";
        public const string BluePrefix = "INFRA_BLUE_SLOT";

        public const string ImageTagSuffix = "_IMAGE_TAG";
        public const string CpuLimitSuffix = "_CPU_LIMIT";
        public const string MemoryLimitSuffix = "_MEMORY_LIMIT";
        public const string MinReplicasSuffix = "_MIN_REPLICAS";
        public const string MaxReplicasSuffix = "_MAX_REPLICAS";
        public const string EnvironmentVariablePrefix = "_ENV_";
    }
}

