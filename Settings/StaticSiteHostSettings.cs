using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

public class StaticSiteHostSettings
{
    public bool Enabled { get; set; } = true;

    [StringLength(32, MinimumLength = 2)]
    public string SiteName { get; set; } = string.Empty;

    [StringLength(253)]
    public string HostName { get; set; } = string.Empty;

    [StringLength(128)]
    public string IndexDocument { get; set; } = "index.html";

    [StringLength(128)]
    public string ErrorDocument404Path { get; set; } = "index.html";
}
