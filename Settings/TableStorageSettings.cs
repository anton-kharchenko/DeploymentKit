using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

public class TableStorageSettings
{
    /// <summary>
    /// Table names to create
    /// </summary>
    public List<string> TableNames { get; set; } = new() { "users", "sessions", "logs" };

    /// <summary>
    /// Enable table encryption
    /// </summary>
    public bool EnableEncryption { get; set; } = true;

    /// <summary>
    /// Enable CORS for table service
    /// </summary>
    public bool EnableCors { get; set; }

    /// <summary>
    /// CORS allowed origins
    /// </summary>
    public List<string> CorsAllowedOrigins { get; set; } = new() { "*" };

    /// <summary>
    /// CORS allowed methods
    /// </summary>
    public List<string> CorsAllowedMethods { get; set; } = new() { "GET", "POST", "PUT", "DELETE" };

    /// <summary>
    /// CORS allowed headers
    /// </summary>
    public List<string> CorsAllowedHeaders { get; set; } = new() { "*" };

    /// <summary>
    /// CORS exposed headers
    /// </summary>
    public List<string> CorsExposedHeaders { get; set; } = new() { "*" };

    /// <summary>
    /// CORS max age in seconds
    /// </summary>
    [Range(0, 2000000000, ErrorMessage = "CORS max age must be between 0 and 2000000000 seconds")]
    public int CorsMaxAgeInSeconds { get; set; } = 3600;

    /// <summary>
    /// Enable logging for table service
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Log retention days
    /// </summary>
    [Range(1, 365, ErrorMessage = "Log retention must be between 1 and 365 days")]
    public int LogRetentionDays { get; set; } = 7;

    /// <summary>
    /// Enable metrics for table service
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Metrics retention days
    /// </summary>
    [Range(1, 365, ErrorMessage = "Metrics retention must be between 1 and 365 days")]
    public int MetricsRetentionDays { get; set; } = 7;

    /// <summary>
    /// Enable hour metrics
    /// </summary>
    public bool EnableHourMetrics { get; set; } = true;

    /// <summary>
    /// Enable minute metrics
    /// </summary>
    public bool EnableMinuteMetrics { get; set; }
}


