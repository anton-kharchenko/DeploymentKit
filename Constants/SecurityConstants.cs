namespace DeploymentKit.Constants;

public static class SecurityConstants
{
    public static class ErrorMessages
    {
        public const string EnvFilePathRequired = "Environment file path cannot be null or empty";
        public const string InvalidKeyVaultSecretNames = "Invalid Key Vault secret names in .env file: {0}";
        public const string FailedToParseEnvFile = "Failed to parse .env file '{0}': {1}";
    }
}

