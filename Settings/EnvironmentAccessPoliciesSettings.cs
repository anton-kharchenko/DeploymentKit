namespace DeploymentKit.Settings;

/// <summary>
/// Environment-specific access policies for Key Vault
/// </summary>
public class EnvironmentAccessPoliciesSettings
{
    /// <summary>
    /// Development environment access policies
    /// </summary>
    public DevAccessPoliciesSettings Development { get; set; } = new();

    /// <summary>
    /// Production environment access policies
    /// </summary>
    public ProdAccessPoliciesSettings Production { get; set; } = new();
}

