using DeploymentKit.Constants;
using DeploymentKit.Helpers;
using DeploymentKit.Interfaces;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Partial class for InfrastructureBuilder containing remaining resource configuration methods.
/// </summary>
public partial class InfrastructureBuilder
{
    private bool _addMessageBroker;
    private EventHubsSettings? _eventHubsSettings;

    private bool _addInsights;
    private MonitoringSettings? _monitoringSettings;

    private bool _addContainerRegistry;

    private bool _addContainerApps;
    private ContainerSettings? _containerSettings;

    private bool _enableGreenBlueDeployment;
    private GreenBlueDeploymentSettings? _greenBlueSettings;

    public IInfrastructureBuilder AddMessageBroker()
    {
        _addMessageBroker = true;
        _eventHubsSettings = InfrastructureDefaultSettingsFactory.GetDefaultEventHubsSettings();
        _logger.LogInformation(BuilderConstants.LoggingMessages.EventHubsDefault);
        return this;
    }

    public IInfrastructureBuilder AddMessageBroker(EventHubsSettings eventHubsSettings)
    {
        _addMessageBroker = true;
        _eventHubsSettings = eventHubsSettings ?? throw new ArgumentNullException(nameof(eventHubsSettings));
        _logger.LogInformation(BuilderConstants.LoggingMessages.EventHubsCustom);
        return this;
    }

    public IInfrastructureBuilder AddInsights()
    {
        _addInsights = true;
        _monitoringSettings = InfrastructureDefaultSettingsFactory.GetDefaultMonitoringSettings();
        _logger.LogInformation(BuilderConstants.LoggingMessages.InsightsDefault);
        return this;
    }

    public IInfrastructureBuilder AddInsights(MonitoringSettings monitoringSettings)
    {
        _addInsights = true;
        _monitoringSettings = monitoringSettings ?? throw new ArgumentNullException(nameof(monitoringSettings));
        _logger.LogInformation(BuilderConstants.LoggingMessages.InsightsCustom);
        return this;
    }

    public IInfrastructureBuilder AddContainerRegistry()
    {
        _addContainerRegistry = true;
        _logger.LogInformation(BuilderConstants.LoggingMessages.ContainerRegistryAdded);
        return this;
    }

    public IInfrastructureBuilder AddContainerApps()
    {
        _addContainerApps = true;
        _containerSettings = InfrastructureDefaultSettingsFactory.GetDefaultContainerSettings();
        _logger.LogInformation(BuilderConstants.LoggingMessages.ContainerAppsDefault);
        return this;
    }

    public IInfrastructureBuilder AddContainerApps(ContainerSettings containerSettings)
    {
        _addContainerApps = true;
        _containerSettings = containerSettings ?? throw new ArgumentNullException(nameof(containerSettings));
        _logger.LogInformation(BuilderConstants.LoggingMessages.ContainerAppsCustom);
        return this;
    }

    public IInfrastructureBuilder EnableGreenBlueDeployment()
    {
        _enableGreenBlueDeployment = true;
        _greenBlueSettings = InfrastructureDefaultSettingsFactory.GetDefaultGreenBlueSettings();
        _logger.LogInformation(BuilderConstants.LoggingMessages.GreenBlueDefault);
        return this;
    }

    public IInfrastructureBuilder EnableGreenBlueDeployment(GreenBlueDeploymentSettings greenBlueSettings)
    {
        _enableGreenBlueDeployment = true;
        _greenBlueSettings = greenBlueSettings ?? throw new ArgumentNullException(nameof(greenBlueSettings));
        _logger.LogInformation(BuilderConstants.LoggingMessages.GreenBlueCustom);
        return this;
    }
}

