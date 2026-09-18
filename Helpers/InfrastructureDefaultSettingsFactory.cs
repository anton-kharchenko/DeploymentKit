using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using DeploymentKit.Settings;

namespace DeploymentKit.Helpers;

/// <summary>
/// Factory for creating default infrastructure settings.
/// </summary>
public static class InfrastructureDefaultSettingsFactory
{
    /// <summary>
    /// Creates default Key Vault settings.
    /// </summary>
    /// <returns>Default Key Vault settings.</returns>
    public static KeyVaultSettings GetDefaultKeyVaultSettings()
    {
        const KeyVaultSkuType skuName = KeyVaultSkuType.Standard;
        return new KeyVaultSettings
        {
            SkuName = skuName,
            SkuNameString = skuName.ToStringValue(),
            EnableSoftDelete = true,
            SoftDeleteRetentionDays = 90,
            EnablePurgeProtection = true,
            EnableRbacAuthorization = true
        };
    }

    /// <summary>
    /// Creates default Cache settings.
    /// </summary>
    /// <returns>Default Cache settings.</returns>
    public static CacheSettings GetDefaultCacheSettings()
    {
        const RedisSkuType skuName = RedisSkuType.Basic;
        return new CacheSettings
        {
            SkuName = skuName,
            SkuNameString = skuName.ToStringValue(),
            SkuFamily = RedisSkuFamilyType.C,
            SkuCapacity = 0,
            EnableNonSslPort = false
        };
    }

    /// <summary>
    /// Creates default Event Hubs settings.
    /// </summary>
    /// <returns>Default Event Hubs settings.</returns>
    public static EventHubsSettings GetDefaultEventHubsSettings()
    {
        return new EventHubsSettings
        {
            SkuName = EventHubsSkuType.Standard,
            Capacity = 1,
            EnableAutoInflate = false,
            MaximumThroughputUnits = 1,
            MessageRetentionInDays = 1,
            PartitionCount = 2
        };
    }

    /// <summary>
    /// Creates default Monitoring settings.
    /// </summary>
    /// <returns>Default Monitoring settings.</returns>
    public static MonitoringSettings GetDefaultMonitoringSettings()
    {
        return new MonitoringSettings
        {
            ApplicationInsightsType = ApplicationInsightsType.Web,
            LogRetentionDays = 90,
            EnableDetailedMetrics = true,
            EnableLiveMetrics = true
        };
    }

    /// <summary>
    /// Creates default Database settings.
    /// </summary>
    /// <returns>Default Database settings.</returns>
    public static DatabaseSettings GetDefaultDatabaseSettings()
    {
        return new DatabaseSettings
        {
            SkuNameString = InfrastructureConstants.Defaults.DatabaseSkuName,
            StorageSizeGb = InfrastructureConstants.Defaults.DatabaseStorageSizeGb,
            BackupRetentionDays = 7,
            EnableGeoRedundantBackup = false,
            EnableHighAvailability = false,
            VersionString = InfrastructureConstants.Defaults.DatabaseVersion,
            AdminUser = InfrastructureConstants.Defaults.DatabaseAdminUser,
            AdminUsername = InfrastructureConstants.Defaults.DatabaseAdminUser,
            Password = InfrastructureConstants.Defaults.LocalDevelopmentPassword,
            AdminPassword = InfrastructureConstants.Defaults.LocalDevelopmentPassword,
            AvailabilityZone = InfrastructureConstants.Defaults.DatabaseAvailabilityZone
        };
    }

    /// <summary>
    /// Creates default Storage settings.
    /// </summary>
    /// <returns>Default Storage settings.</returns>
    public static StorageSettings GetDefaultStorageSettings()
    {
        return new StorageSettings
        {
            AccountTier = StorageAccountTierType.Standard,
            ReplicationType = StorageReplicationType.StandardLrs,
            AllowBlobPublicAccess = false,
            EnableHttpsTrafficOnly = true,
            MinimumTlsVersion = TlsVersionType.Tls12,
            EnableVersioning = true
        };
    }

    /// <summary>
    /// Creates default Blob Storage settings.
    /// </summary>
    /// <returns>Default Blob Storage settings.</returns>
    public static BlobStorageSettings GetDefaultBlobStorageSettings()
    {
        return new BlobStorageSettings
        {
            AccessTier = BlobAccessTierType.Hot,
            EnableVersioning = true,
            EnableChangeFeed = false,
            EnableSoftDelete = true,
            SoftDeleteRetentionDays = 7,
            ContainerNames = new List<string> { "default" },
            AllowPublicAccess = false,
            DefaultContentType = "application/octet-stream",
            EnableLifecycleManagement = false,
            CoolTierTransitionDays = 30,
            ArchiveTierTransitionDays = 90,
            DeleteAfterDays = 365
        };
    }

    /// <summary>
    /// Creates default Cosmos DB settings.
    /// </summary>
    /// <returns>Default Cosmos DB settings.</returns>
    public static CosmosDbSettings GetDefaultCosmosDbSettings()
    {
        return new CosmosDbSettings
        {
            ConsistencyLevel = CosmosDbConsistencyLevelType.Session,
            DatabaseName = "DefaultDatabase",
            Containers = new Dictionary<string, string>
            {
                { "DefaultContainer", "/id" }
            },
            DefaultThroughput = 400,
            EnableAutomaticFailover = false,
            EnableMultipleWriteLocations = false,
            BackupIntervalMinutes = 240,
            BackupRetentionHours = 8,
            EnableAnalyticalStorage = false,
            MaxStalenessPrefix = 100000,
            MaxIntervalInSeconds = 300,
            Locations = new List<string>(),
            EnableFreeTier = false
        };
    }

    /// <summary>
    /// Creates default Table Storage settings.
    /// </summary>
    /// <returns>Default Table Storage settings.</returns>
    public static TableStorageSettings GetDefaultTableStorageSettings()
    {
        return new TableStorageSettings
        {
            TableNames = new List<string> { "DefaultTable" },
            EnableEncryption = true,
            EnableCors = false,
            EnableLogging = false,
            EnableMetrics = false,
            EnableHourMetrics = false,
            EnableMinuteMetrics = false
        };
    }

    /// <summary>
    /// Creates default Container settings.
    /// </summary>
    /// <returns>Default Container settings.</returns>
    public static ContainerSettings GetDefaultContainerSettings()
    {
        return new ContainerSettings
        {
            ApiImageTag = "latest",
            JobsImageTag = "latest",
            AutoScaling = new AutoScalingSettings
            {
                MinReplicas = 1,
                MaxReplicas = 10,
                CpuThreshold = 70,
                MemoryThreshold = 80
            },
            MinReplicas = 1,
            MaxReplicas = 10,
            EnableDapr = false,
            CpuLimit = 0.5,
            MemoryLimit = 1.0,
            ApiCpuLimit = 1.0,
            ApiMemoryLimit = 2.0,
            IngressSettings = new IngressSettings
            {
                External = true,
                TargetPort = 8080,
                AllowInsecure = false,
                Transport = "Http"
            }
        };
    }

    /// <summary>
    /// Creates default Network settings.
    /// </summary>
    /// <returns>Default Network settings.</returns>
    public static NetworkSettings GetDefaultNetworkSettings()
    {
        return new NetworkSettings
        {
            VNetAddressSpace = InfrastructureConstants.Network.DefaultVNetAddressSpace,
            ContainerAppsSubnet = InfrastructureConstants.Network.DefaultContainerAppsSubnet,
            PrivateEndpointsSubnet = InfrastructureConstants.Network.DefaultPrivateEndpointsSubnet,
            EnableNetworkSecurityGroups = true,
            EnableDdosProtection = false,
            IsInternalEnvironment = false
        };
    }

    /// <summary>
    /// Creates default Application Gateway settings.
    /// </summary>
    /// <returns>Default Application Gateway settings.</returns>
    public static ApplicationGatewaySettings GetDefaultApplicationGatewaySettings()
    {
        return new ApplicationGatewaySettings
        {
            Enabled = true,
            EnableWaf = true,
            WafMode = "Prevention",
            MinCapacity = 0,
            MaxCapacity = 10
        };
    }

    /// <summary>
    /// Creates default Green-Blue deployment settings.
    /// </summary>
    /// <returns>Default Green-Blue deployment settings.</returns>
    public static GreenBlueDeploymentSettings GetDefaultGreenBlueSettings()
    {
        return new GreenBlueDeploymentSettings
        {
            EnableGreenBlueDeployment = true,
            TrafficSplitPercentage = 100,
            AutoSwitchSlots = false,
            HealthCheckTimeoutSeconds = 30
        };
    }
}

