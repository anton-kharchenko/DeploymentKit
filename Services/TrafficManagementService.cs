using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Exceptions;
using DeploymentKit.Extensions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing traffic switching.
/// </summary>
public class TrafficManagementService : ITrafficManagementService
{
    private readonly ILogger<TrafficManagementService> _logger;
    private readonly IResourceNamingService _namingService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly GreenBlueTrafficSwitcher _trafficSwitcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrafficManagementService"/> class.
    /// </summary>
    public TrafficManagementService(
        ILogger<TrafficManagementService> logger,
        IResourceNamingService namingService,
        ICorrelationIdService correlationIdService,
        GreenBlueTrafficSwitcher trafficSwitcher)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
        _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
        _trafficSwitcher = trafficSwitcher ?? throw new ArgumentNullException(nameof(trafficSwitcher));
    }

    /// <inheritdoc/>
    public Task<GreenBlueDeploymentOutputs> SwitchTrafficAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        string targetSlot,
        int trafficPercentage = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Switching traffic to {TargetSlot} slot with {TrafficPercentage}% traffic", targetSlot, trafficPercentage);

            var (greenWeight, blueWeight) = _trafficSwitcher.CalculateTrafficDistribution(targetSlot, trafficPercentage);
            var targetSlotLower = targetSlot.ToLowerInvariant();

            // Generate container app names
            var environmentName = _namingService.GenerateContainerAppsEnvironmentName(settings.NamingPrefix, settings.Environment);
            var mainAppName = _namingService.GenerateContainerAppName(settings.NamingPrefix, GreenBlueConstants.MainApiName, settings.Environment);
            var greenAppName = _namingService.GenerateContainerAppName(settings.NamingPrefix, GreenBlueConstants.GreenApiName, settings.Environment);
            var blueAppName = _namingService.GenerateContainerAppName(settings.NamingPrefix, GreenBlueConstants.BlueApiName, settings.Environment);

            // Get the managed environment
            var managedEnvironment = new ManagedEnvironment(environmentName, new ManagedEnvironmentArgs
            {
                ResourceGroupName = resourceGroup,
                Location = settings.Location,
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, ContainerAppConstants.ContainerAppsEnvironmentType)
            });

            // Update the main ingress container app with new traffic distribution
            var updatedMainApp = new ContainerApp(mainAppName, new ContainerAppArgs
            {
                ResourceGroupName = resourceGroup,
                Location = settings.Location,
                ManagedEnvironmentId = managedEnvironment.Id,
                Configuration = new ConfigurationArgs
                {
                    Ingress = new IngressArgs
                    {
                        External = true,
                        TargetPort = 8080,
                        Traffic = new[]
                        {
                            new TrafficWeightArgs
                            {
                                Weight = greenWeight,
                                RevisionName = greenAppName
                            },
                            new TrafficWeightArgs
                            {
                                Weight = blueWeight,
                                RevisionName = blueAppName
                            }
                        }
                    }
                },
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, GreenBlueConstants.MainContainerAppTag, new Dictionary<string, string>
                {
                    [ContainerAppConstants.ActiveSlotKey] = targetSlotLower,
                    [ContainerAppConstants.TrafficSwitchTimestampKey] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture)
                })
            });

            // Create slot outputs for the current state
            var greenSlotOutputs = new SlotOutputs
            {
                SlotName = GreenBlueConstants.GreenSlotName,
                AppName = greenAppName,
                AppId = Output.Create($"/subscriptions/subscription-id/resourceGroups/{resourceGroup}/providers/Microsoft.App/containerApps/{greenAppName}"),
                AppUrl = Output.Create($"https://{greenAppName}.azurecontainerapps.io"),
                InternalFqdn = Output.Create($"{greenAppName}.internal.azurecontainerapps.io"),
                TrafficPercentage = greenWeight,
                IsActive = targetSlotLower == GreenBlueConstants.GreenSlotName,
                ImageTag = settings.GreenSlot.ImageTag,
                Version = settings.GreenSlot.VersionString,
                DeploymentTimestamp = DateTime.UtcNow,
                IsHealthy = true,
                HealthCheckUrl = Output.Create($"https://{greenAppName}.azurecontainerapps.io{settings.GreenBlueDeployment?.HealthCheckPath}")
            };

            var blueSlotOutputs = new SlotOutputs
            {
                SlotName = GreenBlueConstants.BlueSlotName,
                AppName = blueAppName,
                AppId = Output.Create($"/subscriptions/subscription-id/resourceGroups/{resourceGroup}/providers/Microsoft.App/containerApps/{blueAppName}"),
                AppUrl = Output.Create($"https://{blueAppName}.azurecontainerapps.io"),
                InternalFqdn = Output.Create($"{blueAppName}.internal.azurecontainerapps.io"),
                TrafficPercentage = blueWeight,
                IsActive = targetSlotLower == GreenBlueConstants.BlueSlotName,
                ImageTag = settings.BlueSlot.ImageTag,
                Version = settings.BlueSlot.VersionString,
                DeploymentTimestamp = DateTime.UtcNow,
                IsHealthy = true,
                HealthCheckUrl = Output.Create($"https://{blueAppName}.azurecontainerapps.io{settings.GreenBlueDeployment?.HealthCheckPath}")
            };

            // Create the updated deployment outputs
            var deploymentOutputs = new GreenBlueDeploymentOutputs
            {
                GreenSlot = greenSlotOutputs,
                BlueSlot = blueSlotOutputs,
                ActiveSlot = targetSlotLower,
                TargetSlot = targetSlotLower,
                MainAppUrl = updatedMainApp.Configuration.Apply(c => $"{ContainerAppConstants.HttpsScheme}{c?.Ingress?.Fqdn}"),
                StagingUrl = targetSlotLower == GreenBlueConstants.GreenSlotName ? blueSlotOutputs.AppUrl : greenSlotOutputs.AppUrl,
                EnvironmentName = environmentName,
                EnvironmentId = managedEnvironment.Id,
                IsGreenBlueEnabled = settings.GreenBlueDeployment is { Enabled: true },
                DeploymentStatus = DeploymentStatusType.Switching.ToStringValue(),
                TrafficDistribution = new Dictionary<string, int>
                {
                    { GreenBlueConstants.GreenSlotName, greenWeight },
                    { GreenBlueConstants.BlueSlotName, blueWeight }
                }
            };

            _logger.LogInformation("Successfully switched traffic to {TargetSlot} slot with {TrafficPercentage}% traffic", targetSlot, trafficPercentage);
            return Task.FromResult(deploymentOutputs);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Traffic switching operation was cancelled for target slot: {TargetSlot}", targetSlot);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch traffic to {TargetSlot} slot with {TrafficPercentage}% traffic", targetSlot, trafficPercentage);
            throw new ResourceCreationException(
                $"Failed to switch traffic to {targetSlot} slot",
                ex,
                GreenBlueConstants.ExceptionContext,
                GreenBlueConstants.TrafficSwitchContext,
                settings.Environment,
                _correlationIdService.GetOrGenerateCorrelationId(),
                GreenBlueConstants.TrafficSwitchFailedErrorCode);
        }
    }
}


