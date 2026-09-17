using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Service for performing health checks specifically for Green-Blue deployments.
/// </summary>
public class GreenBlueHealthCheckService(HttpClient httpClient, ILogger<GreenBlueHealthCheckService> logger)
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<GreenBlueHealthCheckService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Performs a health check on a deployment slot.
    /// </summary>
    /// <param name="slotName">Slot name used for logging.</param>
    /// <param name="healthCheckUrl">Resolved health check URL.</param>
    /// <param name="appUrl">Resolved application URL used as fallback when health URL is not set.</param>
    /// <param name="healthCheckSettings">Settings for the health check (retries, timeout, etc.).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the slot is healthy, otherwise false.</returns>
    public async Task<bool> PerformHealthCheckAsync(
        string slotName,
        string? healthCheckUrl,
        string? appUrl,
        GreenBlueDeploymentSettings healthCheckSettings,
        CancellationToken cancellationToken = default)
    {
        if (!healthCheckSettings.EnableHealthChecks)
        {
            _logger.LogInformation("Health checks are disabled, skipping health check for slot: {SlotName}", slotName);
            return true;
        }

        var timeout = TimeSpan.FromSeconds(healthCheckSettings.HealthCheckTimeoutSeconds);
        var retries = healthCheckSettings.HealthCheckRetries;
        var retryDelay = TimeSpan.FromSeconds(healthCheckSettings.HealthCheckRetryDelaySeconds);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Starting health check for slot: {SlotName}", slotName);

            if (string.IsNullOrWhiteSpace(healthCheckUrl))
            {
                if (string.IsNullOrWhiteSpace(appUrl))
                {
                    _logger.LogWarning("No health check URL or app URL available for slot: {SlotName}", slotName);
                    return false;
                }

                healthCheckUrl = $"{appUrl.TrimEnd('/')}{healthCheckSettings.HealthCheckPath}";
            }

            _logger.LogDebug("Performing health check request to: {HealthCheckUrl}", healthCheckUrl);

            for (var attempt = 1; attempt <= retries; attempt++)
            {
                try
                {
                    _logger.LogDebug("Health check attempt {Attempt}/{MaxAttempts} for slot: {SlotName}", attempt, retries, slotName);

                    using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    requestCts.CancelAfter(timeout);

                    var response = await _httpClient.GetAsync(healthCheckUrl, requestCts.Token);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Health check passed for slot: {SlotName} (Status: {StatusCode})", slotName, response.StatusCode);
                        return true;
                    }

                    _logger.LogWarning("Health check failed for slot: {SlotName} (Status: {StatusCode})", slotName, response.StatusCode);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Health check cancelled for slot: {SlotName}", slotName);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Health check attempt {Attempt} failed with exception for slot: {SlotName}", attempt, slotName);
                }

                if (attempt < retries)
                {
                    _logger.LogInformation("Waiting {DelaySeconds} seconds before retry due to exception", retryDelay.TotalSeconds);
                    await Task.Delay(retryDelay, cancellationToken);
                }
            }

            _logger.LogError("All health check attempts failed for slot: {SlotName} after {Attempts} attempts", slotName, retries);
            return false;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Health check operation was cancelled for slot: {SlotName}", slotName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during health check for slot: {SlotName}", slotName);
            return false;
        }
    }

}

