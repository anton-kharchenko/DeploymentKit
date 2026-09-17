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

namespace DeploymentKit.Helpers;

/// <summary>
/// Orchestrates the creation of Green-Blue infrastructure.
/// </summary>
public class GreenBlueInfrastructureCreator
{
    private readonly ILogger _logger;
    private readonly IResourceNamingService _namingService;
    private readonly ISlotManagementService _slotManagementService;
    private readonly IIngressManagementService _ingressManagementService;
    private readonly ICorrelationIdService _correlationIdService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GreenBlueInfrastructureCreator"/> class.
    /// </summary>
    public GreenBlueInfrastructureCreator(
        ILogger logger,
        IResourceNamingService namingService,
        ISlotManagementService slotManagementService,
        IIngressManagementService ingressManagementService,
        ICorrelationIdService correlationIdService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
        _slotManagementService = slotManagementService ?? throw new ArgumentNullException(nameof(slotManagementService));
        _ingressManagementService = ingressManagementService ?? throw new ArgumentNullException(nameof(ingressManagementService));
        _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
    }

    /// <summary>
    /// Creates the green-blue deployment infrastructure including container apps environment, slots, and main ingress.
    /// </summary>
    public async Task<GreenBlueDeploymentOutputs> CreateAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        MonitoringOutputs monitoring,
        ContainerRegistryOutputs containerRegistry,
        DatabaseOutputs database,
        CacheOutputs cache,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation(GreenBlueConstants.Messages.CreatingGreenBlueDeployment, settings.Environment);

            var environmentName = _namingService.GenerateContainerAppsEnvironmentName(settings.NamingPrefix, settings.Environment);

            var containerAppsEnvironment = new ManagedEnvironment(environmentName, new ManagedEnvironmentArgs
            {
                ResourceGroupName = resourceGroup,
                Location = settings.Location,
                AppLogsConfiguration = new AppLogsConfigurationArgs
                {
                    Destination = ContainerAppConstants.LogAnalyticsDestination,
                    LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                    {
                        CustomerId = monitoring.LogAnalyticsWorkspaceId,
                        SharedKey = monitoring.LogAnalyticsWorkspacePrimaryKey
                    }
                },
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, ContainerAppConstants.ContainerAppsEnvironmentType)
            });

            // Create green and blue slots
            var greenSlotTask = _slotManagementService.CreateSlotContainerAppAsync(
                DeploymentSlotType.Green.ToStringValue(),
                settings,
                resourceGroup,
                containerAppsEnvironment,
                containerRegistry,
                database,
                cache,
                monitoring,
                settings.GreenSlot ?? throw new InvalidOperationException("Green slot cannot be null"));
            var blueSlotTask = _slotManagementService.CreateSlotContainerAppAsync(
                DeploymentSlotType.Blue.ToStringValue(),
                settings,
                resourceGroup,
                containerAppsEnvironment,
                containerRegistry,
                database,
                cache,
                monitoring,
                settings.BlueSlot ?? throw new InvalidOperationException("Blue slot cannot be null"));

            await Task.WhenAll(greenSlotTask, blueSlotTask);

            var greenSlotOutputs = await greenSlotTask;
            var blueSlotOutputs = await blueSlotTask;

            // Create main ingress with traffic routing
            var mainIngress = await _ingressManagementService.CreateMainIngressAsync(settings, resourceGroup, containerAppsEnvironment, greenSlotOutputs, blueSlotOutputs);

            var outputs = new GreenBlueDeploymentOutputs
            {
                GreenSlot = greenSlotOutputs,
                BlueSlot = blueSlotOutputs,
                ActiveSlot = settings.GreenBlueDeployment?.ActiveSlot ?? throw new InvalidOperationException(),
                TargetSlot = settings.GreenBlueDeployment.TargetSlot,
                MainAppUrl = mainIngress.Configuration.Apply(c => $"{ContainerAppConstants.HttpsScheme}{c?.Ingress?.Fqdn}"),
                StagingUrl = GetStagingUrl(settings, greenSlotOutputs, blueSlotOutputs),
                EnvironmentName = environmentName,
                EnvironmentId = containerAppsEnvironment.Id,
                IsGreenBlueEnabled = settings.GreenBlueDeployment.Enabled,
                DeploymentStatus = DeploymentStatusType.Stable.ToStringValue(),
                TrafficDistribution = new Dictionary<string, int>
                {
                    { DeploymentSlotType.Green.ToStringValue(), settings.GreenSlot.TrafficPercentage },
                    { DeploymentSlotType.Blue.ToStringValue(), settings.BlueSlot.TrafficPercentage }
                }
            };

            _logger.LogInformation(GreenBlueConstants.Messages.GreenBlueDeploymentCreated, environmentName);
            return outputs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, GreenBlueConstants.Messages.GreenBlueDeploymentCreationFailed, settings.Environment);
            throw new ResourceCreationException(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, GreenBlueConstants.Messages.GreenBlueDeploymentCreationException, settings.Environment),
                ex,
                GreenBlueConstants.ExceptionContext,
                GreenBlueConstants.ContainerAppsContext,
                settings.Environment,
                _correlationIdService.GetOrGenerateCorrelationId(),
                GreenBlueConstants.CreationFailedErrorCode);
        }
    }

    private static Output<string> GetStagingUrl(InfrastructureSettings settings, SlotOutputs greenSlot, SlotOutputs blueSlot) => settings.GreenBlueDeployment?.TargetSlot.ToLowerInvariant() switch
    {
        GreenBlueConstants.GreenSlotName => greenSlot.AppUrl,
        _ => blueSlot.AppUrl
    };
}



