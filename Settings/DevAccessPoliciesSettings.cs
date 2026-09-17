namespace DeploymentKit.Settings;

/// <summary>
/// Development environment access policies (more permissive for development)
/// </summary>
public class DevAccessPoliciesSettings
{
    /// <summary>
    /// Allow developers to manage secrets in DEV environment
    /// </summary>
    public bool AllowDeveloperSecretAccess { get; set; } = true;

    /// <summary>
    /// Allow developers to manage keys in DEV environment
    /// </summary>
    public bool AllowDeveloperKeyAccess { get; set; } = true;

    /// <summary>
    /// Allow developers to manage certificates in DEV environment
    /// </summary>
    public bool AllowDeveloperCertificateAccess { get; set; } = true;

    /// <summary>
    /// Developer group object IDs with access to DEV Key Vault
    /// </summary>
    public List<string> DeveloperGroupIds { get; set; } = new();

    /// <summary>
    /// Service principal object IDs for DEV applications
    /// </summary>
    public List<string> ServicePrincipalIds { get; set; } = new();
}

