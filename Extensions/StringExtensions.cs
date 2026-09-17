using DeploymentKit.Enums;

namespace DeploymentKit.Extensions;

public static class StringExtensions
{
    public static EnvironmentType MapStringValueToEnvironmentType(this string environment) =>
        environment.ToLowerInvariant() switch
        {
            "dev" or "development" => EnvironmentType.Development,
            "prod" or "production" => EnvironmentType.Production,
            "test" => EnvironmentType.Development,
            "staging" => EnvironmentType.Production,
            _ => EnvironmentType.Development
        };
}

