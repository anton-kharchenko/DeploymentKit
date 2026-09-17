using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available max memory policy types for Azure Redis Cache
/// </summary>
public enum RedisMaxMemoryPolicyType
{
    /// <summary>
    /// No eviction - Returns errors when memory limit is reached
    /// </summary>
    [Description("noeviction")]
    NoEviction,

    /// <summary>
    /// All keys LRU - Evicts least recently used keys from all keys
    /// </summary>
    [Description("allkeys-lru")]
    AllKeysLru,

    /// <summary>
    /// Volatile LRU - Evicts least recently used keys from keys with expiration set
    /// </summary>
    [Description("volatile-lru")]
    VolatileLru,

    /// <summary>
    /// All keys random - Evicts random keys from all keys
    /// </summary>
    [Description("allkeys-random")]
    AllKeysRandom,

    /// <summary>
    /// Volatile random - Evicts random keys from keys with expiration set
    /// </summary>
    [Description("volatile-random")]
    VolatileRandom,

    /// <summary>
    /// Volatile TTL - Evicts keys with expiration set, prioritizing shorter TTL
    /// </summary>
    [Description("volatile-ttl")]
    VolatileTtl
}
