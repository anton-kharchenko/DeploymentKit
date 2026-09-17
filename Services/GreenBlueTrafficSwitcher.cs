using DeploymentKit.Enums;
using DeploymentKit.Extensions;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing traffic switching logic for Green-Blue deployments.
/// </summary>
public class GreenBlueTrafficSwitcher(
    ILogger<GreenBlueTrafficSwitcher> logger)
{
    private readonly ILogger<GreenBlueTrafficSwitcher> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Calculates the traffic distribution based on the target slot and percentage.
    /// </summary>
    /// <param name="targetSlot">The target slot name ("green" or "blue").</param>
    /// <param name="targetTrafficPercentage">The percentage of traffic to direct to the target slot.</param>
    /// <returns>A dictionary containing traffic weights for green and blue slots.</returns>
    public (int GreenWeight, int BlueWeight) CalculateTrafficDistribution(string targetSlot, int targetTrafficPercentage)
    {
        if (targetTrafficPercentage is < 0 or > 100)
        {
            throw new ArgumentException("Traffic percentage must be between 0 and 100.", nameof(targetTrafficPercentage));
        }

        var targetSlotLower = targetSlot.ToLowerInvariant();
        if (targetSlotLower != DeploymentSlotType.Green.ToStringValue() &&
            targetSlotLower != DeploymentSlotType.Blue.ToStringValue())
        {
             throw new ArgumentException($"Invalid target slot: {targetSlot}. Must be '{DeploymentSlotType.Green.ToStringValue()}' or '{DeploymentSlotType.Blue.ToStringValue()}'.", nameof(targetSlot));
        }

        var sourceTrafficPercentage = 100 - targetTrafficPercentage;

        int greenWeight, blueWeight;
        if (targetSlotLower == DeploymentSlotType.Green.ToStringValue())
        {
            greenWeight = targetTrafficPercentage;
            blueWeight = sourceTrafficPercentage;
        }
        else
        {
            blueWeight = targetTrafficPercentage;
            greenWeight = sourceTrafficPercentage;
        }

        _logger.LogInformation("Traffic distribution calculated: Green={Green}%, Blue={Blue}% (Target: {TargetSlot})",
            greenWeight, blueWeight, targetSlotLower);

        return (greenWeight, blueWeight);
    }
}

