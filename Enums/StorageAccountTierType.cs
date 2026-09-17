using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available tier types for Azure Storage Account
/// </summary>
public enum StorageAccountTierType
{
    /// <summary>
    /// Standard tier - General-purpose storage with magnetic drives
    /// </summary>
    [Description("Standard")]
    Standard,

    /// <summary>
    /// Premium tier - High-performance storage with SSD drives
    /// </summary>
    [Description("Premium")]
    Premium
}
