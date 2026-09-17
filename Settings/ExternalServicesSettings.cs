namespace DeploymentKit.Settings
{
    /// <summary>
    /// External services configuration
    /// </summary>
    public class ExternalServicesSettings
    {
        /// <summary>
        /// SerpApi configuration
        /// </summary>
        public ExternalServiceConfig SerpApi { get; set; } = new();

        /// <summary>
        /// Stripe payment service configuration
        /// </summary>
        public ExternalServiceConfig Stripe { get; set; } = new();

        /// <summary>
        /// UniRate API configuration
        /// </summary>
        public ExternalServiceConfig UniRateApi { get; set; } = new();

        /// <summary>
        /// OpenChargeMap configuration
        /// </summary>
        public ExternalServiceConfig OpenChargeMap { get; set; } = new();

        /// <summary>
        /// OpenTripMap configuration
        /// </summary>
        public ExternalServiceConfig OpenTripMap { get; set; } = new();

        /// <summary>
        /// OpenWeatherMap configuration
        /// </summary>
        public ExternalServiceConfig OpenWeatherMap { get; set; } = new();

        /// <summary>
        /// Transitland configuration
        /// </summary>
        public ExternalServiceConfig Transitland { get; set; } = new();
    }
}
