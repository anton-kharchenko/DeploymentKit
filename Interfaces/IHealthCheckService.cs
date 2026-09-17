using DeploymentKit.Models;
using DeploymentKit.Models.Results;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Interface for performing health checks on deployment slots
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Performs a health check on a specific slot
    /// </summary>
    /// <param name="slotName">Name of the slot to check</param>
    /// <param name="appUrl">Application URL for the slot</param>
    /// <param name="healthCheckUrl">Health check endpoint URL</param>
    /// <param name="timeout">Optional timeout for the health check</param>
    /// <returns>Health check result</returns>
    Task<HealthCheckResult> CheckSlotHealthAsync(string slotName, string appUrl, string healthCheckUrl, TimeSpan? timeout = null);

    /// <summary>
    /// Checks basic connectivity to a URL
    /// </summary>
    /// <param name="url">URL to check</param>
    /// <param name="timeout">Connection timeout</param>
    /// <returns>Health check item result</returns>
    Task<HealthCheckItem> CheckConnectivityAsync(string url, TimeSpan timeout);

    /// <summary>
    /// Checks the health endpoint of an application
    /// </summary>
    /// <param name="healthUrl">Health endpoint URL</param>
    /// <param name="timeout">Request timeout</param>
    /// <returns>Health check item result</returns>
    Task<HealthCheckItem> CheckHealthEndpointAsync(string healthUrl, TimeSpan timeout);

    /// <summary>
    /// Checks application readiness
    /// </summary>
    /// <param name="baseUrl">Base application URL</param>
    /// <param name="timeout">Request timeout</param>
    /// <returns>Health check item result</returns>
    Task<HealthCheckItem> CheckReadinessAsync(string baseUrl, TimeSpan timeout);

    /// <summary>
    /// Performs performance checks on the application
    /// </summary>
    /// <param name="baseUrl">Base application URL</param>
    /// <param name="timeout">Request timeout</param>
    /// <returns>Health check item result</returns>
    Task<HealthCheckItem> CheckPerformanceAsync(string baseUrl, TimeSpan timeout);
}

