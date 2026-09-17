namespace DeploymentKit.Settings;

/// <summary>
/// Production environment secret settings
/// </summary>
public class ProdSecretsSettings
{
    /// <summary>
    /// Secret expiration period for PROD (longer for stability)
    /// </summary>
    public int DefaultExpirationDays { get; set; } = 730;

    /// <summary>
    /// Enable automatic secret rotation in PROD
    /// </summary>
    public bool EnableAutoRotation { get; set; } = true;

    /// <summary>
    /// Secret rotation interval in days
    /// </summary>
    public int RotationIntervalDays { get; set; } = 90;

    /// <summary>
    /// Content type for PROD secrets
    /// </summary>
    public string DefaultContentType { get; set; } = "text/plain";

    /// <summary>
    /// Tags to apply to PROD secrets
    /// </summary>
    public Dictionary<string, string> DefaultTags { get; set; } = new()
    {
        { "Environment", "Production" },
        { "Project", "Application" },
        { "ManagedBy", "Pulumi" },
        { "Criticality", "High" },
        { "DataClassification", "Confidential" }
    };
}

