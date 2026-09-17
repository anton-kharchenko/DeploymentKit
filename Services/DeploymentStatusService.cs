using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Service for getting deployment status.
/// </summary>
public class DeploymentStatusService : IDeploymentStatusService
{
    private readonly ILogger<DeploymentStatusService> _logger;
    private readonly IResourceNamingService _namingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentStatusService"/> class.
    /// </summary>
    public DeploymentStatusService(
        ILogger<DeploymentStatusService> logger,
        IResourceNamingService namingService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
    }

    /// <inheritdoc/>
    public async Task<GreenBlueDeploymentOutputs> GetDeploymentStatusAsync(
        InfrastructureSettings settings,
        string resourceGroupName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Getting deployment status for green-blue deployment");

            // Generate container app names for both slots
            var greenAppName = _namingService.GenerateContainerAppName(settings.NamingPrefix, GreenBlueConstants.GreenApiName, settings.Environment);
            var blueAppName = _namingService.GenerateContainerAppName(settings.NamingPrefix, GreenBlueConstants.BlueApiName, settings.Environment);

            _logger.LogDebug("Querying status for apps: Green={GreenApp}, Blue={BlueApp}", greenAppName, blueAppName);

            // Query both container apps concurrently
            var greenTask = GetSlotStatusAsync(resourceGroupName, greenAppName, GreenBlueConstants.GreenSlotName);
            var blueTask = GetSlotStatusAsync(resourceGroupName, blueAppName, GreenBlueConstants.BlueSlotName);

            await Task.WhenAll(greenTask, blueTask);

            var greenSlot = await greenTask;
            var blueSlot = await blueTask;

            // Determine active and target slots based on traffic distribution
            var activeSlot = DetermineActiveSlot(greenSlot, blueSlot);
            var targetSlot = activeSlot == GreenBlueConstants.GreenSlotName ? GreenBlueConstants.BlueSlotName : GreenBlueConstants.GreenSlotName;

            // Create deployment outputs
            var deploymentOutputs = new GreenBlueDeploymentOutputs
            {
                GreenSlot = greenSlot,
                BlueSlot = blueSlot,
                ActiveSlot = activeSlot,
                TargetSlot = targetSlot,
                MainAppUrl = activeSlot == GreenBlueConstants.GreenSlotName ? greenSlot?.AppUrl ?? Output.Create(string.Empty) : blueSlot?.AppUrl ?? Output.Create(string.Empty),
                StagingUrl = targetSlot == GreenBlueConstants.GreenSlotName ? greenSlot?.AppUrl ?? Output.Create(string.Empty) : blueSlot?.AppUrl ?? Output.Create(string.Empty),
                EnvironmentName = settings.Environment,
                EnvironmentId = Output.Create($"{settings.NamingPrefix}-{settings.Environment}"),
                IsGreenBlueEnabled = true,
                DeploymentStatus = DetermineDeploymentStatus(greenSlot, blueSlot),
                LastSlotSwitchTimestamp = GetLastSlotSwitchTimestamp(greenSlot, blueSlot),
                TrafficDistribution = new Dictionary<string, int>
                {
                    [GreenBlueConstants.GreenSlotName] = greenSlot?.TrafficPercentage ?? 0,
                    [GreenBlueConstants.BlueSlotName] = blueSlot?.TrafficPercentage ?? 0
                }
            };

            _logger.LogInformation("Successfully retrieved deployment status. Active slot: {ActiveSlot}, Status: {Status}",
                activeSlot, deploymentOutputs.DeploymentStatus);

            return deploymentOutputs;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Get deployment status operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get deployment status");
            throw new InvalidOperationException($"Failed to get deployment status: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public Task<SlotOutputs> GetSlotStatusAsync(string resourceGroupName, string appName, string slotName)
    {
        try
        {
            // In a real implementation, this would query the Azure Container Apps API
            // For now, we'll create a basic slot status based on naming conventions
            _logger.LogDebug("Getting status for slot {SlotName} (app: {AppName})", slotName, appName);

            // This is a simplified implementation - in practice, you would use Azure SDK
            // to query the actual container app status
            var slotOutputs = new SlotOutputs
            {
                SlotName = slotName,
                AppName = appName,
                AppId = Output.Create($"/subscriptions/subscription-id/resourceGroups/{resourceGroupName}/providers/Microsoft.App/containerApps/{appName}"),
                AppUrl = Output.Create($"{ContainerAppConstants.HttpsScheme}{appName}{ContainerAppConstants.AzureContainerAppsDomain}"),
                InternalFqdn = Output.Create($"{appName}.internal{ContainerAppConstants.AzureContainerAppsDomain}"),
                TrafficPercentage = slotName == GreenBlueConstants.GreenSlotName ? 100 : 0, // Default assumption
                IsActive = slotName == GreenBlueConstants.GreenSlotName, // Default assumption
                ImageTag = GreenBlueConstants.LatestImageTag, // Would be queried from actual app
                Version = GreenBlueConstants.DefaultVersion, // Would be queried from actual app
                DeploymentTimestamp = DateTime.UtcNow.AddHours(-1), // Placeholder
                IsHealthy = true, // Would be determined by actual health checks
                HealthCheckUrl = Output.Create($"{ContainerAppConstants.HttpsScheme}{appName}{ContainerAppConstants.AzureContainerAppsDomain}{ContainerAppConstants.HealthEndpoint}"),
                LastHealthCheckTimestamp = DateTime.UtcNow.AddMinutes(-5) // Placeholder
            };

            return Task.FromResult(slotOutputs);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get status for slot {SlotName}", slotName);
            return Task.FromResult(new SlotOutputs
            {
                SlotName = slotName,
                AppName = appName,
                IsHealthy = false,
                LastHealthCheckTimestamp = DateTime.UtcNow
            });
        }
    }

    private static string DetermineActiveSlot(SlotOutputs? greenSlot, SlotOutputs? blueSlot) =>
        greenSlot?.TrafficPercentage > (blueSlot?.TrafficPercentage ?? 0)
            ?
            DeploymentSlotType.Green.ToStringValue()
            : blueSlot?.TrafficPercentage > 0
                ? DeploymentSlotType.Blue.ToStringValue()
                : DeploymentSlotType.Green.ToStringValue();

    private static string DetermineDeploymentStatus(SlotOutputs? greenSlot, SlotOutputs? blueSlot)
    {
        if (greenSlot == null && blueSlot == null)
        {
            return DeploymentStatusType.NotDeployed.ToStringValue();
        }

        if (greenSlot?.IsHealthy == true && blueSlot?.IsHealthy == true)
        {
            return HealthStatusType.Healthy.ToStringValue();
        }

        if (greenSlot?.IsHealthy == false || blueSlot?.IsHealthy == false)
        {
            return HealthStatusType.Unhealthy.ToStringValue();
        }

        return DeploymentStatusType.Deploying.ToStringValue();
    }

    private static DateTime? GetLastSlotSwitchTimestamp(SlotOutputs? greenSlot, SlotOutputs? blueSlot)
    {
        var greenTime = greenSlot?.DeploymentTimestamp;
        var blueTime = blueSlot?.DeploymentTimestamp;

        if (greenTime.HasValue && blueTime.HasValue)
        {
            return greenTime > blueTime ? greenTime : blueTime;
        }

        return greenTime ?? blueTime;
    }
}

