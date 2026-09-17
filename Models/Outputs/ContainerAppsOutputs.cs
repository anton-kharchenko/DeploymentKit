using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from Container Apps service
/// </summary>
public class ContainerAppsOutputs
{
    public Output<string> ApiAppUrl { get; set; } = null!;
    public Output<string> JobsInternalFqdn { get; set; } = null!;
    public Output<string> BotAppUrl { get; set; } = null!;
    public string ApiAppName { get; set; } = string.Empty;
    public string JobsAppName { get; set; } = string.Empty;
    public string BotAppName { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = string.Empty;
    public Output<string> EnvironmentId { get; set; } = null!;

    /// <summary>
    /// The actual Container App Environment resource (needed for Pulumi to deploy it)
    /// </summary>
    public ManagedEnvironment Environment { get; set; } = null!;

    /// <summary>
    /// The actual API Container App resource (needed for Pulumi to deploy it)
    /// </summary>
    public ContainerApp ApiApp { get; set; } = null!;

    /// <summary>
    /// The actual Jobs Container App resource (needed for Pulumi to deploy it)
    /// </summary>
    public ContainerApp JobsApp { get; set; } = null!;

    /// <summary>
    /// The actual Bot Container App resource (needed for Pulumi to deploy it)
    /// </summary>
    public ContainerApp? BotApp { get; set; }
}



