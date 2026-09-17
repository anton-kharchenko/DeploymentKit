namespace DeploymentKit.Settings;

/// <summary>
/// Environment-specific secret configurations
/// </summary>
public class EnvironmentSecretsSettings
{
    /// <summary>
    /// Development environment secrets
    /// </summary>
    public DevSecretsSettings Development { get; set; } = new();

    /// <summary>
    /// Production environment secrets
    /// </summary>
    public ProdSecretsSettings Production { get; set; } = new();
}

