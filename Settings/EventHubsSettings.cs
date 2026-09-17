using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

public class EventHubsSettings
{
    /// <summary>
    /// Event Hubs SKU name
    /// </summary>
    public EventHubsSkuType SkuName { get; set; } = EventHubsSkuType.Standard;

    /// <summary>
    /// Event Hubs SKU tier (alias for SkuName)
    /// </summary>
    public EventHubsSkuType SkuTier
    {
        get => SkuName;
        set => SkuName = value;
    }

    /// <summary>
    /// Event Hubs namespace capacity
    /// </summary>
    [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20")]
    public int Capacity { get; set; } = InfrastructureConstants.EventHubs.DefaultCapacity;

    /// <summary>
    /// Message retention period in days
    /// </summary>
    [Range(1, 7, ErrorMessage = "Message retention must be between 1 and 7 days")]
    public int MessageRetentionInDays { get; set; } = InfrastructureConstants.EventHubs.DefaultMessageRetentionDays;

    /// <summary>
    /// Message retention period in days (alias for MessageRetentionInDays)
    /// </summary>
    public int MessageRetentionDays
    {
        get => MessageRetentionInDays;
        set => MessageRetentionInDays = value;
    }

    /// <summary>
    /// Number of partitions for event hubs
    /// </summary>
    [Range(2, 32, ErrorMessage = "Partition count must be between 2 and 32")]
    public int PartitionCount { get; set; } = InfrastructureConstants.EventHubs.DefaultPartitionCount;

    /// <summary>
    /// SKU capacity (alias for Capacity)
    /// </summary>
    public int SkuCapacity
    {
        get => Capacity;
        set => Capacity = value;
    }

    /// <summary>
    /// Enable auto-inflate for throughput units
    /// </summary>
    public bool EnableAutoInflate { get; set; }

    /// <summary>
    /// Maximum throughput units when auto-inflate is enabled
    /// </summary>
    [Range(1, 20, ErrorMessage = "Maximum throughput units must be between 1 and 20")]
    public int MaximumThroughputUnits { get; set; } = 20;

    /// <summary>
    /// Enable Kafka for Event Hubs
    /// </summary>
    public bool EnableKafka { get; set; } = true;

    /// <summary>
    /// Enable zone redundancy
    /// </summary>
    public bool EnableZoneRedundancy { get; set; }

    /// <summary>
    /// Enable dedicated cluster
    /// </summary>
    public bool EnableDedicatedCluster { get; set; }

    /// <summary>
    /// Enable capture for event data
    /// </summary>
    public bool EnableCapture { get; set; }

    /// <summary>
    /// Capture interval in seconds
    /// </summary>
    [Range(60, 900, ErrorMessage = "Capture interval must be between 60 and 900 seconds")]
    public int CaptureIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// Capture size limit in bytes
    /// </summary>
    [Range(10485760, 524288000, ErrorMessage = "Capture size limit must be between 10MB and 500MB")]
    public int CaptureSizeLimitBytes { get; set; } = 314572800; // 300MB

    /// <summary>
    /// Whether Event Hubs is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    // String properties for backward compatibility and Pulumi integration
    public string SkuNameString
    {
        get => SkuName.ToStringValue();
        set { }
    }

    internal string SkuTierString => SkuTier.ToStringValue();
}



