using DeploymentKit.Components;
using DeploymentKit.Constants;
using DeploymentKit.Exceptions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.DBforPostgreSQL;
using Pulumi.AzureNative.DBforPostgreSQL.Inputs;
using System.Diagnostics;
using Configuration = Pulumi.AzureNative.DBforPostgreSQL.Configuration;
using ConfigurationArgs = Pulumi.AzureNative.DBforPostgreSQL.ConfigurationArgs;
using Database = Pulumi.AzureNative.DBforPostgreSQL.Database;
using DatabaseArgs = Pulumi.AzureNative.DBforPostgreSQL.DatabaseArgs;
using Server = Pulumi.AzureNative.DBforPostgreSQL.Server;
using ServerArgs = Pulumi.AzureNative.DBforPostgreSQL.ServerArgs;

namespace DeploymentKit.Services;

/// <summary>
/// Service for creating and managing PostgreSQL database resources
/// </summary>
public class DatabaseService(ILogger<DatabaseService> logger, ICorrelationIdService correlationIdService, IResourceNamingService namingService) : IDatabaseService
{
    private readonly ILogger<DatabaseService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
    private readonly IResourceNamingService _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
    
    public Task<DatabaseOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        var componentName = $"{settings.NamingPrefix}-database-{settings.Environment}";
        return DeploymentKitDatabase.CreateAsync(componentName, () => CreateCoreAsync(settings, resourceGroup, cancellationToken));
    }

    Task<DatabaseOutputs> IDatabaseService.CreateAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        NetworkOutputs? network,
        CancellationToken cancellationToken) =>
        CreateAsync(settings, resourceGroup, cancellationToken);

    private Task<DatabaseOutputs> CreateCoreAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        if (string.IsNullOrEmpty(settings.NamingPrefix))
            throw new ArgumentException("NamingPrefix cannot be null or empty");

        if (string.IsNullOrEmpty(settings.Environment))
            throw new ArgumentException("Environment cannot be null or empty");

        // If Database settings are not provided, skip PostgreSQL provisioning
        if (settings.Database == null)
        {
            _logger.LogInformation("Database settings not provided. Skipping PostgreSQL provisioning.");
            return Task.FromResult(new DatabaseOutputs
            {
                ServerName = string.Empty,
                DatabaseName = string.Empty,
                HostName = Output.Create(string.Empty),
                FullyQualifiedDomainName = Output.Create(string.Empty),
                ConnectionString = Output.CreateSecret(string.Empty),
                Password = Output.CreateSecret(string.Empty),
                AdminUsername = string.Empty
            });
        }

        if (string.IsNullOrWhiteSpace(settings.Database.Password))
            throw new ArgumentException($"Database password cannot be null or empty. Please set the {EnvironmentVariableNames.Database.AdminPassword} environment variable.");

        if (settings.Database.Password.Length < 8)
            throw new ArgumentException($"Database password must be at least 8 characters long. Current length: {settings.Database.Password.Length}");

        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        var stopwatch = Stopwatch.StartNew();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId,
            [LoggingConstants.PropertyNames.Environment] = settings.Environment,
            [LoggingConstants.PropertyNames.Service] = LoggingConstants.ServiceNames.DatabaseService,
            [LoggingConstants.PropertyNames.Operation] = ServiceConstants.ServiceOperations.CreatePostgreSqlInfrastructure
        });

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(ServiceConstants.Database.CreationStartMessage, settings.Environment, correlationId, settings.Database.VersionString, settings.Database.SkuNameString, settings.Database.StorageSizeGb);

            var serverName = _namingService.GeneratePostgreSqlServerName(settings.NamingPrefix, settings.Environment);
            var databaseName = _namingService.GeneratePostgreSqlDatabaseName(settings.NamingPrefix, settings.Environment);

            _logger.LogDebug(ServiceConstants.Database.GeneratedResourceNamesMessage, correlationId, serverName, databaseName);

            // Create PostgreSQL Server
            var serverStopwatch = Stopwatch.StartNew();
            var postgresServer = CreatePostgreSqlServer(settings, resourceGroup, serverName, correlationId);
            _logger.LogInformation(ServiceConstants.Database.ServerCreationInitiatedMessage, serverName, serverStopwatch.ElapsedMilliseconds, correlationId);

            // Configure Server Parameters (SSL, etc.)
            CreateServerConfigurations(resourceGroup, postgresServer, serverName);

            // Create Database
            var databaseStopwatch = Stopwatch.StartNew();
            var database = CreateDatabase(resourceGroup, postgresServer, databaseName, correlationId);
            _logger.LogInformation(ServiceConstants.Database.DatabaseCreationInitiatedMessage, databaseName, databaseStopwatch.ElapsedMilliseconds, correlationId);

            // Generate connection string
            var connectionString = Output.CreateSecret(
                Output.Format($"Host={postgresServer.FullyQualifiedDomainName};Database={database.Name};Username={settings.Database.AdminUser};Password={settings.Database.Password};SSL Mode=Require;"));

            var outputs = new DatabaseOutputs
            {
                ServerName = serverName,
                DatabaseName = databaseName,
                HostName = postgresServer.FullyQualifiedDomainName,
                FullyQualifiedDomainName = postgresServer.FullyQualifiedDomainName,
                ConnectionString = connectionString,
                Password = Output.CreateSecret(settings.Database.Password),
                AdminUsername = settings.Database.AdminUser
            };

            stopwatch.Stop();
            _logger.LogInformation(ServiceConstants.Database.CreationSuccessMessage, settings.Environment, stopwatch.ElapsedMilliseconds, correlationId, serverName, databaseName, settings.Database.SkuNameString);

            return Task.FromResult(outputs);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(ServiceConstants.Database.CreationCancelledMessage, settings.Environment, stopwatch.ElapsedMilliseconds, correlationId);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, ServiceConstants.Database.CreationFailedMessage, settings.Environment, stopwatch.ElapsedMilliseconds, correlationId, ex.Message);

            throw new ResourceCreationException(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Database.ResourceCreationFailedMessage, settings.Environment, correlationId),
                ex,
                ServiceConstants.ResourceTypes.Database,
                ServiceConstants.ResourceTypes.PostgreSQL,
                settings.Environment,
                correlationId,
                ServiceConstants.ErrorCodes.DatabaseCreationFailed);
        }
    }

    private Server CreatePostgreSqlServer(InfrastructureSettings settings, Input<string> resourceGroup, string serverName, string correlationId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.ResourceType] = ServiceConstants.ResourceTypes.PostgreSQLServer,
            [LoggingConstants.PropertyNames.ServerName] = serverName,
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId
        });

        try
        {
            _logger.LogInformation(ServiceConstants.Database.ServerCreationStartMessage, serverName, settings.Location, correlationId, settings.Database.VersionString, SkuTier.Burstable, settings.Database.AvailabilityZone);

            var postgresServer = new Server(serverName, new ServerArgs
            {
                ServerName = serverName,
                ResourceGroupName = resourceGroup,
                Location = settings.Location,
                Version = settings.Database.VersionString,
                AdministratorLogin = settings.Database.AdminUser,
                AdministratorLoginPassword = Output.CreateSecret(settings.Database.Password),
                Storage = new StorageArgs
                {
                    StorageSizeGB = settings.Database.StorageSizeGb
                },
                AvailabilityZone = settings.Database.AvailabilityZone,
                Sku = new SkuArgs
                {
                    Name = settings.Database.SkuNameString,
                    Tier = SkuTier.Burstable
                },
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, DeploymentConstants.ResourceTags.PostgreSqlServerType, correlationId),
                AuthConfig = new AuthConfigArgs
                {
                    ActiveDirectoryAuth = "Enabled",
                    PasswordAuth = "Enabled"
                }
            }, ComponentResourceScope.CreateChildOptions(serverName));

            _logger.LogDebug(ServiceConstants.Database.ServerConfiguredMessage, serverName, correlationId, settings.Database.StorageSizeGb, settings.Database.AdminUser);

            return postgresServer;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Pulumi") || ex.Message.Contains("Deployment.Instance"))
        {
            _logger.LogWarning("Pulumi context exception in CreatePostgreSqlServer: {Message}", ex.Message);
            throw new ResourceCreationException(
                $"Pulumi context error creating PostgreSQL server {serverName}",
                ex,
                ServiceConstants.ResourceTypes.PostgreSQLServer,
                ServiceConstants.ResourceTypes.PostgreSQL,
                correlationId,
                correlationId,
                ServiceConstants.ErrorCodes.DatabaseCreationFailed);
        }
        catch (Exception ex) when (ex.Message.Contains("Pulumi") || ex.Message.Contains("Deployment.Instance"))
        {
            _logger.LogWarning("Pulumi context exception in CreatePostgreSqlServer: {Message}", ex.Message);
            throw new ResourceCreationException(
                $"Pulumi context error creating PostgreSQL server {serverName}",
                ex,
                ServiceConstants.ResourceTypes.PostgreSQLServer,
                ServiceConstants.ResourceTypes.PostgreSQL,
                correlationId,
                correlationId,
                ServiceConstants.ErrorCodes.DatabaseCreationFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ServiceConstants.Database.ServerCreationFailedMessage, serverName, correlationId, ex.Message);
            throw;
        }
    }

    private static void CreateServerConfigurations(Input<string> resourceGroup, Server postgresServer, string serverName)
    {
        // Enforce SSL/TLS
        _ = new Configuration($"{serverName}-require_secure_transport", new ConfigurationArgs
        {
            ResourceGroupName = resourceGroup,
            ServerName = postgresServer.Name,
            ConfigurationName = "require_secure_transport",
            Value = "ON",
            Source = "user-override"
        }, ComponentResourceScope.CreateChildOptions($"{serverName}-require_secure_transport"));

        _ = new Configuration($"{serverName}-ssl_min_protocol_version", new ConfigurationArgs
        {
            ResourceGroupName = resourceGroup,
            ServerName = postgresServer.Name,
            ConfigurationName = "ssl_min_protocol_version",
            Value = "TLSv1.2",
            Source = "user-override"
        }, ComponentResourceScope.CreateChildOptions($"{serverName}-ssl_min_protocol_version"));
    }

    private Database CreateDatabase(Input<string> resourceGroup, Server postgresServer, string databaseName, string correlationId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.ResourceType] = ServiceConstants.ResourceTypes.Database,
            [LoggingConstants.PropertyNames.DatabaseName] = databaseName,
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId
        });

        try
        {
            _logger.LogInformation(ServiceConstants.Database.DatabaseCreationStartMessage,
                databaseName, correlationId);

            // Note: Collation parameter is intentionally omitted due to known Azure API issues
            // (see: https://github.com/pulumi/pulumi-azure-native/issues/2916)
            // Azure will use the default collation (en_US.utf8) if not specified
            var database = new Database(databaseName, new DatabaseArgs
            {
                DatabaseName = databaseName,
                ResourceGroupName = resourceGroup,
                ServerName = postgresServer.Name,
                Charset = DeploymentConstants.Database.Utf8Charset
            }, ComponentResourceScope.CreateChildOptions(databaseName));

            _logger.LogDebug(ServiceConstants.Database.DatabaseConfiguredMessage,
                databaseName, correlationId);

            return database;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Pulumi") || ex.Message.Contains("Deployment.Instance"))
        {
            _logger.LogWarning("Pulumi context exception in CreateDatabase: {Message}", ex.Message);
            throw new ResourceCreationException(
                $"Pulumi context error creating database {databaseName}",
                ex,
                ServiceConstants.ResourceTypes.Database,
                ServiceConstants.ResourceTypes.PostgreSQL,
                correlationId,
                correlationId,
                ServiceConstants.ErrorCodes.DatabaseCreationFailed);
        }
        catch (Exception ex) when (ex.Message.Contains("Pulumi") || ex.Message.Contains("Deployment.Instance"))
        {
            _logger.LogWarning("Pulumi context exception in CreateDatabase: {Message}", ex.Message);
            throw new ResourceCreationException(
                $"Pulumi context error creating database {databaseName}",
                ex,
                ServiceConstants.ResourceTypes.Database,
                ServiceConstants.ResourceTypes.PostgreSQL,
                correlationId,
                correlationId,
                ServiceConstants.ErrorCodes.DatabaseCreationFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ServiceConstants.Database.DatabaseCreationFailedMessage,
                databaseName, correlationId, ex.Message);
            throw;
        }
    }

    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken) => await CreateAsync(settings, resourceGroup, cancellationToken);

    /// <summary>
    /// Explicit implementation of IInfrastructureService.CreateAsync without CancellationToken
    /// </summary>
    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup) => await CreateAsync(settings, resourceGroup);
}
