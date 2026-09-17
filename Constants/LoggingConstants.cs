namespace DeploymentKit.Constants;

/// <summary>
/// Constants for logging messages and properties
/// </summary>
public static class LoggingConstants
{
    /// <summary>
    /// Structured logging property names
    /// </summary>
    public static class PropertyNames
    {
        public const string CorrelationId = "CorrelationId";
        public const string Environment = "Environment";
        public const string Service = "Service";
        public const string Operation = "Operation";
        public const string ResourceType = "ResourceType";
        public const string CacheName = "CacheName";
        public const string ResourceName = "ResourceName";
        public const string ServerName = "ServerName";
        public const string DatabaseName = "DatabaseName";
        public const string StorageAccountName = "StorageAccountName";
    }

    /// <summary>
    /// Service names for structured logging
    /// </summary>
    public static class ServiceNames
    {
        public const string CacheService = "CacheService";
        public const string StorageService = "StorageService";
        public const string MonitoringService = "MonitoringService";
        public const string NetworkService = "NetworkService";
        public const string DatabaseService = "DatabaseService";
    }

    /// <summary>
    /// Boolean string representations for logging
    /// </summary>
    public static class BooleanStrings
    {
        public const string True = "True";
        public const string False = "False";
    }

    /// <summary>
    /// Builder log messages
    /// </summary>
    public static class BuilderMessages
    {
        public const string DeploymentNameValidated = "Deployment name '{Name}' validated and set successfully";
        public const string DeploymentNameValidationFailed = "Failed to validate deployment name '{Name}': {Error}";
        public const string EnvironmentSet = "Environment set to '{Environment}'";
        public const string LocationSet = "Location set to '{Location}'";
        public const string LocationRegionSet = "Location set to '{Location}' ({Region})";
        public const string SubscriptionIdSet = "Subscription ID set";
        public const string ResourceGroupNameSet = "Resource group name set to '{ResourceGroupName}'";
        public const string NamingPrefixSet = "Naming prefix set to '{NamingPrefix}'";
        public const string UsingExistingSettings = "Using existing infrastructure settings";
        public const string BuildingWithExistingSettings = "Building infrastructure with existing settings";
        public const string BuildFailedValidation = "Build failed due to validation errors: {Errors}";
        public const string BuildSuccess = "Infrastructure settings built successfully for deployment '{DeploymentName}'";
        public const string ValidationModeSet = "Validation mode set to '{ValidationMode}'";
        public const string ValidationModeBasic = "Basic validation mode: Settings and naming checks only. Recommended for initial deployments.";
        public const string ValidationModeSkip = "Validation mode set to Skip - all validation will be bypassed. Use with caution!";
        public const string AzureAuthSkipped = "Azure authentication validation will be skipped. Using Azure CLI or default authentication chain.";
        public const string AzureAuthPerformed = "Azure authentication validation will be performed.";
    }

    /// <summary>
    /// Green-blue deployment log messages
    /// </summary>
    public static class GreenBlueMessages
    {
        public const string CreatingDeployment = "Creating green-blue deployment for environment: {Environment}";
        public const string CreationSuccess = "Successfully created green-blue deployment with environment: {EnvironmentName}";
        public const string CreationFailed = "Failed to create green-blue deployment for environment: {Environment}";
    }
}

