namespace DeploymentKit.Settings
{
    /// <summary>
    /// Feature flags configuration
    /// </summary>
    public class FeatureFlagsSettings
    {
        /// <summary>
        /// Flagsmith environment key
        /// </summary>
        public string FlagsmithEnvironmentKey { get; set; } = string.Empty;

        /// <summary>
        /// Enable feature flag caching
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// Cache duration in minutes
        /// </summary>
        public int CacheDurationMinutes { get; set; } = 5;

        /// <summary>
        /// Default feature flag values for fallback
        /// </summary>
        public Dictionary<string, bool> DefaultFlags { get; set; } = new();
    }
}
