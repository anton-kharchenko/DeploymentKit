using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Specifies the level of validation to perform during infrastructure deployment
/// </summary>
public enum ValidationMode
{
    /// <summary>
    /// Full validation - Performs all pre-deployment checks including Azure connectivity, resource existence, and drift detection
    /// Use for production deployments and existing infrastructure updates
    /// </summary>
    [Description("Full")]
    Full,

    /// <summary>
    /// Basic validation - Only validates settings and naming conventions without Azure connectivity checks
    /// Use for initial deployments or when Azure resources don't exist yet
    /// </summary>
    [Description("Basic")]
    Basic,

    /// <summary>
    /// Minimal validation - Only validates required fields and basic settings
    /// Use for development and testing scenarios
    /// </summary>
    [Description("Minimal")]
    Minimal,

    /// <summary>
    /// Skip validation - Bypasses all validation checks (not recommended for production)
    /// Use only when you're certain the configuration is correct
    /// </summary>
    [Description("Skip")]
    Skip
}


