using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available SKU types for Azure Event Hubs
/// </summary>
public enum EventHubsSkuType
{
    /// <summary>
    /// Basic tier - Up to 20 throughput units, 1 day retention
    /// </summary>
    [Description("Basic")]
    Basic,

    /// <summary>
    /// Standard tier - Up to 20 throughput units, 7 days retention, consumer groups
    /// </summary>
    [Description("Standard")]
    Standard,

    /// <summary>
    /// Premium tier - Dedicated capacity, up to 90 days retention, VNet integration
    /// </summary>
    [Description("Premium")]
    Premium
}
