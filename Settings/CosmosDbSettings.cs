using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

public class CosmosDbSettings
{
    private CosmosDbConsistencyLevelType _consistencyLevel = CosmosDbConsistencyLevelType.Session;
    private string _consistencyLevelString = string.Empty;

    /// <summary>
    /// Consistency level for Cosmos DB
    /// </summary>
    public CosmosDbConsistencyLevelType ConsistencyLevel
    {
        get => _consistencyLevel;
        set
        {
            _consistencyLevel = value;
            _consistencyLevelString = value.ToStringValue();
        }
    }

    /// <summary>
    /// String representation of consistency level
    /// </summary>
    public string ConsistencyLevelString => string.IsNullOrEmpty(_consistencyLevelString) ? _consistencyLevel.ToStringValue() : _consistencyLevelString;

    /// <summary>
    /// Database name
    /// </summary>
    [Required]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Database name must be between 1 and 255 characters")]
    public string DatabaseName { get; set; } = "app";

    /// <summary>
    /// Container names and their partition keys
    /// </summary>
    public Dictionary<string, string> Containers { get; set; } = new()
    {
        { "users", "/userId" },
        { "trips", "/tripId" },
        { "bookings", "/bookingId" }
    };

    /// <summary>
    /// Default throughput for containers (RU/s)
    /// </summary>
    [Range(400, 1000000, ErrorMessage = "Throughput must be between 400 and 1,000,000 RU/s")]
    public int DefaultThroughput { get; set; } = 400;

    /// <summary>
    /// Enable automatic failover
    /// </summary>
    public bool EnableAutomaticFailover { get; set; } = true;

    /// <summary>
    /// Enable multiple write locations
    /// </summary>
    public bool EnableMultipleWriteLocations { get; set; }

    /// <summary>
    /// Backup policy interval in minutes
    /// </summary>
    [Range(60, 1440, ErrorMessage = "Backup interval must be between 60 and 1440 minutes")]
    public int BackupIntervalMinutes { get; set; } = 240;

    /// <summary>
    /// Backup retention hours
    /// </summary>
    [Range(8, 720, ErrorMessage = "Backup retention must be between 8 and 720 hours")]
    public int BackupRetentionHours { get; set; } = 168;

    /// <summary>
    /// Enable analytical storage
    /// </summary>
    public bool EnableAnalyticalStorage { get; set; }

    /// <summary>
    /// Max staleness prefix for bounded staleness consistency
    /// </summary>
    [Range(10, 2147483647, ErrorMessage = "Max staleness prefix must be at least 10")]
    public int MaxStalenessPrefix { get; set; } = 100;

    /// <summary>
    /// Max interval in seconds for bounded staleness consistency
    /// </summary>
    [Range(5, 86400, ErrorMessage = "Max interval must be between 5 and 86400 seconds")]
    public int MaxIntervalInSeconds { get; set; } = 300;

    /// <summary>
    /// Locations for geo-replication
    /// </summary>
    public List<string> Locations { get; set; } = new() { "East US" };

    /// <summary>
    /// Enable free tier
    /// </summary>
    public bool EnableFreeTier { get; set; }
}

