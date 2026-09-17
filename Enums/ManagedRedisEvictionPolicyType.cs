using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the eviction policy types for Azure Managed Redis
/// </summary>
public enum ManagedRedisEvictionPolicyType
{
    /// <summary>
    /// No eviction - Returns errors when memory limit is reached
    /// </summary>
    [Description("NoEviction")]
    NoEviction,

    /// <summary>
    /// All keys LFU - Evicts least frequently used keys from all keys
    /// </summary>
    [Description("AllKeysLFU")]
    AllKeysLFU,

    /// <summary>
    /// All keys LRU - Evicts least recently used keys from all keys
    /// </summary>
    [Description("AllKeysLRU")]
    AllKeysLRU,

    /// <summary>
    /// All keys random - Evicts random keys from all keys
    /// </summary>
    [Description("AllKeysRandom")]
    AllKeysRandom,

    /// <summary>
    /// Volatile LFU - Evicts least frequently used keys from keys with expiration set
    /// </summary>
    [Description("VolatileLFU")]
    VolatileLFU,

    /// <summary>
    /// Volatile LRU - Evicts least recently used keys from keys with expiration set (default)
    /// </summary>
    [Description("VolatileLRU")]
    VolatileLRU,

    /// <summary>
    /// Volatile random - Evicts random keys from keys with expiration set
    /// </summary>
    [Description("VolatileRandom")]
    VolatileRandom,

    /// <summary>
    /// Volatile TTL - Evicts keys with expiration set, prioritizing shorter TTL
    /// </summary>
    [Description("VolatileTTL")]
    VolatileTTL
}

