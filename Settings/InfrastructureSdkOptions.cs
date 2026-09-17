namespace DeploymentKit.Settings;

/// <summary>
/// Generic configuration options for deploying applications with the SDK.
/// </summary>
public class InfrastructureSdkOptions
{
    public bool EnableStructuredLogging { get; set; } = true;

    public bool EnableApplicationInsights { get; set; }

    public string? HttpClientName { get; set; }

    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromMinutes(5);

    public string? UserAgent { get; set; }

    public int MaxRetryAttempts { get; set; } = 3;
}

