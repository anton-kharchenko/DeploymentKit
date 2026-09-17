namespace DeploymentKit.Settings;

/// <summary>
/// Diagnostic settings for Key Vault
/// </summary>
public class DiagnosticSettings
{
    /// <summary>
    /// Enable diagnostic logging
    /// </summary>
    public bool EnableDiagnostics { get; set; } = true;

    /// <summary>
    /// Log retention period in days
    /// </summary>
    public int LogRetentionDays { get; set; } = 30;

    /// <summary>
    /// Enable audit event logging
    /// </summary>
    public bool EnableAuditEvents { get; set; } = true;

    /// <summary>
    /// Enable all logs category
    /// </summary>
    public bool EnableAllLogs { get; set; } = true;

    /// <summary>
    /// Enable all metrics category
    /// </summary>
    public bool EnableAllMetrics { get; set; } = true;
}

