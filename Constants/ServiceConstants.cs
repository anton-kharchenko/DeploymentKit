namespace DeploymentKit.Constants;

/// <summary>
/// Constants for service operations and configurations
/// </summary>
public static class ServiceConstants
{
    /// <summary>
    /// Container application types
    /// </summary>
    public static class ContainerAppTypes
    {
        public const string Api = "api";
        public const string Jobs = "jobs";
        public const string Bot = "bot";
    }

    /// <summary>
    /// Storage account types
    /// </summary>
    public static class StorageAccountTypes
    {
        public const string StandardLrs = "Standard_LRS";
        public const string StandardGrs = "Standard_GRS";
        public const string StandardRagrs = "Standard_RAGRS";
        public const string StandardZrs = "Standard_ZRS";
        public const string PremiumLrs = "Premium_LRS";
    }

    /// <summary>
    /// Health check constants
    /// </summary>
    public static class HealthCheck
    {
        public const string ConnectivityCheckName = "Connectivity";
        public const string HealthEndpointCheckName = "HealthEndpoint";
        public const string ReadinessCheckName = "Readiness";
        public const string PerformanceCheckName = "Performance";

        public const string HealthyStatus = "Healthy";
        public const string UnhealthyStatus = "Unhealthy";
        public const string FailedNoConnectivityStatus = "Failed - No connectivity";
        public const string ErrorStatusPrefix = "Error: ";
    }

    /// <summary>
    /// Environment variable names for container applications
    /// </summary>
    public static class EnvironmentVariables
    {
        public const string AspNetCoreUrls = "ASPNETCORE_URLS";
        public const string AspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";
        public const string PostgresConnectionString = "ConnectionStrings__Db";
        public const string RedisConnectionString = "ConnectionStrings__Redis";
        public const string EventHubsConnectionString = "ConnectionStrings__EventHubs";
        public const string KafkaConnectionString = "ConnectionStrings__kafka";
        public const string ApplicationInsightsConnectionString = "APPLICATIONINSIGHTS_CONNECTION_STRING";
        public const string OtelExporterEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";
        public const string AzureFrontDoorId = "AZURE_FRONTDOOR_ID";
        public const string BotWebhookUrl = "Bot__WebhookUrl";
        public const string BotWebhookSecretToken = "Bot__WebhookSecretToken";
        public const string BotMiniAppUrl = "Bot__MiniAppUrl";
        public const string KafkaSecurityProtocol = "Aspire__Confluent__Kafka__Consumer__Config__SecurityProtocol";
        public const string KafkaSaslMechanism = "Aspire__Confluent__Kafka__Consumer__Config__SaslMechanism";
        public const string KafkaSaslUsername = "Aspire__Confluent__Kafka__Consumer__Config__SaslUsername";
        public const string KafkaSaslPassword = "Aspire__Confluent__Kafka__Consumer__Config__SaslPassword";
    }

    /// <summary>
    /// Default values for container applications
    /// </summary>
    public static class ContainerDefaults
    {
        public const string AspNetCoreUrlsValue = "http://+:8080";
        public const string OtelExporterEndpointValue = "http://localhost:4317";
        public const int DefaultTargetPort = 8080;
        public const string DefaultTransport = "Http";
        public const string PlaceholderImage = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest";
    }

    /// <summary>
    /// Auto-scaling configuration constants
    /// </summary>
    public static class AutoScaling
    {
        public const string CpuType = "cpu";
        public const string MemoryType = "memory";
        public const string UtilizationType = "Utilization";
        public const string ConcurrentRequestsKey = "concurrentRequests";
        public const string TypeKey = "type";
        public const string ValueKey = "value";
    }

    /// <summary>
    /// Resource type names
    /// </summary>
    public static class ResourceTypes
    {
        public const string ContainerApps = "ContainerApps";
        public const string Database = "Database";
        public const string PostgreSQL = "PostgreSQL";
        public const string PostgreSQLServer = "PostgreSQLServer";
        public const string Redis = "Redis";
        public const string RedisCache = "RedisCache";
        public const string Storage = "Storage";
        public const string StorageAccount = "StorageAccount";
        public const string StorageAccountKeys = "StorageAccountKeys";
        public const string Monitoring = "Monitoring";
        public const string LogAnalyticsWorkspace = "LogAnalyticsWorkspace";
        public const string ApplicationInsights = "ApplicationInsights";
        public const string LogAnalyticsApplicationInsights = "LogAnalytics/ApplicationInsights";
        public const string Network = "Network";
        public const string VirtualNetwork = "VirtualNetwork";
        public const string VirtualNetworkSubnets = "VirtualNetwork/Subnets";
        public const string NetworkSecurityGroup = "NetworkSecurityGroup";
        public const string PublicIP = "PublicIP";
        public const string ContainerRegistry = "ContainerRegistry";
        public const string Registry = "Registry";
        public const string DomainOptimization = "DomainOptimization";
        public const string CdnDns = "CDN/DNS";
        public const string VPN = "VPN";
        public const string VpnGateway = "VpnGateway";
    }

    /// <summary>
    /// Error codes for service operations
    /// </summary>
    public static class ErrorCodes
    {
        public const string ContainerAppsCreationFailed = "CONTAINER_APPS_CREATION_FAILED";
        public const string DatabaseCreationFailed = "DATABASE_CREATION_FAILED";
        public const string RedisCreationFailed = "REDIS_CREATION_FAILED";
        public const string StorageCreationFailed = "STORAGE_CREATION_FAILED";
        public const string MonitoringCreationFailed = "MONITORING_CREATION_FAILED";
        public const string NetworkCreationFailed = "NETWORK_CREATION_FAILED";
        public const string AppGatewayCreationFailed = "APP_GATEWAY_CREATION_FAILED";
        public const string AcrCreationFailed = "ACR_CREATION_FAILED";
        public const string DomainOptimizationCreationFailed = "DOMAIN_OPTIMIZATION_CREATION_FAILED";
        public const string VpnGatewayCreationFailed = "VPN_GATEWAY_CREATION_FAILED";
    }

    /// <summary>
    /// Service operation names
    /// </summary>
    public static class ServiceOperations
    {
        public const string CreateRedisCache = "CreateRedisCache";
        public const string CreatePostgreSqlInfrastructure = "CreatePostgreSQLInfrastructure";
        public const string CreateStorageAccount = "CreateStorageAccount";
        public const string CreateNetworkInfrastructure = "CreateNetworkInfrastructure";
        public const string ListKeys = "ListKeys";
    }

    /// <summary>
    /// Validation constants for deployment validation
    /// </summary>
    public static class Validation
    {
        public const string InProgressStatus = "InProgress";
        public const string GreenSlot = "green";
        public const string BlueSlot = "blue";
        public const string ProductionEnvironment = "production";
        public const string DevelopmentEnvironment = "development";

        public static class Messages
        {
            public const string HealthCheckTimeoutPositive = "Health check timeout must be positive";
            public const string TrafficShiftIntervalPositive = "Traffic shift interval must be positive";
            public const string MaxTrafficShiftPercentageRange = "Max traffic shift percentage must be between 1 and 100";
            public const string RollbackThresholdPercentageRange = "Rollback threshold percentage must be between 0 and 100";
            public const string SlotNameCannotBeEmpty = "slot name cannot be empty";
            public const string SlotCpuLimitPositive = "slot CPU limit must be positive";
            public const string SlotMemoryLimitPositive = "slot memory limit must be positive";
            public const string SlotMinReplicasNonNegative = "slot minimum replicas cannot be negative";
            public const string SlotMaxReplicasGreaterThanMin = "slot maximum replicas must be >= minimum replicas";
            public const string SlotTrafficPercentageRange = "slot traffic percentage must be between 0 and 100";
            public const string TotalTrafficMustEqual100 = "Total traffic percentage must equal 100% (current: {0}%)";
            public const string AtLeastOneSlotMustReceiveTraffic = "At least one slot must receive traffic";
            public const string ApiImageTagRequired = "API image tag is required";
            public const string JobsImageTagRequired = "Jobs image tag is required";
            public const string ApiCpuLimitPositive = "API CPU limit must be positive";
            public const string ApiMemoryLimitPositive = "API memory limit must be positive";
            public const string GreenBlueRecommendedForProduction = "Green-blue deployment is recommended for production environments";
            public const string ProductionShouldHaveTwoReplicas = "Production environments should have at least 2 replicas per slot for high availability";
            public const string DevelopmentDoesntNeedManyReplicas = "Development environments typically don't need more than 2 replicas per slot";
            public const string CannotStartNewDeploymentInProgress = "Cannot start new deployment while another deployment is in progress";
            public const string ActiveSlotMustBeSpecified = "Active slot must be specified";
            public const string ActiveSlotMustBeGreenOrBlue = "Active slot must be either 'green' or 'blue'";
            public const string ResourceGroupNameRequired = "Resource group name is required";
            public const string AzureLocationRequired = "Azure location is required";
            public const string NoInactiveSlotForRollback = "No inactive slot available for rollback - inactive slot has no image";
            public const string InactiveSlotVersionMissing = "Inactive slot version information is missing";
            public const string BothSlotsRunSameVersion = "Both slots are running the same version - rollback may not provide different functionality";
            public const string InactiveSlotHealthCheckWarning = "Inactive slot has a recorded last health check timestamp; ensure health checks are passing before rollback";
            public const string InactiveSlotOldDeployment = "Inactive slot was deployed {0:F1} days ago - consider the age of the rollback target";
            public const string InvalidVNetAddressSpace = "Invalid VNet address space CIDR: {0}";
            public const string InvalidContainerAppsSubnet = "Invalid Container Apps subnet address space CIDR: {0}";
            public const string InvalidDatabaseSubnet = "Invalid Database subnet address space CIDR: {0}";
            public const string InvalidStorageAccountNameFormat = "Invalid storage account name format: {0}";
            public const string InvalidStorageAccountType = "Invalid storage account type: {0}";
            public const string HighCpuRequirement = "High CPU requirement detected: {0} cores";
            public const string HighMemoryRequirement = "High memory requirement detected: {0}MB";
            public const string InvalidApiImageTagFormat = "Invalid API image tag format: {0}";
            public const string InvalidJobsImageTagFormat = "Invalid Jobs image tag format: {0}";
            public const string InvalidRegistryUrlFormat = "Invalid registry URL format: {0}. Registry URL must be a valid absolute URI";
            public const string RegistryUrlFormatValid = "Registry URL format is valid";
            public const string InvalidImageTagFormat = "Invalid image tag format: {0}. Image tag must not contain invalid characters or be empty";
            public const string ImageTagFormatValid = "Image tag format is valid";
            public const string RegistryAccessible = "Registry is accessible at {0}";
            public const string RegistryConnectivityFailed = "Registry connectivity failed: HTTP {0} {1}";
            public const string RegistryConnectivityTimeout = "Registry connectivity check timed out";
            public const string ImageExists = "Image {0}:{1} exists in registry";
            public const string ImageNotFound = "Image {0}:{1} not found in registry";
            public const string ImageExistenceAuthRequired = "Cannot verify image existence: Authentication required for {0}:{1}";
            public const string ImageExistenceInconclusive = "Image existence check inconclusive: HTTP {0} {1}";
            public const string ImageExistenceCheckFailed = "Image existence check failed: {0}";
            public const string ImageExistenceCheckTimeout = "Image existence check timed out";
        }

        public static class HttpHeaders
        {
            public const string CorrelationId = "X-Correlation-Id";
            public const string DockerManifestMediaType = "application/vnd.docker.distribution.manifest.v2+json";
        }

        public static class Endpoints
        {
            public const string RegistryV2Api = "/v2/";
            public const string ManifestPath = "/v2/{0}/manifests/{1}";
        }

        public static class Limits
        {
            public const int HighCpuThreshold = 10; // CPU cores
            public const int HighMemoryThreshold = 20480; // MB (20GB)
            public const int StorageAccountNameMinLength = 3;
            public const int StorageAccountNameMaxLength = 24;
            public const int RegistryConnectivityTimeoutSeconds = 10;
            public const int ImageExistenceTimeoutSeconds = 15;
            public const int RollbackAgeWarningDays = 7;
        }
    }

    /// <summary>
    /// VPN Gateway service constants
    /// </summary>
    public static class VpnGateway
    {
        // Logging messages
        public const string CreationStartMessage = "Creating VPN Gateway infrastructure for environment: {Environment}";
        public const string CreationSuccessMessage = "Successfully created VPN Gateway infrastructure for environment: {Environment}";
        public const string CreationFailedMessage = "Failed to create VPN Gateway infrastructure for environment: {Environment}";
        public const string DisabledMessage = "VPN Gateway is disabled for environment: {Environment}";
        public const string ClientConfigGenerationMessage = "Generating VPN client configuration for environment: {Environment}";
        public const string ClientConfigGenerationFailedMessage = "Failed to generate VPN client configuration for environment: {Environment}";
        public const string SubnetCreationMessage = "Creating VPN Gateway subnet: {SubnetName}";
        public const string PublicIpCreationMessage = "Creating VPN Gateway Public IP: {PublicIpName}";
        public const string GatewayCreationMessage = "Creating VPN Gateway: {VpnGatewayName}";
        public const string P2SConfigurationMessage = "Configuring Point-to-Site VPN for environment: {Environment}";

        // Gateway configuration
        public const string GatewaySubnetName = "GatewaySubnet";
        public const string DefaultIpConfigurationName = "default";
        public const string GatewayType = "Vpn";
        public const string VpnType = "RouteBased";
        public const string SkuName = "VpnGw1";
        public const string SkuTier = "VpnGw1";
        public const string PrivateIpAllocationMethod = "Dynamic";

        // Naming patterns
        public const string P2SEnvironmentFormat = "P2S-{0}";

        // Certificate configuration
        public const string RootCertificateName = "ApplicationRootCert";
        public const string PlaceholderCertificateData = "PLACEHOLDER_FOR_ROOT_CERT_DATA";

        // AAD authentication URLs
        public const string AadTenantUrl = "https://login.microsoftonline.com/common/";
        public const string AzureVpnClientAppId = "41b23e61-6c1e-4545-b367-cd054e0ed4b4";
        public const string AadIssuerUrl = "https://sts.windows.net/common/";

        // Resource types
        public const string VpnGatewayResourceType = "vpn-gateway";
        public const string VpnGatewayPublicIpResourceType = "vpn-gateway-public-ip";

        /// <summary>
        /// VPN client configuration template constants
        /// </summary>
        public static class ClientConfigurationTemplate
        {
            public const string Header = "VPN Configuration for {0} Environment";
            public const string Separator = "=====================================";
            public const string GatewayLabel = "Gateway Configuration:";
            public const string TunnelTypeLabel = "- Tunnel Type: {0}";
            public const string AuthenticationLabel = "- Authentication: {0}";
            public const string ClientAddressPoolLabel = "- Client Address Pool: {0}";
            public const string ConnectionInstructions = "Connection Instructions:\n1. Download the VPN client configuration\n2. Install the VPN client\n3. Import the configuration file\n4. Connect using your credentials";
            public const string SecurityFeatures = "Security Features:\n- Certificate-based authentication\n- Azure AD integration\n- OpenVPN and IKEv2 protocols supported";
        }
    }

    /// <summary>
    /// Monitoring service constants
    /// </summary>
    public static class Monitoring
    {
        // Logging messages
        public const string CreationStartMessage = "Starting monitoring resources creation for environment: {Environment} with CorrelationId: {CorrelationId}";
        public const string CreationSuccessMessage = "Successfully created monitoring resources in {ElapsedMs}ms - LogAnalytics: {LogAnalyticsName}, AppInsights: {AppInsightsName} for CorrelationId: {CorrelationId}";
        public const string CreationFailedMessage = "Failed to create monitoring resources for environment: {Environment} after {ElapsedMs}ms for CorrelationId: {CorrelationId}";
        public const string CreationCancelledMessage = "Monitoring resources creation was cancelled after {ElapsedMs}ms for CorrelationId: {CorrelationId}";
        public const string ResourceNamesGeneratedMessage = "Generated resource names - LogAnalytics: {LogAnalyticsName}, AppInsights: {AppInsightsName} for CorrelationId: {CorrelationId}";

        // Log Analytics messages
        public const string LogAnalyticsCreationMessage = "Creating Log Analytics Workspace: {LogAnalyticsName} for CorrelationId: {CorrelationId}";
        public const string LogAnalyticsCreatedMessage = "Log Analytics Workspace created in {ElapsedMs}ms for CorrelationId: {CorrelationId}";
        public const string LogAnalyticsConfigurationMessage = "Creating Log Analytics Workspace with retention: {RetentionDays} days for CorrelationId: {CorrelationId}";
        public const string LogAnalyticsConfiguredMessage = "Log Analytics Workspace resource configured for CorrelationId: {CorrelationId}";
        public const string LogAnalyticsCreationFailedMessage = "Failed to create Log Analytics Workspace: {LogAnalyticsName} for CorrelationId: {CorrelationId}";

        // Application Insights messages
        public const string AppInsightsCreationMessage = "Creating Application Insights: {AppInsightsName} for CorrelationId: {CorrelationId}";
        public const string AppInsightsCreatedMessage = "Application Insights created in {ElapsedMs}ms for CorrelationId: {CorrelationId}";
        public const string AppInsightsConfigurationMessage = "Creating Application Insights with type: {AppInsightsType} for CorrelationId: {CorrelationId}";
        public const string AppInsightsConfiguredMessage = "Application Insights resource configured for CorrelationId: {CorrelationId}";
        public const string AppInsightsCreationFailedMessage = "Failed to create Application Insights: {AppInsightsName} for CorrelationId: {CorrelationId}";

        // Workspace keys messages
        public const string WorkspaceKeysRetrievalMessage = "Retrieving Log Analytics workspace shared keys for CorrelationId: {CorrelationId}";
        public const string WorkspaceKeysRetrievedMessage = "Log Analytics workspace shared keys retrieved in {ElapsedMs}ms for CorrelationId: {CorrelationId}";

        // Error message format
        public const string EnvironmentCreationFailedFormat = "Failed to create monitoring resources for environment '{0}'";
    }

    /// <summary>
    /// Container Registry configuration constants
    /// </summary>
    public static class ContainerRegistry
    {
        public const string ResourceType = "container-registry";

        // Error messages
        public const string CreationFailedMessage = "Failed to create Azure Container Registry for environment '{0}'";
        public const string CreationStartMessage = "Creating Azure Container Registry for environment: {0}";
        public const string CreationSuccessMessage = "Successfully created Azure Container Registry: {0}";
    }

    /// <summary>
    /// Application Gateway configuration constants
    /// </summary>
    public static class ApplicationGateway
    {
        // Configuration names
        public const string IpConfigurationName = "appGatewayIpConfig";
        public const string FrontendIpConfigurationName = "appGatewayFrontendIP";
        public const string BackendPoolName = "appServiceBackendPool";

        // URL schemes
        public const string HttpsScheme = "https://";
        public const string HttpScheme = "http://";

        // Logging messages
        public const string CreationStartMessage = "Creating Application Gateway for environment: {0}";
        public const string CreationSuccessMessage = "Successfully created Application Gateway for environment: {0}";
        public const string CreationFailedMessage = "Failed to create Application Gateway for environment: {0}";
        public const string PublicIpCreationMessage = "Creating Public IP: {0}";
        public const string AppGatewayCreationMessage = "Creating Application Gateway: {0}";
    }

    /// <summary>
    /// Container Apps service constants
    /// </summary>
    public static class ContainerApps
    {
        public const string CreationStartMessage = "Creating Container Apps environment and applications for environment: {Environment}";
        public const string CreationSuccessMessage = "Successfully created Container Apps environment: {EnvironmentName} with API and Jobs apps";
        public const string CreationFailedMessage = "Failed to create Container Apps for environment: {Environment}";
        public const string AcrPasswordSecretRef = "acr-password";
        public const string DbPasswordSecretRef = "db-password";
        public const string PostgresConnectionStringSecretRef = "postgres-connection-string";
        public const string CpuScalingRuleName = "cpu-scaling-rule";
        public const string MemoryScalingRuleName = "memory-scaling-rule";
        public const string HttpScalingRuleName = "http-scaling-rule";
        public const string LogAnalyticsDestination = "log-analytics";
        public const string MemoryUnit = "Gi";
    }

    /// <summary>
    /// Network service constants
    /// </summary>
    public static class Network
    {
        // Logging messages
        public const string CreationStartMessage = "Starting network infrastructure creation for environment: {Environment} with CorrelationId: {CorrelationId}. VNet Address Space: {AddressSpace}";
        public const string CreationSuccessMessage = "Successfully created complete network infrastructure for environment: {Environment} in {TotalElapsedMs}ms with CorrelationId: {CorrelationId}. Resources: VNet={VNetName}, Subnets=4, NSGs=2, DNS Zones=2";
        public const string CreationCancelledMessage = "Network infrastructure creation was cancelled for environment: {Environment} after {ElapsedMs}ms with CorrelationId: {CorrelationId}";
        public const string CreationFailedMessage = "Failed to create network infrastructure for environment: {Environment} after {ElapsedMs}ms with CorrelationId: {CorrelationId}. Error: {ErrorMessage}";

        // NSG messages
        public const string DatabaseNsgCreationMessage = "Creating Database NSG: {NsgName} for CorrelationId: {CorrelationId}";
        public const string DatabaseNsgConfiguredMessage = "Database NSG {NsgName} configured with PostgreSQL access rules for CorrelationId: {CorrelationId}";
        public const string NsgCreationFailedMessage = "Failed to create Network Security Group for CorrelationId: {CorrelationId}. Error: {ErrorMessage}";
        public const string DatabaseNsgCreationFailedMessage = "Failed to create Database NSG for CorrelationId: {CorrelationId}. Error: {ErrorMessage}";

        // Subnet messages
        public const string DatabaseSubnetConfiguredMessage = "Database Subnet {SubnetName} configured with delegation to Microsoft.DBforPostgreSQL/flexibleServers for CorrelationId: {CorrelationId}";
        public const string DatabaseSubnetCreationFailedMessage = "Failed to create Database Subnet for CorrelationId: {CorrelationId}. Error: {ErrorMessage}";

        // DNS Zone messages
        public const string DnsZoneCreationParallelMessage = "Creating Private DNS Zones in parallel for CorrelationId: {CorrelationId}";
        public const string DnsZonesCreatedMessage = "Private DNS Zones created in {ElapsedMs}ms for CorrelationId: {CorrelationId}";
    }

    /// <summary>
    /// Storage service constants
    /// </summary>
    public static class Storage
    {
        // Logging messages
        public const string CreationStartMessage = "Starting Azure Storage Account creation for environment: {Environment} with CorrelationId: {CorrelationId}. Kind: {Kind}, SKU: {SkuName}, AccessTier: {AccessTier}, HTTPS Only: {EnableHttpsTrafficOnly}";
        public const string CreationSuccessMessage = "Successfully created complete Storage Account infrastructure for environment: {Environment} in {TotalElapsedMs}ms with CorrelationId: {CorrelationId}. Account: {StorageAccountName}, Kind: {Kind}, SKU: {SkuName}";
        public const string CreationFailedMessage = "Failed to create Storage Account for environment: {Environment} after {ElapsedMs}ms with CorrelationId: {CorrelationId}. Error: {ErrorMessage}";
        public const string CreationCancelledMessage = "Storage Account creation was cancelled for environment: {Environment} after {ElapsedMs}ms with CorrelationId: {CorrelationId}";

        // Storage Account messages
        public const string AccountNameGeneratedMessage = "Generated storage account name for CorrelationId: {CorrelationId} - StorageAccountName: {StorageAccountName}";
        public const string AccountCreationStartMessage = "Creating Storage Account: {StorageAccountName} in location: {Location} for CorrelationId: {CorrelationId}. Configuration: Kind={Kind}, SKU={SkuName}, AccessTier={AccessTier}, MinTLS={MinimumTlsVersion}";
        public const string AccountCreationSuccessMessage = "Storage Account {StorageAccountName} creation initiated in {ElapsedMs}ms for CorrelationId: {CorrelationId}";
        public const string AccountCreationFailedMessage = "Failed to create Storage Account {StorageAccountName} for CorrelationId: {CorrelationId}. Error: {ErrorMessage}";
        public const string AccountConfiguredMessage = "Storage Account {StorageAccountName} configured successfully for CorrelationId: {CorrelationId}. Security: HTTPS Only, TLS 1.2+, No Public Blob Access";

        // Storage Account Keys messages
        public const string KeysRetrievalStartMessage = "Retrieving Storage Account keys for CorrelationId: {CorrelationId}";
        public const string KeysRetrievalSuccessMessage = "Storage Account keys retrieval initiated in {ElapsedMs}ms for CorrelationId: {CorrelationId}";
        public const string KeysRetrievalFailedMessage = "Failed to retrieve Storage Account keys for CorrelationId: {CorrelationId}. Error: {ErrorMessage}";
        public const string KeysConfiguredMessage = "Storage Account keys retrieval configured successfully for CorrelationId: {CorrelationId}";

        // Error messages
        public const string ResourceCreationFailedMessage = "Failed to create Azure Storage Account for environment '{0}' (CorrelationId: {1})";
    }

    /// <summary>
    /// Database service constants
    /// </summary>
    public static class Database
    {
        // Main creation logging messages
        public const string CreationStartMessage = "Starting PostgreSQL Flexible Server creation for environment: {Environment} with CorrelationId: {CorrelationId}. Version: {Version}, SKU: {SkuName}, Storage: {StorageSizeGb}GB";
        public const string GeneratedResourceNamesMessage = "Generated resource names for CorrelationId: {CorrelationId} - Server: {ServerName}, Database: {DatabaseName}";
        public const string CreationSuccessMessage = "Successfully created complete PostgreSQL infrastructure for environment: {Environment} in {TotalElapsedMs}ms with CorrelationId: {CorrelationId}. Server: {ServerName}, Database: {DatabaseName}, SKU: {SkuName}";
        public const string CreationCancelledMessage = "PostgreSQL infrastructure creation was cancelled for environment: {Environment} after {ElapsedMs}ms with CorrelationId: {CorrelationId}";
        public const string CreationFailedMessage = "Failed to create PostgreSQL infrastructure for environment: {Environment} after {ElapsedMs}ms with CorrelationId: {CorrelationId}. Error: {ErrorMessage}";
        public const string ResourceCreationFailedMessage = "Failed to create PostgreSQL infrastructure for environment '{0}' (CorrelationId: {1})";

        // Server operations
        public const string ServerCreationInitiatedMessage = "PostgreSQL Server {ServerName} creation initiated in {ElapsedMs}ms for CorrelationId: {CorrelationId}";
        public const string ServerCreationStartMessage = "Creating PostgreSQL Flexible Server: {ServerName} in location: {Location} for CorrelationId: {CorrelationId}. Configuration: Version={Version}, Tier={Tier}, AvailabilityZone={AvailabilityZone}";
        public const string ServerConfiguredMessage = "PostgreSQL Server {ServerName} configured successfully for CorrelationId: {CorrelationId}. Storage: {StorageSize}GB, Admin User: {AdminUser}";
        public const string ServerCreationFailedMessage = "Failed to create PostgreSQL Server {ServerName} for CorrelationId: {CorrelationId}. Error: {ErrorMessage}";

        // Database operations
        public const string DatabaseCreationInitiatedMessage = "Database {DatabaseName} creation initiated in {ElapsedMs}ms for CorrelationId: {CorrelationId}";
        public const string DatabaseCreationStartMessage = "Creating Database: {DatabaseName} on server for CorrelationId: {CorrelationId}. Charset: UTF8, Collation: en_US.utf8";
        public const string DatabaseConfiguredMessage = "Database {DatabaseName} configured successfully for CorrelationId: {CorrelationId}";
        public const string DatabaseCreationFailedMessage = "Failed to create Database {DatabaseName} for CorrelationId: {CorrelationId}. Error: {ErrorMessage}";
    }

    /// <summary>
    /// Cache service constants
    /// </summary>
    public static class Cache
    {
        // Main creation logging messages
        public const string CreationStartMessage = "Starting Azure Cache for Redis creation for environment: {Environment} with CorrelationId: {CorrelationId}. SKU: {SkuName}/{SkuFamily}, Capacity: {SkuCapacity}, NonSSL: {EnableNonSslPort}";
        public const string CreationSuccessMessage = "Successfully created complete Redis cache infrastructure for environment: {Environment} in {TotalElapsedMs}ms with CorrelationId: {CorrelationId}. Cache: {CacheName}, SKU: {SkuName}/{SkuFamily}, Capacity: {SkuCapacity}";
        public const string CreationCancelledMessage = "Redis cache creation was cancelled for environment: {Environment} after {ElapsedMs}ms with CorrelationId: {CorrelationId}";
        public const string CreationFailedMessage = "Failed to create Redis cache for environment: {Environment} after {ElapsedMs}ms with CorrelationId: {CorrelationId}. Error: {ErrorMessage}";
        public const string ResourceCreationFailedMessage = "Failed to create Azure Cache for Redis for environment '{0}' (CorrelationId: {1})";

        // Cache operations
        public const string CacheCreationInitiatedMessage = "Redis cache {CacheName} creation initiated in {ElapsedMs}ms for CorrelationId: {CorrelationId}";
        public const string CacheCreationStartMessage = "Creating Redis cache: {CacheName} in location: {Location} for CorrelationId: {CorrelationId}. Configuration: SKU={SkuName}/{SkuFamily}, Capacity={SkuCapacity}, NonSSL={EnableNonSslPort}";
        public const string CacheCreationFailedMessage = "Failed to create Redis cache {CacheName} for CorrelationId: {CorrelationId}. Error: {ErrorMessage}";

        // Keys operations
        public const string KeysRetrievalInitiatedMessage = "Redis keys retrieval initiated in {ElapsedMs}ms for CorrelationId: {CorrelationId}";
        public const string KeysRetrievalStartMessage = "Retrieving Redis keys for cache for CorrelationId: {CorrelationId}";
        public const string KeysRetrievalFailedMessage = "Failed to retrieve Redis keys for CorrelationId: {CorrelationId}. Error: {ErrorMessage}";
    }

    /// <summary>
    /// Key Vault service constants
    /// </summary>
    public static class KeyVault
    {
        // Logging messages
        public const string CreationStartMessage = "Creating Key Vault for environment: {Environment}";
        public const string CreationSuccessMessage = "Successfully created Key Vault: {KeyVaultName}";
        public const string CreationFailedMessage = "Failed to create Key Vault for environment: {Environment}";

        // Environment constants
        public const string ProductionEnvironment = "prod";
        public const string SecretsUserRoleId = "4633458b-17de-408a-b874-0445c86b69e6";
    }

    /// <summary>
    /// Test defaults for mock implementations
    /// </summary>
    public static class TestDefaults
    {
        public const string WorkspaceId = "default-workspace-id";
        public const string WorkspaceKey = "default-workspace-key";
        public const string AIKey = "default-ai-key";
        public const string AIConnection = "default-ai-connection";
        public const string RegistryServer = "mcr.microsoft.com";
        public const string ConnectionString = "Server=localhost;Database=Db;Integrated Security=true;";
        public const string ServerName = "localhost";
        public const string DatabaseName = "Application";
        public const string RedisConnection = "localhost:6379";
        public const string RedisHost = "localhost";
        public const string RedisName = "localhost-cache";
        public const string VnetId = "default-vnet";
        public const string SubnetId = "default-subnet";
        public const string AppGatewaySubnetId = "default-agw-subnet";
        public const string EventHubNamespace = "default-eventhub-namespace";
        public const string EventHubConnection = "Endpoint=sb://localhost/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=placeholder";
    }
}

