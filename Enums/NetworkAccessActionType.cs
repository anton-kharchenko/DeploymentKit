using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Represents the default network access action types for Azure resources.
/// </summary>
public enum NetworkAccessActionType
{
    /// <summary>
    /// Allow network access by default.
    /// </summary>
    [Description("Allow")]
    Allow,

    /// <summary>
    /// Deny network access by default.
    /// </summary>
    [Description("Deny")]
    Deny
}
