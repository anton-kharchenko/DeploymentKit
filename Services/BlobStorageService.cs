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
/// Service for managing Azure Blob Storage resources
/// </summary>
public class BlobStorageService(ILogger<BlobStorageService> logger, IResourceNamingService namingService, ICorrelationIdService correlationIdService) : IBlobStorageService
{
    private readonly ILogger<BlobStorageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IResourceNamingService _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));

    public Task<BlobStorageOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default)
    {
        // Argument validation - must be done before try-catch to avoid wrapping in ResourceCreationException
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        if (string.IsNullOrEmpty(settings.NamingPrefix))
            throw new ArgumentException("NamingPrefix cannot be null or empty");

        if (string.IsNullOrEmpty(settings.Environment))
            throw new ArgumentException("Environment cannot be null or empty");

        if (settings.BlobStorage == null)
            throw new ArgumentException("BlobStorage settings cannot be null");

        if (settings.BlobStorage.ContainerNames == null || !settings.BlobStorage.ContainerNames.Any())
            throw new ArgumentException("ContainerNames cannot be null or empty");

        try
        {
            var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
            var stopwatch = Stopwatch.StartNew();

            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                [LoggingConstants.PropertyNames.CorrelationId] = correlationId,
                [LoggingConstants.PropertyNames.Environment] = settings.Environment,
                [LoggingConstants.PropertyNames.Service] = "BlobStorageService",
                [LoggingConstants.PropertyNames.Operation] = "CreateBlobStorage"
            });
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Starting Blob Storage creation for environment {Environment} with correlation ID {CorrelationId}. Access tier: {AccessTier}, Versioning: {Versioning}",
                settings.Environment, correlationId, settings.BlobStorage.AccessTierString, settings.BlobStorage.EnableVersioning);

            var storageAccountName = _namingService.GenerateStorageAccountName(settings.NamingPrefix, settings.Environment);

            _logger.LogDebug("Generated storage account name {StorageAccountName} for correlation ID {CorrelationId}",
                storageAccountName, correlationId);

            // Create Storage Account with Blob-specific configuration
            var storageStopwatch = Stopwatch.StartNew();
            var storageAccount = CreateBlobStorageAccount(settings, resourceGroup, storageAccountName);
            _logger.LogInformation("Blob Storage account creation initiated for {StorageAccountName} in {ElapsedMs}ms with correlation ID {CorrelationId}",
                storageAccountName, storageStopwatch.ElapsedMilliseconds, correlationId);

            // Create Blob Containers
            var containersStopwatch = Stopwatch.StartNew();
            var containers = CreateBlobContainers(settings, resourceGroup, storageAccount);
            _logger.LogInformation("Blob containers creation initiated in {ElapsedMs}ms with correlation ID {CorrelationId}",
                containersStopwatch.ElapsedMilliseconds, correlationId);

            // Get Storage Account Keys
            var keysStopwatch = Stopwatch.StartNew();
            var storageAccountKeys = GetStorageAccountKeys(resourceGroup, storageAccount);
            _logger.LogInformation("Storage account keys retrieved in {ElapsedMs}ms with correlation ID {CorrelationId}",
                keysStopwatch.ElapsedMilliseconds, correlationId);

            // Generate connection string
            var primaryKey = Output.CreateSecret(storageAccountKeys.Apply(k => k.Keys[0].Value));
            var connectionString = Output.CreateSecret(
                Output.Tuple(storageAccount.Name, primaryKey)
                    .Apply(t => string.Format(System.Globalization.CultureInfo.InvariantCulture, DeploymentConstants.Storage.ConnectionStringTemplate, t.Item1, t.Item2)));

            // Generate container URLs
            var containerUrls = containers.ToDictionary(
                kvp => kvp.Key,
                kvp => Output.Format($"https://{storageAccount.Name}.blob.core.windows.net/{kvp.Value.Name}")
            );

            var outputs = new BlobStorageOutputs
            {
                Name = storageAccountName,
                AccountName = storageAccount.Name,
                PrimaryKey = primaryKey,
                ConnectionString = connectionString,
                BlobEndpoint = storageAccount.PrimaryEndpoints.Apply(e => e.Blob),
                PrimaryBlobEndpoint = storageAccount.PrimaryEndpoints.Apply(e => e.Blob),
                ContainerNames = settings.BlobStorage.ContainerNames,
                ContainerUrls = containerUrls
            };

            stopwatch.Stop();
            _logger.LogInformation("Blob Storage creation completed for environment {Environment} in {ElapsedMs}ms with correlation ID {CorrelationId}. Account: {StorageAccountName}, Access tier: {AccessTier}",
                settings.Environment, stopwatch.ElapsedMilliseconds, correlationId, storageAccountName, settings.BlobStorage.AccessTierString);

            return Task.FromResult(outputs);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("resourceGroup") ||
                                           ex.Message.Contains("Resource group") ||
                                           ex.Message.Contains("empty") ||
                                           ex.Message.Contains("null") ||
                                           ex.Message.Contains("whitespace"))
        {
            var correlationId = Guid.NewGuid().ToString(); // Fallback correlation ID for logging
            _logger.LogWarning("Invalid resource group parameter provided for Blob Storage creation in environment {Environment} with correlation ID {CorrelationId}: {ErrorMessage}",
                settings.Environment, correlationId, ex.Message);
            throw new ArgumentException("Resource group cannot be null, empty, or whitespace", nameof(resourceGroup));
        }
        catch (ArgumentNullException ex) when (ex.ParamName == "resourceGroup")
        {
            var correlationId = Guid.NewGuid().ToString(); // Fallback correlation ID for logging
            _logger.LogWarning("Null resource group parameter provided for Blob Storage creation in environment {Environment} with correlation ID {CorrelationId}: {ErrorMessage}",
                settings.Environment, correlationId, ex.Message);
            throw new ArgumentException("Resource group cannot be null, empty, or whitespace", nameof(resourceGroup));
        }
        catch (OperationCanceledException)
        {
            var correlationId = Guid.NewGuid().ToString(); // Fallback correlation ID for logging
            var stopwatch = Stopwatch.StartNew();
            _logger.LogWarning("Blob Storage creation cancelled for environment {Environment} after {ElapsedMs}ms with correlation ID {CorrelationId}",
                settings.Environment, stopwatch.ElapsedMilliseconds, correlationId);
            throw;
        }
        catch (Exception ex)
        {
            var correlationId = Guid.NewGuid().ToString(); // Fallback correlation ID for logging
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Stop();
            _logger.LogError(ex, "Blob Storage creation failed for environment {Environment} after {ElapsedMs}ms with correlation ID {CorrelationId}: {ErrorMessage}",
                settings.Environment, stopwatch.ElapsedMilliseconds, correlationId, ex.Message);

            throw new ResourceCreationException(
                $"Failed to create Blob Storage infrastructure for environment {settings.Environment} with correlation ID {correlationId}",
                ex,
                "BlobStorage",
                "StorageAccount",
                correlationId);
        }
    }

    private static StorageAccount CreateBlobStorageAccount(InfrastructureSettings settings, Input<string> resourceGroup, string storageAccountName)
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
            AccessTier = settings.BlobStorage!.AccessTierString switch
            {
                "Hot" => AccessTier.Hot,
                "Cool" => AccessTier.Cool,
                "Premium" => AccessTier.Premium,
                _ => AccessTier.Hot
            },
            EnableHttpsTrafficOnly = settings.Storage?.EnableHttpsTrafficOnly ?? true,
            AllowBlobPublicAccess = settings.BlobStorage.AllowPublicAccess,
            AllowSharedKeyAccess = settings.Storage?.AllowSharedKeyAccess ?? true,
            MinimumTlsVersion = settings.Storage?.MinimumTlsVersion.ToStringValue() ?? "TLS1_2",
            Tags = tags
        });
    }

    private static Dictionary<string, BlobContainer> CreateBlobContainers(InfrastructureSettings settings, Input<string> resourceGroup, StorageAccount storageAccount)
    {
        var containers = new Dictionary<string, BlobContainer>();

        foreach (var containerName in settings.BlobStorage!.ContainerNames)
        {
            var container = new BlobContainer($"{storageAccount.Name}-{containerName}", new BlobContainerArgs
            {
                ResourceGroupName = resourceGroup,
                AccountName = storageAccount.Name,
                ContainerName = containerName,
                PublicAccess = settings.BlobStorage.AllowPublicAccess ? PublicAccess.Container : PublicAccess.None
            });

            containers[containerName] = container;
        }

        return containers;
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


