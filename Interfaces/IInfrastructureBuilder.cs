using DeploymentKit.Enums;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Builder interface for configuring DeploymentKit infrastructure deployment with fluent API
/// Provides a clean, chainable interface for setting up various Azure resources
/// </summary>
public interface IInfrastructureBuilder
{
    /// <summary>
    /// Sets the deployment name and validates its availability
    /// </summary>
    /// <param name="deploymentName">The deployment name to set</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentException">Thrown when name is invalid or already occupied</exception>
    IInfrastructureBuilder SetName(string deploymentName);

    /// <summary>
    /// Sets the target environment for deployment
    /// </summary>
    /// <param name="environment">The environment (dev, staging, prod)</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder SetEnvironment(string environment);

    /// <summary>
    /// Sets the target environment for deployment using typed enum
    /// </summary>
    /// <param name="environment">The environment type</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder SetEnvironment(EnvironmentType environment);

    /// <summary>
    /// Sets the Azure location for resource deployment
    /// </summary>
    /// <param name="location">The Azure region location</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder SetLocation(string location);

    /// <summary>
    /// Sets the Azure locationType for resource deployment using typed enum
    /// </summary>
    /// <param name="locationType">The Azure region</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder SetLocation(AzureLocationType locationType);

    /// <summary>
    /// Sets the Azure subscription ID for resource deployment
    /// </summary>
    /// <param name="subscriptionId">The Azure subscription ID (GUID format)</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder SetSubscriptionId(string subscriptionId);

    /// <summary>
    /// Sets the resource group name for infrastructure deployment
    /// </summary>
    /// <param name="resourceGroupName">The resource group name</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder SetResourceGroupName(string resourceGroupName);

    /// <summary>
    /// Sets the naming prefix for all infrastructure resources
    /// </summary>
    /// <param name="namingPrefix">The naming prefix (lowercase alphanumeric only)</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder SetNamingPrefix(string namingPrefix);

    /// <summary>
    /// Adds Key Vault to the infrastructure with secrets loaded from a .env file
    /// </summary>
    /// <param name="envFilePath">Path to the .env file containing secrets</param>
    /// <param name="excludePatterns">Patterns to exclude from Key Vault (e.g., "TEMP_*", "DEBUG_*")</param>
    /// <param name="customSettings">Optional custom Key Vault settings</param>
    /// <param name="applyToContainerApps">Automatically configure all Key Vault secrets as environment variables in Container Apps</param>
    /// <returns>Builder instance for method chaining</returns>
    IInfrastructureBuilder AddKeyVault(string envFilePath, IEnumerable<string>? excludePatterns = null, KeyVaultSettings? customSettings = null, bool applyToContainerApps = false);

    /// <summary>
    /// Adds Key Vault to the infrastructure with custom settings
    /// </summary>
    /// <param name="keyVaultSettings">Custom Key Vault settings</param>
    /// <returns>Builder instance for method chaining</returns>
    IInfrastructureBuilder AddKeyVault(KeyVaultSettings keyVaultSettings);

    /// <summary>
    /// Adds Key Vault to the infrastructure with default settings
    /// </summary>
    /// <returns>Builder instance for method chaining</returns>
    IInfrastructureBuilder AddKeyVault();

    /// <summary>
    /// Adds Redis cache to the infrastructure
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddRedis();

    /// <summary>
    /// Adds Redis cache to the infrastructure with custom settings
    /// </summary>
    /// <param name="cacheSettings">Custom cache configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddRedis(CacheSettings cacheSettings);

    /// <summary>
    /// Adds message broker (Event Hubs) to the infrastructure
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddMessageBroker();

    /// <summary>
    /// Adds message broker (Event Hubs) to the infrastructure with custom settings
    /// </summary>
    /// <param name="eventHubsSettings">Custom Event Hubs configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddMessageBroker(EventHubsSettings eventHubsSettings);

    /// <summary>
    /// Adds Application Insights monitoring to the infrastructure
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddInsights();

    /// <summary>
    /// Adds Application Insights monitoring to the infrastructure with custom settings
    /// </summary>
    /// <param name="monitoringSettings">Custom monitoring configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddInsights(MonitoringSettings monitoringSettings);

    /// <summary>
    /// Adds PostgreSQL database to the infrastructure
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddDatabase();

    /// <summary>
    /// Adds PostgreSQL database to the infrastructure with custom settings
    /// </summary>
    /// <param name="databaseSettings">Custom database configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddDatabase(DatabaseSettings databaseSettings);

    /// <summary>
    /// Adds container registry to the infrastructure
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddContainerRegistry();

    /// <summary>
    /// Adds storage account to the infrastructure
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddStorage();

    /// <summary>
    /// Adds storage account to the infrastructure with custom settings
    /// </summary>
    /// <param name="storageSettings">Custom storage configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddStorage(StorageSettings storageSettings);

    /// <summary>
    /// Adds Azure Blob Storage to the infrastructure
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddBlobStorage();

    /// <summary>
    /// Adds Azure Blob Storage to the infrastructure with custom settings
    /// </summary>
    /// <param name="blobStorageSettings">Custom blob storage configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddBlobStorage(BlobStorageSettings blobStorageSettings);

    /// <summary>
    /// Adds Azure Cosmos DB to the infrastructure
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddCosmosDb();

    /// <summary>
    /// Adds Azure Cosmos DB to the infrastructure with custom settings
    /// </summary>
    /// <param name="cosmosDbSettings">Custom Cosmos DB configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddCosmosDb(CosmosDbSettings cosmosDbSettings);

    /// <summary>
    /// Adds Azure Table Storage to the infrastructure
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddTableStorage();

    /// <summary>
    /// Adds Azure Table Storage to the infrastructure with custom settings
    /// </summary>
    /// <param name="tableStorageSettings">Custom table storage configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddTableStorage(TableStorageSettings tableStorageSettings);

    /// <summary>
    /// Adds container apps to the infrastructure
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddContainerApps();

    /// <summary>
    /// Adds container apps to the infrastructure with custom settings
    /// </summary>
    /// <param name="containerSettings">Custom container configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddContainerApps(ContainerSettings containerSettings);

    /// <summary>
    /// Adds networking infrastructure (VNet, subnets, private endpoints)
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddNetworking();

    /// <summary>
    /// Adds networking infrastructure with custom settings
    /// </summary>
    /// <param name="networkSettings">Custom network configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddNetworking(NetworkSettings networkSettings);

    /// <summary>
    /// Adds Application Gateway for external access
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddApplicationGateway();

    /// <summary>
    /// Adds Application Gateway for external access with custom settings
    /// </summary>
    /// <param name="applicationGatewaySettings">Custom Application Gateway configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddApplicationGateway(ApplicationGatewaySettings applicationGatewaySettings);

    /// <summary>
    /// Adds domain optimization (CDN, DNS, Traffic Manager)
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddDomainOptimization();

    /// <summary>
    /// Adds VPN Gateway for secure connectivity
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder AddVpn();

    /// <summary>
    /// Enables Green-Blue deployment strategy
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder EnableGreenBlueDeployment();

    /// <summary>
    /// Enables Green-Blue deployment strategy with custom settings
    /// </summary>
    /// <param name="greenBlueSettings">Custom Green-Blue deployment configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder EnableGreenBlueDeployment(GreenBlueDeploymentSettings greenBlueSettings);

    /// <summary>
    /// Configures database migrations with Entity Framework Core
    /// </summary>
    /// <param name="migrationAssembly">Assembly containing EF Core migrations (e.g., "MyApp.Data")</param>
    /// <param name="dbContextTypeName">Fully qualified DbContext type name (e.g., "MyApp.Data.ApplicationDbContext")</param>
    /// <param name="autoRunOnDeployment">Whether to automatically run migrations during deployment (default: false)</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder ConfigureDatabaseMigrations(string migrationAssembly, string dbContextTypeName, bool autoRunOnDeployment = false);

    /// <summary>
    /// Configures database migrations with custom settings
    /// </summary>
    /// <param name="migrationSettings">Custom migration configuration</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder ConfigureDatabaseMigrations(MigrationSettings migrationSettings);

    /// <summary>
    /// Configures database migrations using SQL scripts
    /// </summary>
    /// <param name="sqlScriptPath">Path to the SQL script file</param>
    /// <param name="autoRunOnDeployment">Whether to automatically run migrations during deployment (default: false)</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder ConfigureSqlMigrations(string sqlScriptPath, bool autoRunOnDeployment = false);

    /// <summary>
    /// Configures custom domain with automatic DNS and SSL certificate provisioning
    /// </summary>
    /// <param name="domainName">The custom domain name (e.g., api-dev.example.com)</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder SetCustomDomain(string domainName);

    /// <summary>
    /// Configures custom domain with advanced settings
    /// </summary>
    /// <param name="customDomainSettings">Custom domain settings with DNS, SSL, and validation options</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder ConfigureCustomDomain(CustomDomainSettings customDomainSettings);

    /// <summary>
    /// Configures SSL certificate from Azure Key Vault
    /// </summary>
    /// <param name="keyVaultCertificateName">Name of the certificate in Key Vault</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder UseKeyVaultCertificate(string keyVaultCertificateName);

    /// <summary>
    /// Configures SSL certificate upload from local .pfx file
    /// </summary>
    /// <param name="certificateFilePath">Path to .pfx certificate file</param>
    /// <param name="certificatePassword">Certificate password (optional)</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder UploadCertificate(string certificateFilePath, string? certificatePassword = null);

    /// <summary>
    /// Enables Azure Managed Certificate (Let's Encrypt) for the custom domain
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder UseManagedCertificate();

    /// <summary>
    /// Sets the validation mode for the deployment
    /// Full: All validation including Azure connectivity (default for production)
    /// Basic: Settings and naming validation only (recommended for initial deployments)
    /// Minimal: Only required fields validation
    /// Skip: No validation (not recommended)
    /// </summary>
    /// <param name="validationMode">The validation mode to use</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder SetValidationMode(Enums.ValidationMode validationMode);

    /// <summary>
    /// Skips Azure authentication validation - useful when using Azure CLI authentication instead of Service Principal
    /// </summary>
    /// <param name="skip">Whether to skip Azure authentication validation (default: true)</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder SkipAzureAuthValidation(bool skip = true);

    /// <summary>
    /// Uses existing infrastructure settings instead of builder configuration
    /// </summary>
    /// <param name="settings">Pre-configured infrastructure settings</param>
    /// <returns>The builder instance for method chaining</returns>
    IInfrastructureBuilder UseSettings(InfrastructureSettings settings);

    /// <summary>
    /// Builds the infrastructure settings from the configured options
    /// </summary>
    /// <returns>The configured infrastructure settings ready for deployment</returns>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing</exception>
    InfrastructureSettings Build();

    /// <summary>
    /// Asynchronously builds the infrastructure settings from the configured options, performing non-blocking I/O operations
    /// </summary>
    /// <returns>The configured infrastructure settings ready for deployment</returns>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing</exception>
    Task<InfrastructureSettings> BuildAsync();

    /// <summary>
    /// Validates the current builder configuration without building
    /// </summary>
    /// <returns>True if the configuration is valid, false otherwise</returns>
    bool Validate();

    /// <summary>
    /// Gets validation errors for the current configuration
    /// </summary>
    /// <returns>List of validation error messages</returns>
    IReadOnlyList<string> GetValidationErrors();
}
