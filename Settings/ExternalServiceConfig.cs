namespace DeploymentKit.Settings
{
    /// <summary>
    /// External service configuration
    /// </summary>
    public class ExternalServiceConfig
    {
        /// <summary>
        /// Base URL for the external service
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// API key for the service (stored in Key Vault)
        /// </summary>
        public string ApiKeyName { get; set; } = string.Empty;

        /// <summary>
        /// Timeout in seconds for requests
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Enable retry policy
        /// </summary>
        public bool EnableRetry { get; set; } = true;

        /// <summary>
        /// Maximum retry attempts
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;
    }
}
