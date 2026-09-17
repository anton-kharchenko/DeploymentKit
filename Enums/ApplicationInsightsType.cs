using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available Application Insights types for Azure monitoring
/// </summary>
public enum ApplicationInsightsType
{
    /// <summary>
    /// Web application type - For web applications and services
    /// </summary>
    [Description("web")]
    Web,

    /// <summary>
    /// Other application type - For desktop applications, services, and other types
    /// </summary>
    [Description("other")]
    Other
}
