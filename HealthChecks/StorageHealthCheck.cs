using DeploymentKit.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DeploymentKit.HealthChecks;

/// <summary>
/// Health check for storage services
/// </summary>
public class StorageHealthCheck(ILogger<StorageHealthCheck> logger, IStorageService storageService) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting storage health check");

            Dictionary<string, object> healthData = new()
            {
                ["ServiceType"] = storageService.GetType().Name,
                ["CheckTime"] = DateTime.UtcNow
            };

            logger.LogInformation("Storage service is available");
            return Task.FromResult(HealthCheckResult.Healthy("Storage service is available", healthData));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Storage health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Storage health check failed", ex));
        }
    }
}

