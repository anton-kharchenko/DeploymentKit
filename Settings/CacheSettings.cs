using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

public class CacheSettings
{
    /// <summary>
    /// Use Azure Managed Redis instead of Azure Cache for Redis (default: true)
    /// </summary>
    public bool UseAzureManagedRedis { get; set; } = true;

    #region Azure Cache for Redis Settings (Legacy - deprecated)

    /// <summary>
    /// Redis SKU name (Azure Cache for Redis only - deprecated)
    /// </summary>
    public RedisSkuType SkuName { get; set; } = RedisSkuType.Standard;

    /// <summary>
    /// Redis SKU family (Azure Cache for Redis only - deprecated)
    /// </summary>
    public RedisSkuFamilyType SkuFamily { get; set; } = RedisSkuFamilyType.C;

    /// <summary>
    /// Redis max memory policy (Azure Cache for Redis only - deprecated)
    /// </summary>
    public RedisMaxMemoryPolicyType MaxMemoryPolicy { get; set; } = RedisMaxMemoryPolicyType.VolatileLru;

    /// <summary>
    /// SKU capacity (Azure Cache for Redis only - deprecated)
    /// </summary>
    [Range(0, 6, ErrorMessage = "SKU capacity must be between 0 and 6")]
    public int SkuCapacity { get; set; } = InfrastructureConstants.Cache.DefaultSkuCapacity;

    /// <summary>
    /// Enable non-SSL port (Azure Cache for Redis only - deprecated, not supported in Azure Managed Redis)
    /// </summary>
    public bool EnableNonSslPort { get; set; }

    /// <summary>
    /// Minimum TLS version (Azure Cache for Redis only - deprecated)
    /// </summary>
    [RegularExpression(@"^(1\.0|1\.1|1\.2)$", ErrorMessage = "Minimum TLS version must be 1.0, 1.1, or 1.2")]
    public string MinimumTlsVersion { get; set; } = InfrastructureConstants.Cache.DefaultMinimumTlsVersion;

    /// <summary>
    /// Enable Redis AUTH (Azure Cache for Redis only - deprecated)
    /// </summary>
    public bool EnableAuth { get; set; } = true;

    /// <summary>
    /// Redis configuration (Azure Cache for Redis only - deprecated)
    /// </summary>
    public Dictionary<string, string> RedisConfiguration { get; set; } = new();

    /// <summary>
    /// Enable Redis data persistence (Azure Cache for Redis only - deprecated)
    /// </summary>
    public bool EnableDataPersistence { get; set; }

    /// <summary>
    /// Data persistence backup frequency (Azure Cache for Redis only - deprecated)
    /// </summary>
    [Range(15, 1440, ErrorMessage = "Backup frequency must be between 15 and 1440 minutes")]
    public int BackupFrequencyMinutes { get; set; } = 60;

    /// <summary>
    /// Maximum memory reserved (Azure Cache for Redis only - deprecated)
    /// </summary>
    [Range(0, 50, ErrorMessage = "Maximum memory reserved must be between 0 and 50 percent")]
    public int MaxMemoryReserved { get; set; } = 10;

    /// <summary>
    /// Maximum memory delta (Azure Cache for Redis only - deprecated)
    /// </summary>
    [Range(0, 50, ErrorMessage = "Maximum memory delta must be between 0 and 50 percent")]
    public int MaxMemoryDelta { get; set; } = 10;

    #endregion

    #region Azure Managed Redis Settings

    /// <summary>
    /// Azure Managed Redis SKU type
    /// </summary>
    public AzureManagedRedisSkuType ManagedRedisSku { get; set; } = AzureManagedRedisSkuType.Enterprise_E10;

    /// <summary>
    /// Azure Managed Redis SKU capacity (default: 2)
    /// For Enterprise SKUs: 2, 4, 6, ...
    /// For Flash SKUs: 3, 9, 15, ...
    /// </summary>
    [Range(2, 100, ErrorMessage = "SKU capacity must be between 2 and 100")]
    public int ManagedRedisCapacity { get; set; } = 2;

    /// <summary>
    /// Clustering policy for Azure Managed Redis (default: OSSCluster)
    /// </summary>
    public ClusteringPolicyType ClusteringPolicy { get; set; } = ClusteringPolicyType.OSSCluster;

    /// <summary>
    /// Client protocol for Azure Managed Redis (default: Encrypted)
    /// </summary>
    public ClientProtocolType ClientProtocol { get; set; } = ClientProtocolType.Encrypted;

    /// <summary>
    /// Eviction policy for Azure Managed Redis (default: VolatileLRU)
    /// </summary>
    public ManagedRedisEvictionPolicyType ManagedRedisEvictionPolicy { get; set; } = ManagedRedisEvictionPolicyType.VolatileLRU;

    /// <summary>
    /// TCP port for Azure Managed Redis (default: 10000)
    /// </summary>
    [Range(1024, 65535, ErrorMessage = "Port must be between 1024 and 65535")]
    public int ManagedRedisPort { get; set; } = 10000;

    /// <summary>
    /// Enable geo-replication for Azure Managed Redis
    /// </summary>
    public bool EnableGeoReplication { get; set; }

    /// <summary>
    /// Linked database IDs for geo-replication (up to 5)
    /// </summary>
    public List<string> LinkedDatabaseIds { get; set; } = new();

    /// <summary>
    /// Linked database group nickname for geo-replication
    /// </summary>
    public string? LinkedDatabaseGroupNickname { get; set; }

    #endregion

    /// <summary>
    /// Whether the cache is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    // String properties for backward compatibility and Pulumi integration
    internal string SkuNameString { get; set; } = string.Empty;
    internal string SkuFamilyString => SkuFamily.ToStringValue();
    internal string MaxMemoryPolicyString => MaxMemoryPolicy.ToStringValue();
    internal string ManagedRedisSkuString => ManagedRedisSku.ToStringValue();
    internal string ClusteringPolicyString => ClusteringPolicy.ToStringValue();
    internal string ClientProtocolString => ClientProtocol.ToStringValue();
    internal string ManagedRedisEvictionPolicyString => ManagedRedisEvictionPolicy.ToStringValue();
}




