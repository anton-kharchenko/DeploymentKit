using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available SKU family types for Azure Redis Cache
/// </summary>
public enum RedisSkuFamilyType
{
    /// <summary>
    /// C family - Used for Basic and Standard tiers
    /// </summary>
    [Description("C")]
    C,

    /// <summary>
    /// P family - Used for Premium tier
    /// </summary>
    [Description("P")]
    P
}
