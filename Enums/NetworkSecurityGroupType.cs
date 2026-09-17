using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Represents the types of network security group configurations.
/// </summary>
public enum NetworkSecurityGroupType
{
    /// <summary>
    /// NSG for container applications.
    /// </summary>
    [Description("containerapp")]
    ContainerApp,

    /// <summary>
    /// NSG for database resources.
    /// </summary>
    [Description("database")]
    Database
}

