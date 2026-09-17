namespace DeploymentKit.Constants.Services;

public static class ContainerAppsConstants
{
    public const string PlaceholderImage = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest";
    public const string HttpsScheme = "https://";
    public const string ManagedEnvironmentResourceType = "ManagedEnvironment/ContainerApp";

    public static class ErrorMessages
    {
        public const string NamingPrefixRequired = "NamingPrefix cannot be null or empty";
        public const string EnvironmentRequired = "Environment cannot be null or empty";
        public const string SubnetTooSmall = "Container Apps subnet must be at least /23. Current subnet: {0}. Please configure a larger subnet (e.g., 10.0.1.0/23 or 10.0.0.0/21) using the {1} environment variable.";
        public const string CreationFailed = "Failed to create Container Apps for environment: {0}";
    }

    public static class LogMessages
    {
        public const string ContainerSettingsMissing = "Container settings not provided. Skipping Container Apps provisioning.";
        public const string PlaceholderImageWarning = "Container Apps will use PLACEHOLDER images (" + PlaceholderImage + "). Set UsePlaceholderImages=false and ensure your images are pushed to ACR before deploying with real application images.";
        public const string CreatingApiApp = "Creating API Container App with {SecretCount} secrets configured";
        public const string CreatingJobsApp = "Creating Jobs Container App with {SecretCount} secrets configured";
        public const string MinimalDependenciesWarning = "Creating ContainerApps with minimal dependencies. Consider using the full CreateAsync overload for production scenarios.";
    }

    public static class Defaults
    {
        public const string WorkspaceId = "default-workspace-id";
        public const string WorkspaceKey = "default-workspace-key";
        public const string AiKey = "default-ai-key";
        public const string AiConnection = "default-ai-connection";
        public const string AcrServer = "mcr.microsoft.com";
        public const string DbConnection = "Server=localhost;Database=Db;Integrated Security=true;";
        public const string DbServer = "localhost";
        public const string DbName = "Application";
        public const string RedisConnection = "localhost:6379";
        public const string RedisHost = "localhost";
        public const string RedisName = "localhost-cache";
        public const string VnetId = "default-vnet";
        public const string SubnetId = "default-subnet";
        public const string AgwSubnetId = "default-agw-subnet";
        public const string EventHubNamespace = "default-eventhub-namespace";
        public const string EventHubConnection = "Endpoint=sb://localhost/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=placeholder";
    }
}

