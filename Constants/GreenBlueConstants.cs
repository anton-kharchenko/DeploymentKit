namespace DeploymentKit.Constants;

public static class GreenBlueConstants
{
    // Slot Names
    public const string GreenSlotName = "green";
    public const string BlueSlotName = "blue";
    public const string ApiPrefix = "api-";
    public const string ContainerAppPrefix = "container-app-api-";
    public const string MainApiName = "api-main";
    public const string GreenApiName = "api-green";
    public const string BlueApiName = "api-blue";
    public const string MainContainerAppTag = "container-app-api-main";

    // Resources
    public const string DefaultCpuAllocation = "0.5";
    public const string DefaultMemoryAllocation = "1Gi";
    public const string LatestImageTag = "latest";
    public const string DefaultVersion = "1.0.0";

    // Exception Contexts
    public const string ExceptionContext = "GreenBlueDeployment";
    public const string ContainerAppsContext = "ContainerApps";
    public const string TrafficSwitchContext = "TrafficSwitch";

    // Error Codes
    public const string CreationFailedErrorCode = "GREEN_BLUE_DEPLOYMENT_CREATION_FAILED";
    public const string TrafficSwitchFailedErrorCode = "TRAFFIC_SWITCH_FAILED";

    public static class Messages
    {
        public const string CreatingGreenBlueDeployment = "Creating green-blue deployment for environment: {Environment}";
        public const string GreenBlueDeploymentCreated = "Successfully created green-blue deployment with environment: {EnvironmentName}";
        public const string GreenBlueDeploymentCreationFailed = "Failed to create green-blue deployment for environment: {Environment}";
        public const string GreenBlueDeploymentCreationException = "Failed to create green-blue deployment for environment '{0}'";
    }
}

