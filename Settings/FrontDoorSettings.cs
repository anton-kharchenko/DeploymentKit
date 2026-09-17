using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

/// <summary>
/// Azure Front Door (Standard/Premium) configuration.
/// Used to expose the deployment publicly with WAF/IP allowlisting for internal environments.
/// </summary>
public class FrontDoorSettings
{
    /// <summary>
    /// Enable provisioning of Azure Front Door resources.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Front Door SKU name (e.g., Standard_AzureFrontDoor or Premium_AzureFrontDoor).
    /// </summary>
    [StringLength(64)]
    public string SkuName { get; set; } = "Standard_AzureFrontDoor";

    /// <summary>
    /// Optional override for the Front Door endpoint name. If not set, a name will be derived from prefix/environment.
    /// </summary>
    [StringLength(90)]
    public string? EndpointNameOverride { get; set; }

    /// <summary>
    /// Health probe path used by the origin group.
    /// </summary>
    [StringLength(200)]
    public string HealthProbePath { get; set; } = "/health";

    /// <summary>
    /// Enable creation of a Front Door custom domain (e.g., dev.example.com). DNS CNAME must exist for activation.
    /// </summary>
    public bool EnableCustomDomain { get; set; } = true;

    /// <summary>
    /// Custom domain hostname (e.g., dev.example.com).
    /// </summary>
    [StringLength(253)]
    public string CustomDomainHostName { get; set; } = "dev.example.com";

    [StringLength(253)]
    public string WebsiteHostName { get; set; } = string.Empty;

    [StringLength(253)]
    public string MiniAppHostName { get; set; } = string.Empty;

    [StringLength(253)]
    public string ApiHostName { get; set; } = string.Empty;

    /// <summary>
    /// Whether to enable WAF policy and attach it to the Front Door endpoint.
    /// </summary>
    public bool EnableWaf { get; set; } = true;

    /// <summary>
    /// IP ranges (CIDR) allowed through WAF. If empty, will fall back to Container ingress IP allow rules when available.
    /// </summary>
    public List<string> AllowedIpRanges { get; set; } = [];

    /// <summary>
    /// Path prefixes that should bypass IP allowlisting (e.g., Stripe webhook endpoints).
    /// </summary>
    public List<string> WafBypassPathPrefixes { get; set; } =
    [
        "/webhooks/stripe",
        "/api/webhooks/stripe",
        "/stripe/webhook"
    ];
}


