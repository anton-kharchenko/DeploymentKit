namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from Database Migration service
/// </summary>
public class MigrationOutputs
{
    /// <summary>
    /// Whether migrations were executed successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of migrations applied
    /// </summary>
    public int MigrationsApplied { get; set; }

    /// <summary>
    /// Migration execution timestamp
    /// </summary>
    public DateTime ExecutionTime { get; set; }

    /// <summary>
    /// Detailed execution log
    /// </summary>
    public string ExecutionLog { get; set; } = string.Empty;

    /// <summary>
    /// Error message if migration failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Container App Job name (if using Container Job for migrations)
    /// </summary>
    public string? JobName { get; set; }
    
    /// <summary>
    /// Migration type used
    /// </summary>
    public string MigrationType { get; set; } = string.Empty;
}

