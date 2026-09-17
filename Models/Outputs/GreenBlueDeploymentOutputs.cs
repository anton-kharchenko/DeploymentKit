using DeploymentKit.Enums;
using DeploymentKit.Extensions;

namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Output information for green-blue deployment
/// </summary>
public class GreenBlueDeploymentOutputs
{
    /// <summary>
    /// Green slot deployment outputs
    /// </summary>
    public SlotOutputs? GreenSlot { get; set; } = new() { SlotName = DeploymentSlotType.Green.ToStringValue() };

    /// <summary>
    /// Blue slot deployment outputs
    /// </summary>
    public SlotOutputs? BlueSlot { get; set; } = new() { SlotName = DeploymentSlotType.Blue.ToStringValue() };

    /// <summary>
    /// Currently active slot name
    /// </summary>
    public string ActiveSlot { get; set; } = "green";

    /// <summary>
    /// Target slot for new deployments
    /// </summary>
    public string TargetSlot { get; set; } = "blue";

    /// <summary>
    /// Main application URL (points to active slot)
    /// </summary>
    public Output<string> MainAppUrl { get; set; } = Output.Create(string.Empty);

    /// <summary>
    /// Staging URL (points to target slot)
    /// </summary>
    public Output<string> StagingUrl { get; set; } = Output.Create(string.Empty);

    /// <summary>
    /// Container Apps environment name
    /// </summary>
    public string EnvironmentName { get; set; } = string.Empty;

    /// <summary>
    /// Container Apps environment ID
    /// </summary>
    public Output<string> EnvironmentId { get; set; } = Output.Create(string.Empty);

    /// <summary>
    /// Indicates if green-blue deployment is enabled
    /// </summary>
    public bool IsGreenBlueEnabled { get; set; }

    /// <summary>
    /// Current deployment strategy status
    /// </summary>
    public string DeploymentStatus { get; set; } = "Stable";

    /// <summary>
    /// Last slot switch timestamp
    /// </summary>
    public DateTime? LastSlotSwitchTimestamp { get; set; }

    /// <summary>
    /// Traffic distribution between slots
    /// </summary>
    public Dictionary<string, int> TrafficDistribution { get; set; } = new()
    {
        { "green", 100 },
        { "blue", 0 }
    };
}

