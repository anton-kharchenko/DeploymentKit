using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available access tiers for Azure Blob Storage
/// </summary>
public enum BlobAccessTierType
{
    /// <summary>
    /// Hot tier - Optimized for frequent access
    /// </summary>
    [Description("Hot")]
    Hot,

    /// <summary>
    /// Cool tier - Optimized for infrequent access (30+ days)
    /// </summary>
    [Description("Cool")]
    Cool,

    /// <summary>
    /// Archive tier - Optimized for rarely accessed data (180+ days)
    /// </summary>
    [Description("Archive")]
    Archive
}
