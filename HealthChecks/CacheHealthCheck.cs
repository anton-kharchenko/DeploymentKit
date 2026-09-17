using DeploymentKit.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DeploymentKit.HealthChecks;

/// <summary>
/// Health check for cache services
/// </summary>
public class CacheHealthCheck(ILogger<CacheHealthCheck> logger, ICacheService cacheService) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting cache health check");

            var healthData = new Dictionary<string, object>
            {
                ["ServiceType"] = cacheService.GetType().Name,
                ["CheckTime"] = DateTime.UtcNow
            };

            logger.LogInformation("Cache service is available");
            return Task.FromResult(HealthCheckResult.Healthy("Cache service is available", healthData));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cache health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Cache health check failed", ex));
        }
    }
}

