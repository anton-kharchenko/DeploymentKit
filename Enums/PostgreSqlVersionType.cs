using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available PostgreSQL versions for Azure PostgreSQL Flexible Server
/// </summary>
public enum PostgreSqlVersionType
{
    /// <summary>
    /// PostgreSQL version 11
    /// </summary>
    [Description("11")]
    Version11,

    /// <summary>
    /// PostgreSQL version 12
    /// </summary>
    [Description("12")]
    Version12,

    /// <summary>
    /// PostgreSQL version 13
    /// </summary>
    [Description("13")]
    Version13,

    /// <summary>
    /// PostgreSQL version 14
    /// </summary>
    [Description("14")]
    Version14,

    /// <summary>
    /// PostgreSQL version 15
    /// </summary>
    [Description("15")]
    Version15,

    /// <summary>
    /// PostgreSQL version 16 (Latest)
    /// </summary>
    [Description("16")]
    Version16
}
