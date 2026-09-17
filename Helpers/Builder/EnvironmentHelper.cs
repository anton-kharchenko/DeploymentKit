using DeploymentKit.Constants;
using DeploymentKit.Enums;

namespace DeploymentKit.Helpers.Builder;

/// <summary>
/// Helper for environment parsing and normalization.
/// </summary>
public static class EnvironmentHelper
{
    /// <summary>
    /// Parses the environment string into an EnvironmentType enum.
    /// </summary>
    public static EnvironmentType ParseEnvironmentType(string environment)
    {
        return environment.ToLowerInvariant() switch
        {
            ValidationConstants.DevelopmentEnvironment or ValidationConstants.ShortDevelopmentEnvironment => EnvironmentType.Development,
            ValidationConstants.ProductionEnvironment or ValidationConstants.ShortProductionEnvironment => EnvironmentType.Production,
            _ => EnvironmentType.Development
        };
    }

    /// <summary>
    /// Normalizes the environment string to a standard short form.
    /// </summary>
    public static string NormalizeEnvironmentName(string environment)
    {
        return environment.ToLowerInvariant() switch
        {
            ValidationConstants.DevelopmentEnvironment => ValidationConstants.ShortDevelopmentEnvironment,
            ValidationConstants.ProductionEnvironment => ValidationConstants.ShortProductionEnvironment,
            ValidationConstants.ShortDevelopmentEnvironment => ValidationConstants.ShortDevelopmentEnvironment,
            ValidationConstants.TestEnvironment => ValidationConstants.TestEnvironment,
            ValidationConstants.StagingEnvironment => ValidationConstants.StagingEnvironment,
            ValidationConstants.ShortProductionEnvironment => ValidationConstants.ShortProductionEnvironment,
            _ => ValidationConstants.ShortDevelopmentEnvironment // Default to dev for unknown values
        };
    }
}

