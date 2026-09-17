namespace DeploymentKit.Settings
{
    /// <summary>
    /// Authentication and authorization settings
    /// </summary>
    public class AuthSettings
    {
        /// <summary>
        /// JWT token issuer
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// JWT token audience
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// JWT token expiration in minutes
        /// </summary>
        public int TokenExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Refresh token expiration in days
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;

        /// <summary>
        /// Enable two-factor authentication
        /// </summary>
        public bool EnableTwoFactorAuth { get; set; }
    }
}
