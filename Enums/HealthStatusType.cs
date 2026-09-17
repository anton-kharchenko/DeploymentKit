using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Represents the health status of a service or component.
/// </summary>
public enum HealthStatusType
{
    /// <summary>
    /// The service is healthy and functioning normally.
    /// </summary>
    [Description("Healthy")]
    Healthy,

    /// <summary>
    /// The service is unhealthy or experiencing issues.
    /// </summary>
    [Description("Unhealthy")]
    Unhealthy
}

