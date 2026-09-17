namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from Monitoring service
/// </summary>
public class MonitoringOutputs
{
    public Output<string> LogAnalyticsWorkspaceId { get; set; } = null!;
    public Output<string> LogAnalyticsWorkspacePrimaryKey { get; set; } = null!;
    public Output<string> ApplicationInsightsConnectionString { get; set; } = null!;
    public Output<string> ApplicationInsightsInstrumentationKey { get; set; } = null!;
    public Output<string> ApplicationInsightsId { get; set; } = null!;
    public string LogAnalyticsWorkspaceName { get; set; } = string.Empty;
    public string ApplicationInsightsName { get; set; } = string.Empty;
}

