using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using DeploymentKit.Settings;

namespace DeploymentKit.Helpers;

/// <summary>
/// Helper class for building infrastructure settings.
/// </summary>
public static class InfrastructureSettingsComposer
{
    /// <summary>
    /// Builds the infrastructure settings.
    /// </summary>
    /// <param name="deploymentName">The name of the deployment.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="location">The Azure location.</param>
    /// <param name="subscriptionId">The Azure subscription ID.</param>
    /// <param name="resourceGroupName">The resource group name.</param>
    /// <param name="namingPrefix">The naming prefix for resources.</param>
    /// <param name="validationMode">The validation mode.</param>
    /// <param name="skipAzureAuthValidation">Whether to skip Azure authentication validation.</param>
    /// <param name="addDatabase">Whether to add a database.</param>
    /// <param name="databaseSettings">The database settings.</param>
    /// <param name="addContainerApps">Whether to add container apps.</param>
    /// <param name="containerSettings">The container settings.</param>
    /// <param name="addInsights">Whether to add application insights.</param>
    /// <param name="monitoringSettings">The monitoring settings.</param>
    /// <param name="addRedis">Whether to add Redis cache.</param>
    /// <param name="cacheSettings">The cache settings.</param>
    /// <param name="addMessageBroker">Whether to add message broker.</param>
    /// <param name="eventHubsSettings">The event hubs settings.</param>
    /// <param name="addNetworking">Whether to add networking.</param>
    /// <param name="networkSettings">The network settings.</param>
    /// <param name="addKeyVault">Whether to add Key Vault.</param>
    /// <param name="keyVaultSettings">The Key Vault settings.</param>
    /// <param name="addStorage">Whether to add storage.</param>
    /// <param name="storageSettings">The storage settings.</param>
    /// <param name="addBlobStorage">Whether to add blob storage.</param>
    /// <param name="blobStorageSettings">The blob storage settings.</param>
    /// <param name="addCosmosDb">Whether to add Cosmos DB.</param>
    /// <param name="cosmosDbSettings">The Cosmos DB settings.</param>
    /// <param name="addTableStorage">Whether to add table storage.</param>
    /// <param name="tableStorageSettings">The table storage settings.</param>
    /// <param name="addApplicationGateway">Whether to add Application Gateway.</param>
    /// <param name="applicationGatewaySettings">The Application Gateway settings.</param>
    /// <param name="enableGreenBlueDeployment">Whether to enable green-blue deployment.</param>
    /// <param name="greenBlueSettings">The green-blue deployment settings.</param>
    /// <param name="configureMigrations">Whether to configure migrations.</param>
    /// <param name="migrationSettings">The migration settings.</param>
    /// <param name="configureCustomDomain">Whether to configure custom domain.</param>
    /// <param name="customDomainSettings">The custom domain settings.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>The constructed infrastructure settings.</returns>
    public static InfrastructureSettings BuildSettings(
        string deploymentName,
        string environment,
        string location,
        string subscriptionId,
        string resourceGroupName,
        string namingPrefix,
        ValidationMode validationMode,
        bool skipAzureAuthValidation,
        // Resource flags and settings
        bool addDatabase, DatabaseSettings? databaseSettings,
        bool addContainerApps, ContainerSettings? containerSettings,
        bool addInsights, MonitoringSettings? monitoringSettings,
        bool addRedis, CacheSettings? cacheSettings,
        bool addMessageBroker, EventHubsSettings? eventHubsSettings,
        bool addNetworking, NetworkSettings? networkSettings,
        bool addKeyVault, KeyVaultSettings? keyVaultSettings,
        bool addStorage, StorageSettings? storageSettings,
        bool addBlobStorage, BlobStorageSettings? blobStorageSettings,
        bool addCosmosDb, CosmosDbSettings? cosmosDbSettings,
        bool addTableStorage, TableStorageSettings? tableStorageSettings,
        bool addApplicationGateway, ApplicationGatewaySettings? applicationGatewaySettings,
        bool enableGreenBlueDeployment, GreenBlueDeploymentSettings? greenBlueSettings,
        bool configureMigrations, MigrationSettings? migrationSettings,
        bool configureCustomDomain, CustomDomainSettings? customDomainSettings,
        ILogger logger)
    {
        // Map environment names to validation-compliant values
        var mappedEnvironment = InfrastructureValidationHelper.MapEnvironmentName(environment);

        // Use provided naming prefix or derive from deployment name (sanitized)
        var finalNamingPrefix = namingPrefix;

        // Generate default resource group name using the consistent naming pattern: {prefix}-rg-{environment}
        var finalResourceGroupName = resourceGroupName;

        // Build the infrastructure settings
        var settings = new InfrastructureSettings
        {
            Environment = mappedEnvironment,
            Location = location,
            SubscriptionId = subscriptionId,
            ResourceGroupName = finalResourceGroupName,
            NamingPrefix = finalNamingPrefix.ToLowerInvariant(),
            ValidationMode = validationMode,
            SkipAzureAuthValidation = skipAzureAuthValidation,
            Database = addDatabase ? databaseSettings : null,
            Container = addContainerApps ? containerSettings : null,
            Monitoring = addInsights ? monitoringSettings : null,
            Cache = addRedis ? cacheSettings : null,
            EventHubs = addMessageBroker ? eventHubsSettings : null,
            Network = addNetworking ? networkSettings : null,
            KeyVault = addKeyVault ? keyVaultSettings : null,
            Storage = addStorage ? storageSettings : null,
            BlobStorage = addBlobStorage ? blobStorageSettings : null,
            CosmosDb = addCosmosDb ? cosmosDbSettings : null,
            TableStorage = addTableStorage ? tableStorageSettings : null,
            ApplicationGateway = addApplicationGateway ? applicationGatewaySettings : null,
            GreenBlueDeployment = enableGreenBlueDeployment ? greenBlueSettings : null,
            GreenSlot = enableGreenBlueDeployment ? new SlotSettings { SlotName = DeploymentSlotType.Green.ToStringValue() } : null,
            BlueSlot = enableGreenBlueDeployment ? new SlotSettings { SlotName = DeploymentSlotType.Blue.ToStringValue() } : null,
            Migration = configureMigrations ? migrationSettings : null,
            CustomDomain = configureCustomDomain ? customDomainSettings : null
        };

        logger.LogInformation(BuilderConstants.Logs.InfrastructureBuilt, deploymentName);
        return settings;
    }
}


