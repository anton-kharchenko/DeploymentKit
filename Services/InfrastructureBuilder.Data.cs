using DeploymentKit.Constants;
using DeploymentKit.Helpers;
using DeploymentKit.Interfaces;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Partial class for InfrastructureBuilder containing data-related resource configuration methods.
/// </summary>
public partial class InfrastructureBuilder
{
    private bool _addDatabase;
    private DatabaseSettings? _databaseSettings;

    private bool _addCosmosDb;
    private CosmosDbSettings? _cosmosDbSettings;

    /// <summary>
    /// Adds PostgreSQL Database with default settings.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddDatabase()
    {
        _addDatabase = true;
        _databaseSettings = InfrastructureDefaultSettingsFactory.GetDefaultDatabaseSettings();
        _logger.LogInformation(BuilderConstants.LoggingMessages.DatabaseDefault);
        return this;
    }

    /// <summary>
    /// Adds PostgreSQL Database with custom settings.
    /// </summary>
    /// <param name="databaseSettings">Custom database settings.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddDatabase(DatabaseSettings databaseSettings)
    {
        _addDatabase = true;
        _databaseSettings = databaseSettings ?? throw new ArgumentNullException(nameof(databaseSettings));
        _logger.LogInformation(BuilderConstants.LoggingMessages.DatabaseCustom);
        return this;
    }

    /// <summary>
    /// Adds Cosmos DB with default settings.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddCosmosDb()
    {
        _addCosmosDb = true;
        _cosmosDbSettings = InfrastructureDefaultSettingsFactory.GetDefaultCosmosDbSettings();
        _logger.LogInformation(BuilderConstants.LoggingMessages.CosmosDbDefault);
        return this;
    }

    /// <summary>
    /// Adds Cosmos DB with custom settings.
    /// </summary>
    /// <param name="cosmosDbSettings">Custom Cosmos DB settings.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddCosmosDb(CosmosDbSettings cosmosDbSettings)
    {
        _addCosmosDb = true;
        _cosmosDbSettings = cosmosDbSettings ?? throw new ArgumentNullException(nameof(cosmosDbSettings));
        _logger.LogInformation(BuilderConstants.LoggingMessages.CosmosDbCustom);
        return this;
    }
}

