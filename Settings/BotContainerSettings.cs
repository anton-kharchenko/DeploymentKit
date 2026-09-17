using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

public class BotContainerSettings
{
    public bool Enabled { get; set; } = true;

    [Required]
    [StringLength(256, MinimumLength = 1)]
    public string ImageTag { get; set; } = string.Empty;

    [StringLength(253)]
    public string HostName { get; set; } = string.Empty;

    [StringLength(512)]
    public string WebhookUrl { get; set; } = string.Empty;

    [StringLength(256)]
    public string WebhookSecretToken { get; set; } = string.Empty;

    [StringLength(512)]
    public string MiniAppUrl { get; set; } = string.Empty;

    public bool ExternalIngress { get; set; } = true;

    [Range(1, 65535)]
    public int TargetPort { get; set; } = 8080;
}
