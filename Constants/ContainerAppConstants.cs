namespace DeploymentKit.Constants;

public static class ContainerAppConstants
{
    public const string AspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";
    public const string AspNetCoreUrls = "ASPNETCORE_URLS";
    public const string ConnectionStringsDb = "ConnectionStrings__Db";
    public const string ConnectionStringsRedis = "ConnectionStrings__Redis";
    public const string ApplicationInsightsConnectionString = "APPLICATIONINSIGHTS_CONNECTION_STRING";
    public const string OtelExporterOtlpEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";
    public const string DeploymentSlot = "DEPLOYMENT_SLOT";
    public const string DeploymentVersion = "DEPLOYMENT_VERSION";
    public const string SlotName = "SLOT_NAME";
    public const string DeploymentTimestamp = "DEPLOYMENT_TIMESTAMP";

    // Secrets
    public const string AcrPasswordSecretName = "acr-password";
    public const string DbPasswordSecretName = "db-password";
    public const string PostgresConnectionStringSecretName = "postgres-connection-string";

    // Scaling
    public const string HttpScalingRuleName = "http-scaling";
    public const string ConcurrentRequestsMetadata = "concurrentRequests";
    public const string DefaultConcurrentRequests = "10";
    public const string CpuScalingRuleName = "cpu-scaling";
    public const string CpuScalingType = "cpu";
    public const string CpuUtilizationType = "Utilization";
    public const string DefaultCpuThreshold = "70";

    // Urls
    public const string HttpsScheme = "https://";
    public const string AzureContainerAppsDomain = ".azurecontainerapps.io";
    public const string HealthEndpoint = "/health";
    public const string LocalhostOtlpEndpoint = "http://localhost:4317";
    public const string DefaultAspNetCoreUrls = "http://+:8080";

    // Images
    public const string PlaceholderImage = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest";
    public const string DefaultRegistryServer = "mcr.microsoft.com";

    // Logging
    public const string LogAnalyticsDestination = "log-analytics";

    // Traffic
    public const int FullTrafficPercentage = 100;

    // Resource Tags
    public const string ContainerAppsEnvironmentType = "container-apps-environment";
    public const string ContainerAppType = "container-app";
    public const string SlotKey = "Slot";
    public const string ImageTagKey = "ImageTag";
    public const string DeploymentTypeKey = "DeploymentType";
    public const string GreenBlueDeploymentType = "green-blue";
    public const string ActiveSlotKey = "ActiveSlot";
    public const string TrafficSwitchTimestampKey = "TrafficSwitchTimestamp";

    // Messages
    public const string PlaceholderImageWarning = "Container Apps will use PLACEHOLDER images (mcr.microsoft.com/azuredocs/containerapps-helloworld:latest). Set UsePlaceholderImages=false and ensure your images are pushed to ACR before deploying with real application images.";
    public const string MinimalDependenciesWarning = "Creating ContainerApps with minimal dependencies. Consider using the full CreateAsync overload for production scenarios.";
    public const string SettingsNotProvidedMessage = "Container settings not provided. Skipping Container Apps provisioning.";
    public const string NamingPrefixRequired = "NamingPrefix cannot be null or empty";
    public const string EnvironmentRequired = "Environment cannot be null or empty";
    public const string SubnetTooSmallError = "Container Apps subnet must be at least /23. Current subnet: {0}. Please configure a larger subnet (e.g., 10.0.1.0/23 or 10.0.0.0/21) using the {1} environment variable.";
}

