using System.Text;

namespace DeploymentKit.Utilities;

/// <summary>
/// Provides utility methods for retrieving environment variables and configuration values
/// for deployment automation and client applications.
/// </summary>
public static class EnvironmentConfigurationHelper
{
    /// <summary>
    /// Gets the deployment mode from the DEPLOYMENT_MODE environment variable.
    /// </summary>
    /// <returns>The deployment mode in lowercase, or "development" if not set.</returns>
    public static string GetDeploymentMode() => Environment.GetEnvironmentVariable("DEPLOYMENT_MODE")?.ToLower() ?? "development";

    /// <summary>
    /// Gets a required environment variable value.
    /// </summary>
    /// <param name="name">The name of the environment variable.</param>
    /// <returns>The value of the environment variable.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the environment variable is not set or is empty.</exception>
    public static string GetRequiredEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException(new StringBuilder()
                .Append("Required environment variable '")
                .Append(name)
                .Append("' is not set or is empty. ")
                .Append("Please set this variable before running the deployment.").ToString())
            : value;
    }

    /// <summary>
    /// Gets a secret value from environment variables or Pulumi configuration.
    /// Tries environment variable first, then falls back to Pulumi config.
    /// </summary>
    /// <param name="config">The Pulumi configuration object.</param>
    /// <param name="configKey">The key to look up in Pulumi configuration.</param>
    /// <param name="envVar">The environment variable name to check first.</param>
    /// <returns>The secret value from either environment variable or Pulumi config.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the secret is not found in either location.</exception>
    public static string GetSecretFromConfigOrEnv(Config config, string configKey, string envVar)
    {
        var envValue = Environment.GetEnvironmentVariable(envVar);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            return envValue;
        }

        try
        {
            var configValue = config.Get(configKey);
            if (!string.IsNullOrWhiteSpace(configValue))
            {
                return configValue;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Could not read {configKey} from Pulumi config: {ex.Message}");
        }

        throw new InvalidOperationException($"❌ Secret '{envVar}' not found in environment variables or Pulumi config ('{configKey}'). Please ensure this secret is properly configured.");
    }
}

