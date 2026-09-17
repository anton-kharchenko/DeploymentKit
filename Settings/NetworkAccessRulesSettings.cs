using DeploymentKit.Enums;
using DeploymentKit.Extensions;

namespace DeploymentKit.Settings;

public class NetworkAccessRulesSettings
{
    /// <summary>
    /// Default action for network access rules
    /// </summary>
    public NetworkAccessActionType DefaultAction { get; set; } = NetworkAccessActionType.Deny;

    /// <summary>
    /// Bypass settings for network access rules
    /// </summary>
    public NetworkBypassType Bypass { get; set; } = NetworkBypassType.AzureServices;

    /// <summary>
    /// List of allowed IP ranges in CIDR format
    /// </summary>
    public List<string> AllowedIpRanges { get; set; } = [];

    /// <summary>
    /// List of allowed subnet IDs
    /// </summary>
    public List<string> AllowedSubnetIds { get; set; } = [];

    // String properties for backward compatibility and Pulumi integration
    internal string DefaultActionString => DefaultAction.ToStringValue();
    internal string BypassString => Bypass.ToStringValue();
}

