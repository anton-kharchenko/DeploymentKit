using DeploymentKit.Exceptions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.EventHub;
using Pulumi.AzureNative.EventHub.Inputs;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing Azure Event Hubs infrastructure
/// </summary>
public class EventHubsService(ILogger<EventHubsService> logger, IResourceNamingService namingService) : IEventHubsService
{
    private readonly ILogger<EventHubsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IResourceNamingService _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));

    public EventHubsOutputs Outputs { get; private set; } = null!;

    public Task<EventHubsOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        if (string.IsNullOrEmpty(settings.NamingPrefix))
        {
            throw new ArgumentException("NamingPrefix cannot be null or empty");
        }

        if (string.IsNullOrEmpty(settings.Environment))
        {
            throw new ArgumentException("Environment cannot be null or empty");
        }

        try
        {
            _logger.LogInformation("Creating Event Hubs resources for environment: {Environment}", settings.Environment);

            // Check if EventHubs settings are provided
            if (settings.EventHubs == null)
            {
                _logger.LogWarning("EventHubs settings are null. Skipping Event Hubs creation.");
                Outputs = new EventHubsOutputs
                {
                    EventHubsNamespaceName = Output.Create(string.Empty),
                    EventHubsConnectionString = Output.CreateSecret(string.Empty),
                    EventHubsEndpoint = Output.Create(string.Empty),
                    EventHubsResourceId = Output.Create(string.Empty),
                    EventHubName = string.Empty,
                    ConsumerGroupName = string.Empty
                };
                return Task.FromResult(Outputs);
            }

            var namespaceName = _namingService.GenerateEventHubsNamespaceName(settings.NamingPrefix, settings.Environment);
            var eventHubName = _namingService.GenerateEventHubName(settings.NamingPrefix, settings.Environment);

            try
            {
                // Create Event Hubs Namespace
                var eventHubsNamespace = new Namespace(namespaceName, new NamespaceArgs
                {
                    NamespaceName = namespaceName,
                    ResourceGroupName = resourceGroup,
                    Location = settings.Location,
                    Sku = new SkuArgs
                    {
                        Name = settings.EventHubs.SkuNameString,
                        Tier = settings.EventHubs.SkuTierString,
                        Capacity = settings.EventHubs.Capacity
                    },
                    Tags = ResourceTagHelper.GetStandardTags(settings.Environment, "event-hubs")
                });

                // Create Event Hub
                var eventHub = new EventHub(eventHubName, new EventHubArgs
                {
                    EventHubName = eventHubName,
                    ResourceGroupName = resourceGroup,
                    NamespaceName = eventHubsNamespace.Name,
                    MessageRetentionInDays = settings.EventHubs.MessageRetentionInDays,
                    PartitionCount = settings.EventHubs.PartitionCount,
                    Status = EntityStatus.Active
                });

                // Create Consumer Group
                var consumerGroupName = "app-consumer-group";

                var namespaceKeys = ListNamespaceKeys.Invoke(new ListNamespaceKeysInvokeArgs
                {
                    ResourceGroupName = resourceGroup,
                    NamespaceName = eventHubsNamespace.Name,
                    AuthorizationRuleName = "RootManageSharedAccessKey"
                });

                var connectionString = Output.CreateSecret(
                    namespaceKeys.Apply(k => k.PrimaryConnectionString ?? string.Empty));

                Outputs = new EventHubsOutputs
                {
                    EventHubsNamespaceName = eventHubsNamespace.Name,
                    EventHubsConnectionString = connectionString,
                    EventHubsEndpoint = eventHubsNamespace.ServiceBusEndpoint,
                    EventHubsResourceId = eventHubsNamespace.Id,
                    EventHubName = eventHubName,
                    ConsumerGroupName = consumerGroupName
                };

                _logger.LogInformation("Successfully created Event Hubs resources: Namespace: {NamespaceName}, EventHub: {EventHubName}", namespaceName, eventHubName);

                return Task.FromResult(Outputs);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Pulumi") || ex.Message.Contains("Deployment.Instance"))
            {
                _logger.LogWarning("Pulumi context exception during EventHubs creation: {Message}", ex.Message);
                throw new ResourceCreationException("Pulumi context exception during EventHubs creation", ex);
            }
            catch (Exception ex) when (ex.Message.Contains("Pulumi") || ex.Message.Contains("Deployment.Instance"))
            {
                _logger.LogWarning("Pulumi context exception during EventHubs creation: {Message}", ex.Message);
                throw new ResourceCreationException("Pulumi context exception during EventHubs creation", ex);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Event Hubs resources for environment: {Environment}", settings.Environment);
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

