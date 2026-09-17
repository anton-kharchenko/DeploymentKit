using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// Configuration settings for API endpoints and external service integrations
/// </summary>
public class ApiSettings
{
    /// <summary>
    /// Base URL for the API in the current environment
    /// </summary>
    [Required(ErrorMessage = "API base URL is required")]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API version to use for endpoints
    /// </summary>
    [Required(ErrorMessage = "API version is required")]
    public string Version { get; set; } = "v1";

    /// <summary>
    /// Enable HTTPS redirection
    /// </summary>
    public bool EnableHttpsRedirection { get; set; } = true;

    /// <summary>
    /// CORS configuration for the API
    /// </summary>
    public CorsSettings Cors { get; set; } = new();

    /// <summary>
    /// Rate limiting configuration
    /// </summary>
    public RateLimitSettings RateLimit { get; set; } = new();

    /// <summary>
    /// Rate limit requests per minute (direct property for backward compatibility)
    /// </summary>
    [Range(1, 10000, ErrorMessage = "Rate limit requests per minute must be between 1 and 10000")]
    public int RateLimitRequestsPerMinute { get; set; } = 100;

    /// <summary>
    /// External service endpoints configuration
    /// </summary>
    public ExternalServicesSettings ExternalServices { get; set; } = new();

    /// <summary>
    /// Authentication and authorization settings
    /// </summary>
    public AuthSettings Auth { get; set; } = new();

    /// <summary>
    /// Environment-specific feature flags
    /// </summary>
    [Required(ErrorMessage = "Feature flags settings are required")]
    public FeatureFlagsSettings FeatureFlags { get; set; } = new();

    /// <summary>
    /// Enable HTTPS for the API
    /// </summary>
    public bool EnableHttps { get; set; } = true;

    /// <summary>
    /// Enable Swagger documentation
    /// </summary>
    public bool EnableSwagger { get; set; } = true;

    /// <summary>
    /// CORS origins for the API
    /// </summary>
    public string CorsOrigins { get; set; } = "";

    /// <summary>
    /// Enable detailed error messages
    /// </summary>
    public bool EnableDetailedErrors { get; set; }
}
