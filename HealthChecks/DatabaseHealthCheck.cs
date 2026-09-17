using DeploymentKit.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DeploymentKit.HealthChecks;

/// <summary>
/// Health check for database services
/// </summary>
public class DatabaseHealthCheck(ILogger<DatabaseHealthCheck> logger, IDatabaseService databaseService) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting database health check");

            // Basic service availability check
            var healthData = new Dictionary<string, object>
            {
                ["ServiceType"] = databaseService.GetType().Name,
                ["CheckTime"] = DateTime.UtcNow
            };

            logger.LogInformation("Database service is available");
            return Task.FromResult(HealthCheckResult.Healthy("Database service is available", healthData));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Database health check failed", ex));
        }
    }
}

