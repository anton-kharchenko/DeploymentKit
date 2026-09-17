namespace DeploymentKit.Settings;

/// <summary>
/// Production environment access policies (restrictive for security)
/// </summary>
public class ProdAccessPoliciesSettings
{
    /// <summary>
    /// Restrict developer access to secrets in PROD (read-only if any)
    /// </summary>
    public bool AllowDeveloperSecretAccess { get; set; }

    /// <summary>
    /// Restrict developer access to keys in PROD
    /// </summary>
    public bool AllowDeveloperKeyAccess { get; set; }

    /// <summary>
    /// Restrict developer access to certificates in PROD
    /// </summary>
    public bool AllowDeveloperCertificateAccess { get; set; }

    /// <summary>
    /// Admin group object IDs with full access to PROD Key Vault
    /// </summary>
    public List<string> AdminGroupIds { get; set; } = new();

    /// <summary>
    /// Service principal object IDs for PROD applications
    /// </summary>
    public List<string> ServicePrincipalIds { get; set; } = new();

    /// <summary>
    /// Break-glass access group for emergency situations
    /// </summary>
    public List<string> EmergencyAccessGroupIds { get; set; } = new();
}



