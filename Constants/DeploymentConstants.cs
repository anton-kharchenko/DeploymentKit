namespace DeploymentKit.Constants;

/// <summary>
/// Contains constant values used throughout the deployment infrastructure.
/// These constants provide type safety and centralized management of commonly used string values.
/// </summary>
public static class DeploymentConstants
{
    public const string SystemAssignedIdentity = "System";

    /// <summary>
    /// Constants related to logging and monitoring destinations.
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// Log Analytics destination identifier.
        /// </summary>
        public const string LogAnalyticsDestination = "log-analytics";
    }

    /// <summary>
    /// URL and endpoint constants
    /// </summary>
    public static class Urls
    {
        public const string HttpsScheme = "https://";
        public const string AzureContainerAppsDomain = ".azurecontainerapps.io";
        public const string HealthEndpoint = "/health";
        public const string LocalhostOtlpEndpoint = "http://localhost:4317";
        public const string DefaultAspNetCoreUrls = "http://+:8080";
    }

    /// <summary>
    /// Constants related to traffic distribution and percentages.
    /// </summary>
    public static class Traffic
    {
        /// <summary>
        /// Full traffic percentage (100%).
        /// </summary>
        public const int FullTrafficPercentage = 100;
    }

    /// <summary>
    /// Resource tag constants for consistent tagging across resources
    /// </summary>
    public static class ResourceTags
    {
        // Slot and deployment tags
        public const string SlotKey = "Slot";
        public const string ImageTagKey = "ImageTag";
        public const string DeploymentTypeKey = "DeploymentType";
        public const string GreenBlueDeploymentType = "green-blue";
        public const string ActiveSlotKey = "ActiveSlot";
        public const string TrafficSwitchTimestampKey = "TrafficSwitchTimestamp";

        // Resource type tags
        public const string PublicIpType = "public-ip";
        public const string ApplicationGatewayType = "application-gateway";
        public const string ContainerAppType = "container-app";
        public const string DatabaseType = "database";
        public const string LogAnalyticsType = "log-analytics";
        public const string ApplicationInsightsType = "application-insights";
        public const string StorageAccountType = "storage-account";
        public const string RedisCacheType = "redis-cache";
        public const string ContainerAppsEnvironmentType = "container-apps-environment";
        public const string ContainerAppApiType = "container-app-api";
        public const string ContainerAppJobsType = "container-app-jobs";
        public const string PostgreSqlServerType = "postgresql-server";
        public const string NetworkSecurityGroupType = "network-security-group";
        public const string PrivateDnsZoneType = "private-dns-zone";
        public const string PrivateDnsZoneLinkType = "private-dns-zone-link";
        public const string KeyVaultType = "key-vault";
    }

    /// <summary>
    /// Database-related constants for infrastructure deployment
    /// </summary>
    public static class Database
    {
        // Character sets and collations
        public const string Utf8Charset = "UTF8";
        public const string DefaultCollation = "en_US.utf8";
    }

    /// <summary>
    /// Storage-related constants
    /// </summary>
    public static class Storage
    {
        public const string ConnectionStringTemplate = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix=core.windows.net";
    }

    /// <summary>
    /// Cache-related constants
    /// </summary>
    public static class Cache
    {
        public const string RedisConnectionStringTemplate = "{0}:{1},password={2},ssl={3}";
    }

    /// <summary>
    /// Network-related constants for infrastructure deployment
    /// </summary>
    public static class Network
    {
        // Allocation and SKU constants
        public const string StaticAllocation = "Static";
        public const string StandardSku = "Standard";
        public const string RegionalTier = "Regional";
        public const string StandardV2Sku = "Standard_v2";
        public const string StandardV2Tier = "Standard_v2";

        // Capacity and scaling constants
        public const int DefaultCapacity = 2;
        public const int MinCapacity = 1;
        public const int MaxCapacity = 10;

        // Port constants
        public const int HttpPort = 80;
        public const int HttpsPort = 443;
        public const string Port80Name = "port_80";
        public const string Port443Name = "port_443";

        // Configuration names
        public const string BackendPoolName = "appServiceBackendPool";
        public const string BackendHttpSettingsName = "appServiceBackendHttpSettings";
        public const string HttpListenerName = "appGatewayHttpListener";
        public const string FrontendIpConfigName = "appGatewayFrontendIP";
        public const string RoutingRuleName = "rule1";
        public const string HealthProbeName = "appServiceHealthProbe";

        // Configuration values
        public const string DisabledAffinity = "Disabled";
        public const int DefaultRequestTimeout = 30;
        public const string BasicRuleType = "Basic";
        public const int DefaultPriority = 100;

        // Health check constants
        public const string HealthCheckPath = "/health";
        public const int DefaultProbeInterval = 30;
        public const int DefaultProbeTimeout = 30;
        public const int DefaultUnhealthyThreshold = 3;
        public const int MinServers = 0;
        public const string HealthyStatusCodes = "200-399";

        // WAF Configuration
        public const string PreventionMode = "Prevention";
        public const string OwaspRuleSet = "OWASP";
        public const string OwaspVersion = "3.2";

        // Security Rule Names
        public const string AllowHttpsFromAppGatewayRule = "AllowHttpsFromAppGateway";
        public const string AllowHttpFromAppGatewayRule = "AllowHttpFromAppGateway";
        public const string AllowContainerAppsInternalRule = "AllowContainerAppsInternal";
        public const string AllowPostgreSQLFromContainerAppsRule = "AllowPostgreSQLFromContainerApps";
        public const string DenyAllInboundRule = "DenyAllInbound";

        // Security Rule Directions
        public const string InboundDirection = "Inbound";

        // Security Rule Access
        public const string AllowAccess = "Allow";
        public const string DenyAccess = "Deny";

        // Security Rule Protocols
        public const string AllProtocols = "*";
        public const string TcpProtocol = "Tcp";

        // Security Rule Priorities
        public const int HttpsFromAppGatewayPriority = 1000;
        public const int HttpFromAppGatewayPriority = 1100;
        public const int ContainerAppsInternalPriority = 2000;
        public const int PostgreSqlFromContainerAppsPriority = 3000;
        public const int DenyAllInboundPriority = 4000;

        // Port Numbers
        public const string PostgreSqlPort = "5432";

        // Address Prefixes
        public const string AllAddresses = "*";

        // Subnet Types
        public const string ContainerAppSubnetType = "containerapp";
        public const string DatabaseSubnetType = "database";
        public const string PrivateEndpointsSubnetType = "privateendpoints";
        public const string AppGatewaySubnetType = "appgateway";

        // Network Policies
        public const string DisabledNetworkPolicies = "Disabled";

        // DNS Configuration
        public const string GlobalLocation = "Global";
        public const string AzureContainerAppsDomain = "azurecontainerapps.io";
        public const string PostgreSQLDomain = "postgres.database.azure.com";
        public const string VNetLinkSuffix = "-vnet-link";

        // Delegation Names
        public const string ContainerAppEnvironmentDelegation = "Microsoft.App.environments";
        public const string ContainerAppEnvironmentService = "Microsoft.App/environments";
        public const string PostgreSQLFlexibleServerDelegation = "Microsoft.DBforPostgreSQL.flexibleServers";
        public const string PostgreSQLFlexibleServerService = "Microsoft.DBforPostgreSQL/flexibleServers";
    }

    /// <summary>
    /// Constants for Green-Blue Deployment
    /// </summary>
    public static class GreenBlue
    {
        public const string GreenSlotName = "green";
        public const string BlueSlotName = "blue";
        public const string ApiGreenName = "api-green";
        public const string ApiBlueName = "api-blue";
        public const string ApiMainName = "api-main";
    }

    /// <summary>
    /// Container Apps configuration constants
    /// </summary>
    public static class ContainerApps
    {
        public const string HttpScalingRuleName = "http-scaling";
        public const string CpuScalingRuleName = "cpu-scaling";
        public const string CpuScalingType = "cpu";
        public const string CpuUtilizationType = "Utilization";
        public const string AcrPasswordSecretName = "acr-password";
        public const string DbPasswordSecretName = "db-password";
        public const string PostgresConnectionStringSecretName = "postgres-connection-string";
        public const string DefaultCpu = "0.5";
        public const string DefaultMemory = "1Gi";
        public const string DefaultCpuThreshold = "70";
        public const string ConcurrentRequestsMetadata = "concurrentRequests";
        public const string DefaultConcurrentRequests = "10";
    }
}

