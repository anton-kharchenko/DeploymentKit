using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// Configuration settings for database migrations
/// </summary>
public class MigrationSettings
{
    private MigrationType _migrationType = MigrationType.EfCore;
    private string _migrationTypeString = "EfCore";

    /// <summary>
    /// Whether migrations are enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Type of migration provider to use
    /// </summary>
    public MigrationType MigrationType
    {
        get => _migrationType;
        set
        {
            _migrationType = value;
            _migrationTypeString = value.ToStringValue();
        }
    }

    /// <summary>
    /// Type of migration provider as string (synchronized with MigrationType property)
    /// </summary>
    public string MigrationTypeString
    {
        get => _migrationTypeString;
        set
        {
            _migrationTypeString = value;
            if (!string.IsNullOrEmpty(value) && value.TryToEnum<MigrationType>(out var typeEnum))
            {
                _migrationType = typeEnum;
            }
        }
    }

    /// <summary>
    /// Automatically run migrations during deployment
    /// If false, migrations must be run manually
    /// </summary>
    public bool AutoRunOnDeployment { get; set; }

    /// <summary>
    /// Assembly name containing EF Core migrations (for EfCore type)
    /// Example: "MyApp.Data" or "MyApp.Infrastructure.dll"
    /// </summary>
    [StringLength(500, ErrorMessage = "Migration assembly name must not exceed 500 characters")]
    public string? MigrationAssembly { get; set; }

    /// <summary>
    /// Path to SQL script file (for SqlScript type)
    /// Example: "./migrations/schema.sql"
    /// </summary>
    [StringLength(1000, ErrorMessage = "SQL script path must not exceed 1000 characters")]
    public string? SqlScriptPath { get; set; }

    /// <summary>
    /// EF Core DbContext type name (for EfCore type)
    /// Example: "MyApp.Data.ApplicationDbContext"
    /// </summary>
    [StringLength(500, ErrorMessage = "DbContext type name must not exceed 500 characters")]
    public string? DbContextTypeName { get; set; }

    /// <summary>
    /// Timeout for migration execution in seconds
    /// Default: 300 seconds (5 minutes)
    /// </summary>
    [Range(30, 3600, ErrorMessage = "Migration timeout must be between 30 and 3600 seconds")]
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Whether to run migrations in a separate Container App Job
    /// If true, creates a dedicated job for migrations
    /// If false, runs migrations inline during deployment (faster but blocks deployment)
    /// </summary>
    public bool UseContainerJob { get; set; }

    /// <summary>
    /// Container image containing migration tools (if using Container App Job)
    /// Example: "myregistry.azurecr.io/myapp-migrations:latest"
    /// </summary>
    [StringLength(500, ErrorMessage = "Container image name must not exceed 500 characters")]
    public string? MigrationContainerImage { get; set; }

    /// <summary>
    /// Environment variables to pass to migration process
    /// Useful for providing connection strings or other configuration
    /// </summary>
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();

    /// <summary>
    /// Whether to fail deployment if migration fails
    /// If false, logs error but continues deployment (useful for development)
    /// </summary>
    public bool FailOnError { get; set; } = true;

    /// <summary>
    /// Whether to validate migration assembly/scripts before deployment
    /// </summary>
    public bool ValidateMigrationSource { get; set; } = true;

    /// <summary>
    /// Custom connection string for migrations (if different from database connection)
    /// Leave null to use the database connection string
    /// </summary>
    [StringLength(2000, ErrorMessage = "Connection string must not exceed 2000 characters")]
    public string? CustomConnectionString { get; set; }
}



