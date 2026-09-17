namespace DeploymentKit.Constants;

/// <summary>
/// Constants for Green-Blue Service operations and logging.
/// </summary>
public static class GreenBlueServiceConstants
{
    public static class LoggingMessages
    {
        public const string CreatingDeployment = "Creating green-blue deployment for environment: {Environment}";
        public const string DeploymentCreated = "Successfully created green-blue deployment with environment: {EnvironmentName}";
        public const string DeploymentCreationFailedLog = "Failed to create green-blue deployment for environment '{Environment}'";
    }

    public static class ErrorMessages
    {
        public const string DeploymentCreationFailed = "Failed to create green-blue deployment for environment '{0}'";
    }
}

