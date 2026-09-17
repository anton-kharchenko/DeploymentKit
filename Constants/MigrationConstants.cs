namespace DeploymentKit.Constants;

public static class MigrationConstants
{
    public static class ErrorMessages
    {
        public const string MigrationAssemblyRequired = "Migration assembly cannot be null or empty";
        public const string DbContextRequired = "DbContext type name cannot be null or empty";
        public const string SqlScriptPathRequired = "SQL script path cannot be null or empty";
    }

    public static class Logs
    {
        public const string EfCoreConfigured = "Database migrations configured with EF Core: Assembly={Assembly}, DbContext={DbContext}, AutoRun={AutoRun}";
        public const string SqlConfigured = "SQL migrations configured: ScriptPath={ScriptPath}, AutoRun={AutoRun}";
    }
}

