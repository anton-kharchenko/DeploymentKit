namespace DeploymentKit.Helpers.HealthCheck;

/// <summary>
/// Helper class for health check constants and status messages.
/// </summary>
public static class HealthCheckHelper
{
    public const string HealthyStatus = "Healthy";
    public const string UnhealthyStatus = "Unhealthy";

    /// <summary>
    /// Gets the health status string based on the success flag.
    /// </summary>
    public static string GetStatus(bool isSuccess) => isSuccess ? HealthyStatus : UnhealthyStatus;
}

