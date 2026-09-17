using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available replication types for Azure Storage Account
/// </summary>
public enum StorageReplicationType
{
    /// <summary>
    /// Locally redundant storage - 3 copies within a single data center
    /// </summary>
    [Description("Standard_LRS")]
    StandardLrs,

    /// <summary>
    /// Geo-redundant storage - 6 copies across two regions
    /// </summary>
    [Description("Standard_GRS")]
    StandardGrs,

    /// <summary>
    /// Read-access geo-redundant storage - 6 copies with read access to secondary region
    /// </summary>
    [Description("Standard_RAGRS")]
    StandardRagrs,

    /// <summary>
    /// Zone-redundant storage - 3 copies across availability zones
    /// </summary>
    [Description("Standard_ZRS")]
    StandardZrs,

    /// <summary>
    /// Premium locally redundant storage - High-performance with 3 copies within a single data center
    /// </summary>
    [Description("Premium_LRS")]
    PremiumLrs,

    /// <summary>
    /// Geo-zone-redundant storage - Combines ZRS and GRS
    /// </summary>
    [Description("Standard_GZRS")]
    StandardGzrs,

    /// <summary>
    /// Read-access geo-zone-redundant storage - Combines ZRS and RAGRS
    /// </summary>
    [Description("Standard_RAGZRS")]
    StandardRagzrs
}
