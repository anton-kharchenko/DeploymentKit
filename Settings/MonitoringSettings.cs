using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

public class MonitoringSettings
{
    /// <summary>
    /// Application Insights type
    /// </summary>
    public ApplicationInsightsType ApplicationInsightsType { get; set; } = ApplicationInsightsType.Web;

    /// <summary>
    /// Log retention period in days
    /// </summary>
    [Range(30, 730, ErrorMessage = "Log retention days must be between 30 and 730")]
    public int LogRetentionDays { get; set; } = InfrastructureConstants.Monitoring.DefaultLogRetentionDays;

    /// <summary>
    /// Enable detailed metrics collection
    /// </summary>
    public bool EnableDetailedMetrics { get; set; } = true;

    /// <summary>
    /// Enable live metrics stream
    /// </summary>
    public bool EnableLiveMetrics { get; set; } = true;

    /// <summary>
    /// Enable sampling for Application Insights
    /// </summary>
    public bool EnableSampling { get; set; } = true;

    /// <summary>
    /// Sampling percentage (0-100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Sampling percentage must be between 0 and 100")]
    public double SamplingPercentage { get; set; } = 100.0;

    /// <summary>
    /// Enable adaptive sampling
    /// </summary>
    public bool EnableAdaptiveSampling { get; set; }

    /// <summary>
    /// Daily data cap in GB
    /// </summary>
    [Range(0.1, 1000, ErrorMessage = "Daily data cap must be between 0.1 and 1000 GB")]
    public double DailyDataCapGb { get; set; } = 100.0;

    /// <summary>
    /// Enable daily data cap warning
    /// </summary>
    public bool EnableDailyDataCapWarning { get; set; } = true;

    /// <summary>
    /// Data cap warning threshold percentage
    /// </summary>
    [Range(1, 100, ErrorMessage = "Warning threshold must be between 1 and 100 percent")]
    public int WarningThresholdPercentage { get; set; } = 90;

    /// <summary>
    /// Enable request tracking
    /// </summary>
    public bool EnableRequestTracking { get; set; } = true;

    /// <summary>
    /// Enable dependency tracking
    /// </summary>
    public bool EnableDependencyTracking { get; set; } = true;

    /// <summary>
    /// Enable exception tracking
    /// </summary>
    public bool EnableExceptionTracking { get; set; } = true;

    /// <summary>
    /// Enable performance counter collection
    /// </summary>
    public bool EnablePerformanceCounters { get; set; } = true;

    /// <summary>
    /// Whether monitoring is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    // String properties for backward compatibility and Pulumi integration
    internal string ApplicationInsightsTypeString => ApplicationInsightsType.ToStringValue();
}

