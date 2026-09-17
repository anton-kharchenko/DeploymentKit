using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Exceptions;
using DeploymentKit.Extensions;
using DeploymentKit.Helpers.ContainerApps;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing green-blue deployment container apps
/// </summary>
public class GreenBlueContainerAppsService : IGreenBlueContainerAppsService
{
    private readonly ILogger<GreenBlueContainerAppsService> _logger;
    private readonly IResourceNamingService _namingService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly GreenBlueHealthCheckService _healthCheckService;
    private readonly ISlotManagementService _slotManagementService;
    private readonly IIngressManagementService _ingressManagementService;
    private readonly IDeploymentStatusService _deploymentStatusService;
    private readonly ITrafficManagementService _trafficManagementService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GreenBlueContainerAppsService"/> class.
    /// </summary>
    public GreenBlueContainerAppsService(
        ILogger<GreenBlueContainerAppsService> logger,
        IResourceNamingService namingService,
        ICorrelationIdService correlationIdService,
        GreenBlueHealthCheckService healthCheckService,
        ISlotManagementService slotManagementService,
        IIngressManagementService ingressManagementService,
        IDeploymentStatusService deploymentStatusService,
        ITrafficManagementService trafficManagementService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
        _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
        _slotManagementService = slotManagementService ?? throw new ArgumentNullException(nameof(slotManagementService));
        _ingressManagementService = ingressManagementService ?? throw new ArgumentNullException(nameof(ingressManagementService));
        _deploymentStatusService = deploymentStatusService ?? throw new ArgumentNullException(nameof(deploymentStatusService));
        _trafficManagementService = trafficManagementService ?? throw new ArgumentNullException(nameof(trafficManagementService));
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
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
        NetworkOutputs network,
        EventHubsOutputs eventHubs,
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

            // Create green and blue slots concurrently
            var (greenSlotOutputs, blueSlotOutputs) = await ContainerAppsSlotOrchestrator.CreateSlotsAsync(
                _slotManagementService, settings, resourceGroup, containerAppsEnvironment, containerRegistry, database, cache, monitoring, cancellationToken);

            // Create main ingress with traffic routing
            var mainIngress = await _ingressManagementService.CreateMainIngressAsync(settings, resourceGroup, containerAppsEnvironment, greenSlotOutputs, blueSlotOutputs);

            var outputs = new GreenBlueDeploymentOutputs
            {
                GreenSlot = greenSlotOutputs,
                BlueSlot = blueSlotOutputs,
                ActiveSlot = settings.GreenBlueDeployment?.ActiveSlot ?? throw new InvalidOperationException(),
                TargetSlot = settings.GreenBlueDeployment.TargetSlot,
                MainAppUrl = mainIngress.Configuration.Apply(c => $"{ContainerAppConstants.HttpsScheme}{c?.Ingress?.Fqdn}"),
                StagingUrl = settings.GetStagingUrl(greenSlotOutputs, blueSlotOutputs),
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

    public async Task<GreenBlueDeploymentOutputs> CreateAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        Task<MonitoringOutputs> monitoringTask,
        Task<ContainerRegistryOutputs> containerRegistryTask,
        Task<DatabaseOutputs> databaseTask,
        Task<CacheOutputs> cacheTask,
        Task<NetworkOutputs> networkTask,
        Task<EventHubsOutputs> eventHubsTask,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(monitoringTask);
        ArgumentNullException.ThrowIfNull(containerRegistryTask);
        ArgumentNullException.ThrowIfNull(databaseTask);
        ArgumentNullException.ThrowIfNull(cacheTask);
        ArgumentNullException.ThrowIfNull(networkTask);
        ArgumentNullException.ThrowIfNull(eventHubsTask);

        cancellationToken.ThrowIfCancellationRequested();

        await Task.WhenAll(monitoringTask, containerRegistryTask, databaseTask, cacheTask, networkTask, eventHubsTask);

        return await CreateAsync(
            settings,
            resourceGroup,
            await monitoringTask,
            await containerRegistryTask,
            await databaseTask,
            await cacheTask,
            await networkTask,
            await eventHubsTask,
            cancellationToken);
    }

    /// <summary>
    /// Switches traffic between slots.
    /// </summary>
    public Task<GreenBlueDeploymentOutputs> SwitchTrafficAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        string targetSlot,
        int trafficPercentage = 100,
        CancellationToken cancellationToken = default)
    {
        return _trafficManagementService.SwitchTrafficAsync(settings, resourceGroup, targetSlot, trafficPercentage, cancellationToken);
    }

    /// <summary>
    /// Performs health check on a slot.
    /// </summary>
    public Task<bool> PerformHealthCheckAsync(
        string slotName,
        string? healthCheckUrl,
        string? appUrl,
        GreenBlueDeploymentSettings healthCheckSettings,
        CancellationToken cancellationToken = default)
    {
        return _healthCheckService.PerformHealthCheckAsync(slotName, healthCheckUrl, appUrl, healthCheckSettings, cancellationToken);
    }

    /// <summary>
    /// Updates a slot container app.
    /// </summary>
    public Task<SlotOutputs> UpdateSlotAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        string slotName,
        string imageTag,
        Dictionary<string, string>? environmentVariables = null,
        CancellationToken cancellationToken = default)
    {
        return _slotManagementService.UpdateSlotAsync(settings, resourceGroup, slotName, imageTag, environmentVariables, cancellationToken);
    }

    /// <summary>
    /// Gets the deployment status.
    /// </summary>
    public Task<GreenBlueDeploymentOutputs> GetDeploymentStatusAsync(
        InfrastructureSettings settings,
        string resourceGroupName,
        CancellationToken cancellationToken = default)
    {
        return _deploymentStatusService.GetDeploymentStatusAsync(settings, resourceGroupName, cancellationToken);
    }

    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken)
    {
        return await Task.FromResult<object>(new GreenBlueDeploymentOutputs
        {
            GreenSlot = new SlotOutputs { SlotName = GreenBlueConstants.GreenSlotName },
            BlueSlot = new SlotOutputs { SlotName = GreenBlueConstants.BlueSlotName },
            ActiveSlot = settings.GreenBlueDeployment?.ActiveSlot ?? throw new InvalidOperationException(),
            TargetSlot = settings.GreenBlueDeployment.TargetSlot,
            MainAppUrl = Output.Create(string.Empty),
            StagingUrl = Output.Create(string.Empty),
            EnvironmentName = settings.Environment,
            EnvironmentId = Output.Create(string.Empty),
            IsGreenBlueEnabled = settings.GreenBlueDeployment.Enabled,
            DeploymentStatus = DeploymentStatusType.RequiresDependencies.ToStringValue(),
            LastSlotSwitchTimestamp = null,
            TrafficDistribution = new Dictionary<string, int>
            {
                { DeploymentSlotType.Green.ToStringValue(), settings.GreenBlueDeployment.ActiveSlotTrafficPercentage },
                { DeploymentSlotType.Blue.ToStringValue(), settings.GreenBlueDeployment.TargetSlotTrafficPercentage }
            }
        });
    }

    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup)
    {
        return await Task.FromResult<object>(new GreenBlueDeploymentOutputs
        {
            GreenSlot = new SlotOutputs { SlotName = GreenBlueConstants.GreenSlotName },
            BlueSlot = new SlotOutputs { SlotName = GreenBlueConstants.BlueSlotName },
            ActiveSlot = settings.GreenBlueDeployment?.ActiveSlot ?? throw new InvalidOperationException(),
            TargetSlot = settings.GreenBlueDeployment.TargetSlot,
            MainAppUrl = Output.Create(string.Empty),
            StagingUrl = Output.Create(string.Empty),
            EnvironmentName = settings.Environment,
            EnvironmentId = Output.Create(string.Empty),
            IsGreenBlueEnabled = settings.GreenBlueDeployment.Enabled,
            DeploymentStatus = DeploymentStatusType.RequiresDependencies.ToStringValue(),
            LastSlotSwitchTimestamp = null,
            TrafficDistribution = new Dictionary<string, int>
            {
                { DeploymentSlotType.Green.ToStringValue(), settings.GreenBlueDeployment.ActiveSlotTrafficPercentage },
                { DeploymentSlotType.Blue.ToStringValue(), settings.GreenBlueDeployment.TargetSlotTrafficPercentage }
            }
        });
    }
}



