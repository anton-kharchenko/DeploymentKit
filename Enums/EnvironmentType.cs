using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Represents the environment types for deployment.
/// </summary>
public enum EnvironmentType
{
    /// <summary>
    /// Development environment.
    /// </summary>
    [Description("development")]
    Development,

    /// <summary>
    /// Production environment.
    /// </summary>
    [Description("production")]
    Production
}

