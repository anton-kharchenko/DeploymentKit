using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available SKU types for Azure Redis Cache
/// </summary>
public enum RedisSkuType
{
    /// <summary>
    /// Basic tier - Single node, no SLA
    /// </summary>
    [Description("Basic")]
    Basic,

    /// <summary>
    /// Standard tier - Two-node primary/replica, 99.9% SLA
    /// </summary>
    [Description("Standard")]
    Standard,

    /// <summary>
    /// Premium tier - Two-node primary/replica with clustering, persistence, and VNet support, 99.9% SLA
    /// </summary>
    [Description("Premium")]
    Premium
}
