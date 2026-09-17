using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for managing Azure monitoring and observability infrastructure
/// Provides Application Insights, Log Analytics, and monitoring capabilities for the Application application
/// </summary>
public interface IMonitoringService : IInfrastructureService
{
    /// <summary>
    /// Creates and configures Azure monitoring infrastructure including Application Insights and Log Analytics workspace
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including monitoring retention, alerting, and dashboard configurations</param>
    /// <param name="resourceGroup">Azure resource group name where the monitoring resources will be deployed</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Monitoring infrastructure outputs containing instrumentation keys, workspace details, and connection strings</returns>
    new Task<MonitoringOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default);
}

