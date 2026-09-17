using DeploymentKit.Constants;
using DeploymentKit.Exceptions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using System.Diagnostics;
using AzureApplicationInsights = Pulumi.AzureNative.ApplicationInsights;
using AzureOperationalInsights = Pulumi.AzureNative.OperationalInsights;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing monitoring resources (Log Analytics and Application Insights)
/// </summary>
public class MonitoringService(ILogger<MonitoringService> logger, IResourceNamingService namingService, ICorrelationIdService correlationIdService) : IMonitoringService
{
    private readonly ILogger<MonitoringService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IResourceNamingService _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));

    public Task<MonitoringOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        if (settings.Monitoring == null)
            throw new ArgumentException("Monitoring settings cannot be null");

        if (string.IsNullOrEmpty(settings.NamingPrefix))
            throw new ArgumentException("NamingPrefix cannot be null or empty");

        if (string.IsNullOrEmpty(settings.Environment))
            throw new ArgumentException("Environment cannot be null or empty");

        if (settings.Monitoring.LogRetentionDays is < 1 or > 365)
            throw new ArgumentException("LogRetentionDays must be between 1 and 365");

        // Use correlation ID from service instead of generating new one
        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        var stopwatch = Stopwatch.StartNew();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId,
            [LoggingConstants.PropertyNames.Service] = LoggingConstants.ServiceNames.MonitoringService,
            [LoggingConstants.PropertyNames.Environment] = settings.Environment
        });

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation(ServiceConstants.Monitoring.CreationStartMessage,
                settings.Environment, correlationId);

            var logAnalyticsName = _namingService.GenerateLogAnalyticsWorkspaceName(settings.NamingPrefix, settings.Environment);
            var appInsightsName = _namingService.GenerateApplicationInsightsName(settings.NamingPrefix, settings.Environment);

            _logger.LogDebug(ServiceConstants.Monitoring.ResourceNamesGeneratedMessage,
                logAnalyticsName, appInsightsName, correlationId);

            // Create Log Analytics Workspace
            var logAnalyticsStopwatch = Stopwatch.StartNew();
            _logger.LogInformation(ServiceConstants.Monitoring.LogAnalyticsCreationMessage, logAnalyticsName, correlationId);

            var logAnalyticsWorkspace = CreateLogAnalyticsWorkspace(settings, resourceGroup, logAnalyticsName, correlationId);

            _logger.LogInformation(ServiceConstants.Monitoring.LogAnalyticsCreatedMessage, logAnalyticsStopwatch.ElapsedMilliseconds, correlationId);

            // Create Application Insights
            var appInsightsStopwatch = Stopwatch.StartNew();
            _logger.LogInformation(ServiceConstants.Monitoring.AppInsightsCreationMessage, appInsightsName, correlationId);

            var appInsights = CreateApplicationInsights(settings, resourceGroup, appInsightsName, logAnalyticsWorkspace, correlationId);

            _logger.LogInformation(ServiceConstants.Monitoring.AppInsightsCreatedMessage, appInsightsStopwatch.ElapsedMilliseconds, correlationId);

            // Get workspace shared keys
            var keysStopwatch = Stopwatch.StartNew();
            _logger.LogDebug(ServiceConstants.Monitoring.WorkspaceKeysRetrievalMessage, correlationId);

            var workspaceSharedKeys = AzureOperationalInsights.GetSharedKeys.Invoke(new AzureOperationalInsights.GetSharedKeysInvokeArgs
            {
                ResourceGroupName = resourceGroup,
                WorkspaceName = logAnalyticsWorkspace.Name
            });

            _logger.LogDebug(ServiceConstants.Monitoring.WorkspaceKeysRetrievedMessage, keysStopwatch.ElapsedMilliseconds, correlationId);

            var outputs = new MonitoringOutputs
            {
                LogAnalyticsWorkspaceId = logAnalyticsWorkspace.CustomerId,
                LogAnalyticsWorkspacePrimaryKey = Output.CreateSecret(workspaceSharedKeys.Apply(k => k.PrimarySharedKey ?? string.Empty)),
                LogAnalyticsWorkspaceName = logAnalyticsName,
                ApplicationInsightsId = appInsights.Id,
                ApplicationInsightsName = appInsightsName,
                ApplicationInsightsConnectionString = appInsights.ConnectionString,
                ApplicationInsightsInstrumentationKey = appInsights.InstrumentationKey
            };

            _logger.LogInformation(ServiceConstants.Monitoring.CreationSuccessMessage, stopwatch.ElapsedMilliseconds, logAnalyticsName, appInsightsName, correlationId);

            return Task.FromResult(outputs);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(ServiceConstants.Monitoring.CreationCancelledMessage, stopwatch.ElapsedMilliseconds, correlationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ServiceConstants.Monitoring.CreationFailedMessage,
                settings.Environment, stopwatch.ElapsedMilliseconds, correlationId);
            throw new ResourceCreationException(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Monitoring.EnvironmentCreationFailedFormat, settings.Environment),
                ex,
                ServiceConstants.ResourceTypes.Monitoring,
                ServiceConstants.ResourceTypes.LogAnalyticsApplicationInsights,
                settings.Environment,
                correlationId,
                ServiceConstants.ErrorCodes.MonitoringCreationFailed);
        }
    }

    private AzureOperationalInsights.Workspace CreateLogAnalyticsWorkspace(InfrastructureSettings settings, Input<string> resourceGroup, string logAnalyticsName, string correlationId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId,
            [LoggingConstants.PropertyNames.ResourceType] = ServiceConstants.ResourceTypes.LogAnalyticsWorkspace,
            [LoggingConstants.PropertyNames.ResourceName] = logAnalyticsName
        });

        try
        {
            _logger.LogDebug(ServiceConstants.Monitoring.LogAnalyticsConfigurationMessage,
                settings.Monitoring.LogRetentionDays, correlationId);

            var workspace = new AzureOperationalInsights.Workspace(logAnalyticsName, new AzureOperationalInsights.WorkspaceArgs
            {
                WorkspaceName = logAnalyticsName,
                ResourceGroupName = resourceGroup,
                Location = settings.Location,
                Sku = new AzureOperationalInsights.Inputs.WorkspaceSkuArgs
                {
                    Name = AzureOperationalInsights.WorkspaceSkuNameEnum.PerGB2018
                },
                RetentionInDays = settings.Monitoring.LogRetentionDays,
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, DeploymentConstants.ResourceTags.LogAnalyticsType)
            });

            _logger.LogDebug(ServiceConstants.Monitoring.LogAnalyticsConfiguredMessage, correlationId);
            return workspace;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ServiceConstants.Monitoring.LogAnalyticsCreationFailedMessage,
                logAnalyticsName, correlationId);
            throw;
        }
    }

    private AzureApplicationInsights.Component CreateApplicationInsights(InfrastructureSettings settings, Input<string> resourceGroup, string appInsightsName, AzureOperationalInsights.Workspace logAnalyticsWorkspace, string correlationId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId,
            [LoggingConstants.PropertyNames.ResourceType] = ServiceConstants.ResourceTypes.ApplicationInsights,
            [LoggingConstants.PropertyNames.ResourceName] = appInsightsName
        });

        try
        {
            _logger.LogDebug(ServiceConstants.Monitoring.AppInsightsConfigurationMessage, settings.Monitoring.ApplicationInsightsTypeString, correlationId);

            var appInsights = new AzureApplicationInsights.Component(appInsightsName, new AzureApplicationInsights.ComponentArgs
            {
                ResourceName = appInsightsName,
                ResourceGroupName = resourceGroup,
                Location = settings.Location,
                Kind = settings.Monitoring.ApplicationInsightsTypeString,
                ApplicationType = AzureApplicationInsights.ApplicationType.Web,
                WorkspaceResourceId = logAnalyticsWorkspace.Id,
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, DeploymentConstants.ResourceTags.ApplicationInsightsType)
            });

            _logger.LogDebug(ServiceConstants.Monitoring.AppInsightsConfiguredMessage, correlationId);
            return appInsights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ServiceConstants.Monitoring.AppInsightsCreationFailedMessage,
                appInsightsName, correlationId);
            throw;
        }
    }

    /// <summary>
    /// Explicit implementation of IInfrastructureService.CreateAsync
    /// </summary>
    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken) => await CreateAsync(settings, resourceGroup, cancellationToken);

    /// <summary>
    /// Explicit implementation of IInfrastructureService.CreateAsync without CancellationToken
    /// </summary>
    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup) => await CreateAsync(settings, resourceGroup);
}


