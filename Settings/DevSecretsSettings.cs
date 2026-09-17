namespace DeploymentKit.Settings;

/// <summary>
/// Development environment secret settings
/// </summary>
public class DevSecretsSettings
{
    /// <summary>
    /// Secret expiration period for DEV (shorter for security)
    /// </summary>
    public int DefaultExpirationDays { get; set; } = 365;

    /// <summary>
    /// Enable automatic secret rotation in DEV
    /// </summary>
    public bool EnableAutoRotation { get; set; }

    /// <summary>
    /// Content type for DEV secrets
    /// </summary>
    public string DefaultContentType { get; set; } = "text/plain";

    /// <summary>
    /// Tags to apply to DEV secrets
    /// </summary>
    public Dictionary<string, string> DefaultTags { get; set; } = new()
    {
        { "Environment", "Development" },
        { "Project", "Application" },
        { "ManagedBy", "Pulumi" }
    };
}



