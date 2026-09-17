using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Represents the types of DNS zone configurations.
/// </summary>
public enum DnsZoneType
{
    /// <summary>
    /// DNS zone for database resources.
    /// </summary>
    [Description("Database")]
    Database
}

