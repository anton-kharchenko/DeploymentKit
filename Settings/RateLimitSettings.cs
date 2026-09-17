namespace DeploymentKit.Settings
{
    /// <summary>
    /// Rate limiting configuration
    /// </summary>
    public class RateLimitSettings
    {
        /// <summary>
        /// Enable rate limiting
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Requests per minute limit
        /// </summary>
        public int RequestsPerMinute { get; set; } = 100;

        /// <summary>
        /// Burst limit for requests
        /// </summary>
        public int BurstLimit { get; set; } = 20;

        /// <summary>
        /// Rate limit window in minutes
        /// </summary>
        public int WindowMinutes { get; set; } = 1;
    }
}
