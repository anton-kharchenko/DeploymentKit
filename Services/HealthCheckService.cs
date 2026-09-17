using DeploymentKit.Constants;
using DeploymentKit.Helpers.HealthCheck;
using DeploymentKit.Interfaces;
using DeploymentKit.Models;
using DeploymentKit.Models.Results;
using System.Diagnostics;
using System.Text.Json;

namespace DeploymentKit.Services;

/// <summary>
/// Service for performing health checks on deployment slots
/// </summary>
public class HealthCheckService(HttpClient httpClient, ILogger<HealthCheckService> logger, ICorrelationIdService correlationIdService) : IHealthCheckService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<HealthCheckService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));

    public async Task<HealthCheckResult> CheckSlotHealthAsync(string slotName, string appUrl, string healthCheckUrl, TimeSpan? timeout = null)
    {
        var checkTimeout = timeout ?? TimeSpan.FromSeconds(30);
        var stopwatch = Stopwatch.StartNew();
        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["SlotName"] = slotName
        });

        var result = new HealthCheckResult
        {
            SlotName = slotName,
            CheckTimestamp = DateTime.UtcNow,
            Checks = new Dictionary<string, HealthCheckItem>()
        };

        try
        {
            logger.LogInformation("Starting health check for slot: {SlotName}", slotName);

            // Check basic connectivity
            var connectivityResult = await CheckConnectivityAsync(appUrl, checkTimeout);
            result.Checks.Add(ServiceConstants.HealthCheck.ConnectivityCheckName, connectivityResult);

            if (!connectivityResult.IsHealthy)
            {
                result.OverallStatus = ServiceConstants.HealthCheck.FailedNoConnectivityStatus;
                return result;
            }

            // Check health endpoint
            var healthEndpointResult = await CheckHealthEndpointAsync(healthCheckUrl, checkTimeout);
            result.Checks.Add(ServiceConstants.HealthCheck.HealthEndpointCheckName, healthEndpointResult);

            // Check application readiness
            var readinessResult = await CheckReadinessAsync(appUrl, checkTimeout);
            result.Checks.Add(ServiceConstants.HealthCheck.ReadinessCheckName, readinessResult);

            // Check performance metrics
            var performanceResult = await CheckPerformanceAsync(appUrl, checkTimeout);
            result.Checks.Add(ServiceConstants.HealthCheck.PerformanceCheckName, performanceResult);

            // Determine overall health
            result.IsHealthy = result.Checks.Values.All(c => c.IsHealthy);
            result.OverallStatus = HealthCheckHelper.GetStatus(result.IsHealthy);
            result.TotalCheckDuration = stopwatch.Elapsed;

            logger.LogInformation("Health check completed for slot: {SlotName}. Status: {Status}",
                slotName, result.OverallStatus);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed for slot: {SlotName}", slotName);
            result.OverallStatus = $"{ServiceConstants.HealthCheck.ErrorStatusPrefix}{ex.Message}";
            result.TotalCheckDuration = stopwatch.Elapsed;
            return result;
        }
    }

    public async Task<HealthCheckItem> CheckConnectivityAsync(string url, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Url"] = url
        });

        try
        {
            using var cts = new CancellationTokenSource(timeout);
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
            var response = await _httpClient.SendAsync(request, cts.Token);

            return new HealthCheckItem
            {
                IsHealthy = response.IsSuccessStatusCode,
                Status = HealthCheckHelper.GetStatus(response.IsSuccessStatusCode),
                Message = response.IsSuccessStatusCode ?
                    $"Connection to {url} successfully established" :
                    $"HTTP {(int)response.StatusCode} {response.StatusCode}",
                Duration = stopwatch.Elapsed,
                Data = new()
            };
        }
        catch (TaskCanceledException)
        {
            return new HealthCheckItem
            {
                IsHealthy = false,
                Status = HealthCheckHelper.UnhealthyStatus,
                Message = "Request timeout",
                Duration = stopwatch.Elapsed,
                Data = new()
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckItem
            {
                IsHealthy = false,
                Status = "Unhealthy",
                Message = ex.Message,
                Duration = stopwatch.Elapsed,
                Data = new()
            };
        }
    }

    public async Task<HealthCheckItem> CheckHealthEndpointAsync(string healthUrl, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["HealthUrl"] = healthUrl
        });

        try
        {
            using var cts = new CancellationTokenSource(timeout);
            using var request = new HttpRequestMessage(HttpMethod.Get, healthUrl);
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
            var response = await _httpClient.SendAsync(request, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);

                // Try to parse health response
                try
                {
                    var healthData = JsonSerializer.Deserialize<JsonElement>(content);
                    var status = healthData.GetProperty("status").GetString();

                    return new HealthCheckItem
                    {
                        IsHealthy = status?.Equals("Healthy", StringComparison.OrdinalIgnoreCase) == true,
                        Status = status?.Equals(HealthCheckHelper.HealthyStatus, StringComparison.OrdinalIgnoreCase) == true ? HealthCheckHelper.HealthyStatus : HealthCheckHelper.UnhealthyStatus,
                        Message = $"Health endpoint status: {status}",
                        Duration = stopwatch.Elapsed,
                        Data = new()
                    };
                }
                catch
                {
                    return new HealthCheckItem
                    {
                        IsHealthy = true,
                        Status = HealthCheckHelper.HealthyStatus,
                        Message = "Health endpoint responded",
                        Duration = TimeSpan.Zero,
                        Data = new()
                    };
                }
            }

            return new HealthCheckItem
            {
                IsHealthy = false,
                Status = "Unhealthy",
                Message = $"Health endpoint error: {(int)response.StatusCode}",
                Duration = stopwatch.Elapsed,
                Data = new()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health endpoint");
            return new HealthCheckItem
            {
                IsHealthy = false,
                Status = "Unhealthy",
                Message = $"Health check failed: {ex.Message}",
                Duration = stopwatch.Elapsed,
                Data = new()
            };
        }
    }

    public async Task<HealthCheckItem> CheckReadinessAsync(string baseUrl, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["BaseUrl"] = baseUrl
        });

        try
        {
            var readinessUrl = $"{baseUrl.TrimEnd('/')}/ready";
            using var cts = new CancellationTokenSource(timeout);
            using var request = new HttpRequestMessage(HttpMethod.Get, readinessUrl);
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
            var response = await _httpClient.SendAsync(request, cts.Token);

            return new HealthCheckItem
            {
                IsHealthy = response.IsSuccessStatusCode,
                Status = HealthCheckHelper.GetStatus(response.IsSuccessStatusCode),
                Message = response.IsSuccessStatusCode ? "Application ready" : $"Readiness check failed: {response.StatusCode}",
                Duration = stopwatch.Elapsed,
                Data = new()
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckItem
            {
                IsHealthy = false,
                Status = "Unhealthy",
                Message = $"Readiness check error: {ex.Message}",
                Duration = stopwatch.Elapsed,
                Data = new()
            };
        }
    }

    public async Task<HealthCheckItem> CheckPerformanceAsync(string baseUrl, TimeSpan timeout)
    {
        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["BaseUrl"] = baseUrl
        });

        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var cts = new CancellationTokenSource(timeout);
            using var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
            var response = await _httpClient.SendAsync(request, cts.Token);
            stopwatch.Stop();

            var responseTime = stopwatch.Elapsed;
            var isHealthy = response.IsSuccessStatusCode && responseTime < TimeSpan.FromSeconds(5);

            return new HealthCheckItem
            {
                IsHealthy = isHealthy,
                Status = HealthCheckHelper.GetStatus(isHealthy),
                Message = $"Response time: {responseTime.TotalMilliseconds:F0}ms",
                Duration = responseTime,
                Data = new Dictionary<string, object>()
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new HealthCheckItem
            {
                IsHealthy = false,
                Status = "Unhealthy",
                Message = $"Performance check error: {ex.Message}",
                Duration = stopwatch.Elapsed,
                Data = new Dictionary<string, object>()
            };
        }
    }
}

