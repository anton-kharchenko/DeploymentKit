using DeploymentKit.Enums;

namespace DeploymentKit.Extensions;

/// <summary>
/// Extension methods for DeploymentSlotType enum.
/// </summary>
public static class DeploymentSlotExtensions
{
    /// <summary>
    /// Gets the opposite slotType (Green -> Blue, Blue -> Green).
    /// </summary>
    /// <param name="slotType">The current slotType.</param>
    /// <returns>The opposite slotType.</returns>
    public static DeploymentSlotType GetOppositeSlot(this DeploymentSlotType slotType) => slotType == DeploymentSlotType.Green ? DeploymentSlotType.Blue : DeploymentSlotType.Green;

    /// <summary>
    /// Checks if the slotType is the active slotType based on traffic percentage.
    /// </summary>
    /// <param name="slotType">The slotType to check.</param>
    /// <param name="trafficPercentage">The traffic percentage for this slotType.</param>
    /// <returns>True if this slotType should be considered active.</returns>
    public static bool IsActiveSlot(this DeploymentSlotType slotType, int trafficPercentage) => trafficPercentage > 0;
}

