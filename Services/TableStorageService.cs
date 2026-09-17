using DeploymentKit.Constants;
using DeploymentKit.Exceptions;
using DeploymentKit.Extensions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using System.Diagnostics;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing Azure Table Storage resources
/// </summary>
public class TableStorageService(ILogger<TableStorageService> logger, IResourceNamingService namingService, ICorrelationIdService correlationIdService) : ITableStorageService
{
    private readonly ILogger<TableStorageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IResourceNamingService _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));

    public Task<TableStorageOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default)
    {
        // Argument validation - must be done before try-catch to avoid wrapping in ResourceCreationException
        ArgumentNullException.ThrowIfNull(settings);
        
        ArgumentNullException.ThrowIfNull(resourceGroup);

        if (string.IsNullOrEmpty(settings.NamingPrefix))
            throw new ArgumentException("NamingPrefix cannot be null or empty");

        if (string.IsNullOrEmpty(settings.Environment))
            throw new ArgumentException("Environment cannot be null or empty");

        if (settings.TableStorage == null)
            throw new ArgumentException("TableStorage settings cannot be null");

        if (settings.TableStorage.TableNames == null || !settings.TableStorage.TableNames.Any())
            throw new ArgumentException("TableNames cannot be null or empty", nameof(settings));

        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        var stopwatch = Stopwatch.StartNew();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId,
            [LoggingConstants.PropertyNames.Environment] = settings.Environment,
            [LoggingConstants.PropertyNames.Service] = "TableStorageService",
            [LoggingConstants.PropertyNames.Operation] = "CreateTableStorage"
        });

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Starting Table Storage creation for environment {Environment} with correlation ID {CorrelationId}. Tables: {TableCount}, Encryption: {Encryption}",
                settings.Environment, correlationId, settings.TableStorage.TableNames.Count, settings.TableStorage.EnableEncryption);

            var storageAccountName = _namingService.GenerateStorageAccountName(settings.NamingPrefix, settings.Environment);

            _logger.LogDebug("Generated storage account name {StorageAccountName} for correlation ID {CorrelationId}",
                storageAccountName, correlationId);

            // Create Storage Account with Table-specific configuration
            var storageStopwatch = Stopwatch.StartNew();
            var storageAccount = CreateTableStorageAccount(settings, resourceGroup, storageAccountName);
            _logger.LogInformation("Table Storage account creation initiated for {StorageAccountName} in {ElapsedMs}ms with correlation ID {CorrelationId}",
                storageAccountName, storageStopwatch.ElapsedMilliseconds, correlationId);

            // Create Tables
            var tablesStopwatch = Stopwatch.StartNew();
            var tables = CreateTables(settings, resourceGroup, storageAccount);
            _logger.LogInformation("Tables creation initiated in {ElapsedMs}ms with correlation ID {CorrelationId}",
                tablesStopwatch.ElapsedMilliseconds, correlationId);

            // Get Storage Account Keys
            var keysStopwatch = Stopwatch.StartNew();
            var storageAccountKeys = GetStorageAccountKeys(resourceGroup, storageAccount);
            _logger.LogInformation("Storage account keys retrieved in {ElapsedMs}ms with correlation ID {CorrelationId}",
                keysStopwatch.ElapsedMilliseconds, correlationId);

            stopwatch.Stop();
            _logger.LogInformation("Table Storage creation completed successfully for environment {Environment} in {ElapsedMs}ms with correlation ID {CorrelationId}",
                settings.Environment, stopwatch.ElapsedMilliseconds, correlationId);

            var primaryKey = Output.CreateSecret(storageAccountKeys.Apply(keys => keys.Keys[0].Value));
            var connectionString = Output.CreateSecret(
                Output.Format($"DefaultEndpointsProtocol=https;AccountName={storageAccount.Name};AccountKey={primaryKey};EndpointSuffix=core.windows.net"));

            return Task.FromResult(new TableStorageOutputs
            {
                Name = storageAccountName,
                AccountName = storageAccount.Name,
                PrimaryKey = primaryKey,
                ConnectionString = connectionString,
                TableEndpoint = Output.Format($"https://{storageAccount.Name}.table.core.windows.net/"),
                PrimaryTableEndpoint = Output.Format($"https://{storageAccount.Name}.table.core.windows.net/"),
                TableNames = settings.TableStorage.TableNames,
                TableUrls = tables.ToDictionary(
                    kvp => kvp.Key,
                    kvp => Output.Format($"https://{storageAccount.Name}.table.core.windows.net/{kvp.Key}")
                )
            });
        }
        catch (ArgumentException ex) when (ex.Message.Contains("resourceGroup") ||
                                           ex.Message.Contains("Resource group") ||
                                           ex.Message.Contains("empty") ||
                                           ex.Message.Contains("null"))
        {
            _logger.LogWarning("Invalid resource group parameter provided for Table Storage creation in environment {Environment} with correlation ID {CorrelationId}: {ErrorMessage}",
                settings.Environment, correlationId, ex.Message);
            throw new ArgumentNullException(nameof(resourceGroup), "Resource group cannot be null, empty, or whitespace");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Table Storage creation cancelled for environment {Environment} after {ElapsedMs}ms with correlation ID {CorrelationId}",
                settings.Environment, stopwatch.ElapsedMilliseconds, correlationId);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Table Storage creation failed for environment {Environment} after {ElapsedMs}ms with correlation ID {CorrelationId}: {ErrorMessage}",
                settings.Environment, stopwatch.ElapsedMilliseconds, correlationId, ex.Message);

            throw new ResourceCreationException(
                $"Failed to create Table Storage infrastructure for environment {settings.Environment} with correlation ID {correlationId}",
                ex,
                "TableStorage",
                "StorageAccount",
                correlationId);
        }
    }

    private static StorageAccount CreateTableStorageAccount(InfrastructureSettings settings, Input<string> resourceGroup, string storageAccountName)
    {
        var tags = ResourceTagHelper.GetStandardTags(settings.Environment, DeploymentConstants.ResourceTags.StorageAccountType);

        return new StorageAccount(storageAccountName, new StorageAccountArgs
        {
            ResourceGroupName = resourceGroup,
            AccountName = storageAccountName,
            Location = settings.Location,
            Kind = Kind.StorageV2,
            Sku = new SkuArgs
            {
                Name = settings.Storage?.ReplicationType.ToStringValue() ?? StorageConstants.StandardLrs
            },
            EnableHttpsTrafficOnly = settings.Storage?.EnableHttpsTrafficOnly ?? true,
            AllowSharedKeyAccess = settings.Storage?.AllowSharedKeyAccess ?? true,
            MinimumTlsVersion = settings.Storage?.MinimumTlsVersion.ToStringValue() ?? "TLS1_2",
            Encryption = (settings.TableStorage!.EnableEncryption ? new EncryptionArgs
            {
                Services = new EncryptionServicesArgs
                {
                    Table = new EncryptionServiceArgs
                    {
                        Enabled = true
                    }
                },
                KeySource = KeySource.Microsoft_Storage
            } : null) ?? throw new InvalidOperationException(),
            Tags = tags
        });
    }

    private static Dictionary<string, Table> CreateTables(InfrastructureSettings settings, Input<string> resourceGroup, StorageAccount storageAccount)
    {
        var tables = new Dictionary<string, Table>();

        foreach (var tableName in settings.TableStorage!.TableNames)
        {
            var table = new Table($"{storageAccount.Name}-{tableName}", new TableArgs
            {
                ResourceGroupName = resourceGroup,
                AccountName = storageAccount.Name,
                TableName = tableName
            });

            tables[tableName] = table;
        }

        return tables;
    }

    private static Output<ListStorageAccountKeysResult> GetStorageAccountKeys(Input<string> resourceGroup, StorageAccount storageAccount)
    {
        return ListStorageAccountKeys.Invoke(new ListStorageAccountKeysInvokeArgs
        {
            ResourceGroupName = resourceGroup,
            AccountName = storageAccount.Name
        });
    }

    /// <summary>
    /// Explicit implementation of IInfrastructureService.CreateAsync with CancellationToken
    /// </summary>
    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken) => await CreateAsync(settings, resourceGroup, cancellationToken);

    /// <summary>
    /// Explicit implementation of IInfrastructureService.CreateAsync without CancellationToken
    /// </summary>
    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup) => await CreateAsync(settings, resourceGroup);
}

