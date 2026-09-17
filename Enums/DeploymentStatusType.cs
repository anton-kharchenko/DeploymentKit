using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Represents the current status of a deployment.
/// </summary>
public enum DeploymentStatusType
{
    /// <summary>
    /// The deployment has not been started or configured.
    /// </summary>
    [Description("NotDeployed")]
    NotDeployed,

    /// <summary>
    /// The deployment is currently in progress.
    /// </summary>
    [Description("InProgress")]
    InProgress,

    /// <summary>
    /// The deployment is stable and running normally.
    /// </summary>
    [Description("Stable")]
    Stable,

    /// <summary>
    /// The deployment is currently being processed.
    /// </summary>
    [Description("Deploying")]
    Deploying,

    /// <summary>
    /// The deployment requires additional dependencies to be configured.
    /// </summary>
    [Description("RequiresDependencies")]
    RequiresDependencies,

    /// <summary>
    /// The deployment is currently switching traffic between slots.
    /// </summary>
    [Description("Switching")]
    Switching
}

