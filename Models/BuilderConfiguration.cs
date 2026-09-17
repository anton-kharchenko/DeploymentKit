using DeploymentKit.Enums;
using DeploymentKit.Settings;

namespace DeploymentKit.Models;

/// <summary>
/// Represents the configuration state of the InfrastructureBuilder.
/// </summary>
public record BuilderConfiguration
{
    public string? DeploymentName { get; init; }
    public string? Environment { get; init; }
    public string? Location { get; init; }
    public string? SubscriptionId { get; init; }
    public string? ResourceGroupName { get; init; }
    public string? NamingPrefix { get; init; }

    public bool AddDatabase { get; init; }
    public DatabaseSettings? DatabaseSettings { get; init; }

    public bool AddCosmosDb { get; init; }
    public CosmosDbSettings? CosmosDbSettings { get; init; }

    public bool ConfigureMigrations { get; init; }
    public MigrationSettings? MigrationSettings { get; init; }

    public bool AddNetworking { get; init; }
    public NetworkSettings? NetworkSettings { get; init; }

    public bool AddApplicationGateway { get; init; }
    public ApplicationGatewaySettings? ApplicationGatewaySettings { get; init; }
    
    public bool ConfigureCustomDomain { get; init; }
    public CustomDomainSettings? CustomDomainSettings { get; init; }

    public bool AddMessageBroker { get; init; }
    public EventHubsSettings? EventHubsSettings { get; init; }

    public bool AddInsights { get; init; }
    public MonitoringSettings? MonitoringSettings { get; init; }

    public bool AddContainerApps { get; init; }
    public ContainerSettings? ContainerSettings { get; init; }

    public bool EnableGreenBlueDeployment { get; init; }
    public GreenBlueDeploymentSettings? GreenBlueSettings { get; init; }

    public bool AddKeyVault { get; init; }
    public KeyVaultSettings? KeyVaultSettings { get; init; }

    public bool AddRedis { get; init; }
    public CacheSettings? CacheSettings { get; init; }

    public bool AddStorage { get; init; }
    public StorageSettings? StorageSettings { get; init; }

    public bool AddBlobStorage { get; init; }
    public BlobStorageSettings? BlobStorageSettings { get; init; }

    public bool AddTableStorage { get; init; }
    public TableStorageSettings? TableStorageSettings { get; init; }

    public ValidationMode ValidationMode { get; init; } = ValidationMode.Basic;
    public bool SkipAzureAuthValidation { get; init; }
}

