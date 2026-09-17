using DeploymentKit.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DeploymentKit.HealthChecks;

/// <summary>
/// Health check for overall infrastructure services
/// </summary>
public class InfrastructureHealthCheck(ILogger<InfrastructureHealthCheck> logger, IServiceProvider serviceProvider) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting infrastructure health check");

            var healthData = new Dictionary<string, object>();
            var isHealthy = true;
            var errors = new List<string>();

            await CheckServiceAvailability<INetworkService>("NetworkService", healthData, errors);
            await CheckServiceAvailability<IDatabaseService>("DatabaseService", healthData, errors);
            await CheckServiceAvailability<ICacheService>("CacheService", healthData, errors);
            await CheckServiceAvailability<IStorageService>("StorageService", healthData, errors);
            await CheckServiceAvailability<IMonitoringService>("MonitoringService", healthData, errors);

            if (errors.Count != 0)
            {
                isHealthy = false;
                logger.LogWarning("Infrastructure health check found issues: {Errors}", string.Join(", ", errors));
            }

            var result = isHealthy
                ? HealthCheckResult.Healthy("All infrastructure services are available", healthData)
                : HealthCheckResult.Degraded($"Some infrastructure services have issues: {string.Join(", ", errors)}", data: healthData);

            logger.LogInformation("Infrastructure health check completed with status: {Status}", result.Status);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Infrastructure health check failed");
            return HealthCheckResult.Unhealthy("Infrastructure health check failed", ex);
        }
    }

    private Task CheckServiceAvailability<T>(string serviceName, Dictionary<string, object> healthData, List<string> errors) where T : class
    {
        try
        {
            var service = serviceProvider.GetService<T>();
            if (service == null)
            {
                errors.Add($"{serviceName} not registered");
                healthData[serviceName] = "Not registered";
            }
            else
            {
                healthData[serviceName] = "Available";
                logger.LogDebug("{ServiceName} is available", serviceName);
            }
        }
        catch (Exception ex)
        {
            errors.Add($"{serviceName} error: {ex.Message}");
            healthData[serviceName] = $"Error: {ex.Message}";
            logger.LogWarning(ex, "Error checking {ServiceName} availability", serviceName);
        }

        return Task.CompletedTask;
    }
}

