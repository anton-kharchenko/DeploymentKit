namespace DeploymentKit.Settings
{
    /// <summary>
    /// CORS configuration settings
    /// </summary>
    public class CorsSettings
    {
        /// <summary>
        /// Allowed origins for CORS
        /// </summary>
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Allowed methods for CORS
        /// </summary>
        public string[] AllowedMethods { get; set; } = { "GET", "POST", "PUT", "DELETE", "OPTIONS" };

        /// <summary>
        /// Allowed headers for CORS
        /// </summary>
        public string[] AllowedHeaders { get; set; } = { "*" };

        /// <summary>
        /// Allow credentials in CORS requests
        /// </summary>
        public bool AllowCredentials { get; set; } = true;
    }
}
