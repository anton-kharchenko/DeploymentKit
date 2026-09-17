using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Severity levels for drift issues
/// </summary>
public enum DriftSeverityType
{
    /// <summary>
    /// Low severity - cosmetic or non-functional drift
    /// </summary>
    [Description("Low")]
    Low,

    /// <summary>
    /// Medium severity - functional drift that may impact operations
    /// </summary>
    [Description("Medium")]
    Medium,

    /// <summary>
    /// High severity - critical drift that requires immediate attention
    /// </summary>
    [Description("High")]
    High,

    /// <summary>
    /// Critical severity - drift that poses security or availability risks
    /// </summary>
    [Description("Critical")]
    Critical
}

