using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Represents the network bypass options for Azure resources.
/// </summary>
public enum NetworkBypassType
{
    /// <summary>
    /// No bypass allowed.
    /// </summary>
    [Description("None")]
    None,

    /// <summary>
    /// Allow Azure services to bypass network restrictions.
    /// </summary>
    [Description("AzureServices")]
    AzureServices
}
