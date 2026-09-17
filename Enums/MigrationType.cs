using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available database migration providers
/// </summary>
public enum MigrationType
{
    /// <summary>
    /// Entity Framework Core migrations
    /// </summary>
    [Description("EfCore")]
    EfCore,

    /// <summary>
    /// FluentMigrator migrations
    /// </summary>
    [Description("FluentMigrator")]
    FluentMigrator,

    /// <summary>
    /// Custom SQL scripts
    /// </summary>
    [Description("SqlScript")]
    SqlScript
}

