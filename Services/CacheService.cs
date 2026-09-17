using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Exceptions;
using DeploymentKit.Extensions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.Redis;
using Pulumi.AzureNative.Redis.Inputs;
using Pulumi.AzureNative.RedisEnterprise;
using Pulumi.AzureNative.RedisEnterprise.Inputs;
using System.Diagnostics;
using RedisEnterpriseDatabase = Pulumi.AzureNative.RedisEnterprise.Database;
using RedisEnterpriseDatabaseArgs = Pulumi.AzureNative.RedisEnterprise.DatabaseArgs;
using RedisEnterpriseSkuArgs = Pulumi.AzureNative.RedisEnterprise.Inputs.SkuArgs;
using RedisSkuArgs = Pulumi.AzureNative.Redis.Inputs.SkuArgs;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing Azure Cache for Redis and Azure Managed Redis infrastructure
/// </summary>
public class CacheService(ILogger<CacheService> logger, IResourceNamingService namingService, ICorrelationIdService correlationIdService, IResourceNameValidator resourceNameValidator) : ICacheService
{
    private readonly ILogger<CacheService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IResourceNamingService _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
    private readonly IResourceNameValidator _resourceNameValidator = resourceNameValidator ?? throw new ArgumentNullException(nameof(resourceNameValidator));

    public Task<CacheOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        if (string.IsNullOrEmpty(settings.NamingPrefix))
            throw new ArgumentException("NamingPrefix cannot be null or empty");

        if (string.IsNullOrEmpty(settings.Environment))
            throw new ArgumentException("Environment cannot be null or empty");

        // If cache settings are not provided, skip Redis provisioning
        // Application can use alternative caching strategies (PostgreSQL distributed cache, IMemoryCache, etc.)
        if (settings.Cache == null)
        {
            _logger.LogInformation("Cache settings not provided. Skipping Azure Cache for Redis provisioning. Application will use alternative caching strategy (PostgreSQL, in-memory, etc.)");
            return Task.FromResult(new CacheOutputs
            {
                Name = "cache-not-provisioned",
                HostName = Output.Create(string.Empty),
                ConnectionString = Output.CreateSecret(string.Empty),
                ResourceId = Output.Create(string.Empty),
                RedisPort = Output.Create(0)
            });
        }

        // Use correlation ID from service instead of generating new one
        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        var stopwatch = Stopwatch.StartNew();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId,
            [LoggingConstants.PropertyNames.Environment] = settings.Environment,
            [LoggingConstants.PropertyNames.Service] = LoggingConstants.ServiceNames.CacheService,
            [LoggingConstants.PropertyNames.Operation] = ServiceConstants.ServiceOperations.CreateRedisCache
        });

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Route to appropriate implementation based on settings
            if (settings.Cache.UseAzureManagedRedis)
            {
                return CreateAzureManagedRedisAsync(settings, resourceGroup, correlationId, stopwatch);
            }
            else
            {
                return CreateAzureCacheForRedisAsync(settings, resourceGroup, correlationId, stopwatch);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(ServiceConstants.Cache.CreationCancelledMessage, settings.Environment, stopwatch.ElapsedMilliseconds, correlationId);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, ServiceConstants.Cache.CreationFailedMessage, settings.Environment, stopwatch.ElapsedMilliseconds, correlationId, ex.Message);

            throw new ResourceCreationException(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Cache.ResourceCreationFailedMessage, settings.Environment, correlationId),
                ex,
                ServiceConstants.ResourceTypes.Redis,
                ServiceConstants.ResourceTypes.Redis,
                settings.Environment,
                correlationId,
                ServiceConstants.ErrorCodes.RedisCreationFailed);
        }
    }

    private Task<CacheOutputs> CreateAzureManagedRedisAsync(InfrastructureSettings settings, Input<string> resourceGroup, string correlationId, Stopwatch stopwatch)
    {
        _logger.LogInformation("Creating Azure Managed Redis with SKU {Sku}, Capacity {Capacity}",
            settings.Cache?.ManagedRedisSkuString, settings.Cache!.ManagedRedisCapacity);

        var clusterName = _namingService.GenerateRedisCacheName(settings.NamingPrefix, settings.Environment);

        _resourceNameValidator.ValidateAndThrowIfDuplicate(
            clusterName,
            ResourceType.Cache,
            settings.Environment.MapStringValueToEnvironmentType(),
            correlationId);

        var cluster = CreateRedisEnterpriseCluster(settings, resourceGroup, clusterName, correlationId);

        var database = CreateRedisEnterpriseDatabase(settings, resourceGroup, cluster, correlationId);

        var databaseKeys = GetRedisDatabaseKeys(resourceGroup, cluster, database, correlationId);

        // Generate connection string for Azure Managed Redis
        // Format: <hostname>:<port>,password=<key>,ssl=True
        var connectionString = Output.CreateSecret(
            Output.Tuple(cluster.HostName, database.Port, databaseKeys)
                .Apply(t => string.Format(System.Globalization.CultureInfo.InvariantCulture, DeploymentConstants.Cache.RedisConnectionStringTemplate,
                    t.Item1,
                    t.Item2,
                    t.Item3.PrimaryKey,
                    LoggingConstants.BooleanStrings.True))); // Azure Managed Redis always uses TLS

        var outputs = new CacheOutputs
        {
            Name = clusterName,
            HostName = cluster.HostName,
            ConnectionString = connectionString,
            ResourceId = cluster.Id,
            RedisPort = database.Port.Apply(p => p ?? settings.Cache.ManagedRedisPort)
        };

        stopwatch.Stop();
        _logger.LogInformation("Azure Managed Redis created successfully in {ElapsedMs}ms. Cluster: {ClusterName}, SKU: {Sku}, Capacity: {Capacity} (CorrelationId: {CorrelationId})",
            stopwatch.ElapsedMilliseconds, clusterName, settings.Cache.ManagedRedisSkuString, settings.Cache.ManagedRedisCapacity, correlationId);

        return Task.FromResult(outputs);
    }

    private Task<CacheOutputs> CreateAzureCacheForRedisAsync(InfrastructureSettings settings, Input<string> resourceGroup, string correlationId, Stopwatch stopwatch)
    {
        _logger.LogInformation(ServiceConstants.Cache.CreationStartMessage, settings.Environment, correlationId, settings.Cache?.SkuNameString, settings.Cache?.SkuFamilyString, settings.Cache!.SkuCapacity, settings.Cache.EnableNonSslPort);

        var cacheName = _namingService.GenerateRedisCacheName(settings.NamingPrefix, settings.Environment);

        // Validate resource name uniqueness before creation
        _resourceNameValidator.ValidateAndThrowIfDuplicate(
            cacheName,
            ResourceType.Cache,
            settings.Environment.MapStringValueToEnvironmentType(),
            correlationId);

        // Create Redis Cache
        var cacheStopwatch = Stopwatch.StartNew();
        var redisCache = CreateRedisCache(settings, resourceGroup, cacheName, correlationId);
        _logger.LogInformation(ServiceConstants.Cache.CacheCreationInitiatedMessage,
            cacheName, cacheStopwatch.ElapsedMilliseconds, correlationId);

        // Get Redis Keys
        var keysStopwatch = Stopwatch.StartNew();
        var redisKeys = GetRedisKeys(resourceGroup, redisCache, correlationId);
        _logger.LogInformation(ServiceConstants.Cache.KeysRetrievalInitiatedMessage,
            keysStopwatch.ElapsedMilliseconds, correlationId);

        // Generate connection string
        var connectionString = Output.CreateSecret(
            Output.Tuple(redisCache.HostName, redisCache.Port, redisKeys)
                .Apply(t => string.Format(System.Globalization.CultureInfo.InvariantCulture, DeploymentConstants.Cache.RedisConnectionStringTemplate,
                    t.Item1,
                    t.Item2,
                    t.Item3.PrimaryKey,
                    settings.Cache.EnableNonSslPort ? LoggingConstants.BooleanStrings.False : LoggingConstants.BooleanStrings.True)));

        var outputs = new CacheOutputs
        {
            Name = cacheName,
            HostName = redisCache.HostName,
            ConnectionString = connectionString,
            ResourceId = redisCache.Id,
            RedisPort = redisCache.Port
        };

        stopwatch.Stop();
        _logger.LogInformation(ServiceConstants.Cache.CreationSuccessMessage, settings.Environment, stopwatch.ElapsedMilliseconds, correlationId, cacheName, settings.Cache.SkuNameString, settings.Cache.SkuFamilyString, settings.Cache.SkuCapacity);

        return Task.FromResult(outputs);
    }

    private Redis CreateRedisCache(InfrastructureSettings settings, Input<string> resourceGroup, string cacheName, string correlationId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.ResourceType] = ServiceConstants.ResourceTypes.RedisCache,
            [LoggingConstants.PropertyNames.CacheName] = cacheName,
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId
        });

        try
        {
            var redisCache = new Redis(cacheName, new RedisArgs
            {
                Name = cacheName,
                ResourceGroupName = resourceGroup,
                Location = settings.Location,
                Sku = new RedisSkuArgs
                {
                    Name = settings.Cache?.SkuNameString ?? throw new InvalidOperationException("Sku name cannot be null"),
                    Family = settings.Cache.SkuFamilyString,
                    Capacity = settings.Cache.SkuCapacity
                },
                EnableNonSslPort = settings.Cache.EnableNonSslPort,
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, DeploymentConstants.ResourceTags.RedisCacheType)
            });

            return redisCache;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ServiceConstants.Cache.CacheCreationFailedMessage,
                cacheName, correlationId, ex.Message);
            throw;
        }
    }

    private Output<ListRedisKeysResult> GetRedisKeys(Input<string> resourceGroup, Redis redisCache, string correlationId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.ResourceType] = ServiceConstants.ResourceTypes.Redis,
            [LoggingConstants.PropertyNames.Operation] = ServiceConstants.ServiceOperations.ListKeys,
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId
        });

        try
        {
            _logger.LogInformation(ServiceConstants.Cache.KeysRetrievalStartMessage, correlationId);

            var redisKeys = ListRedisKeys.Invoke(new ListRedisKeysInvokeArgs
            {
                ResourceGroupName = resourceGroup,
                Name = redisCache.Name
            });

            return redisKeys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ServiceConstants.Cache.KeysRetrievalFailedMessage, correlationId, ex.Message);
            throw;
        }
    }

    private RedisEnterprise CreateRedisEnterpriseCluster(InfrastructureSettings settings, Input<string> resourceGroup, string clusterName, string correlationId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.ResourceType] = "RedisEnterprise",
            [LoggingConstants.PropertyNames.CacheName] = clusterName,
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId
        });

        try
        {
            _logger.LogInformation("Creating Azure Managed Redis cluster '{ClusterName}' in {Location} with SKU {Sku}, Capacity {Capacity} (CorrelationId: {CorrelationId})",
                clusterName, settings.Location, settings.Cache!.ManagedRedisSkuString, settings.Cache.ManagedRedisCapacity, correlationId);

            var cluster = new RedisEnterprise(clusterName, new RedisEnterpriseArgs
            {
                ClusterName = clusterName,
                ResourceGroupName = resourceGroup,
                Location = settings.Location,
                Sku = new RedisEnterpriseSkuArgs
                {
                    Name = settings.Cache.ManagedRedisSkuString,
                    Capacity = settings.Cache.ManagedRedisCapacity
                },
                MinimumTlsVersion = "1.2",
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, "ManagedRedis")
            });

            return cluster;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Azure Managed Redis cluster '{ClusterName}' (CorrelationId: {CorrelationId}): {Error}",
                clusterName, correlationId, ex.Message);
            throw;
        }
    }

    private RedisEnterpriseDatabase CreateRedisEnterpriseDatabase(InfrastructureSettings settings, Input<string> resourceGroup, RedisEnterprise cluster, string correlationId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.ResourceType] = "RedisEnterpriseDatabase",
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId
        });

        try
        {
            _logger.LogInformation("Creating Azure Managed Redis database in cluster with clustering policy {ClusteringPolicy}, eviction policy {EvictionPolicy} (CorrelationId: {CorrelationId})",
                settings.Cache!.ClusteringPolicyString, settings.Cache.ManagedRedisEvictionPolicyString, correlationId);

            var databaseArgs = new RedisEnterpriseDatabaseArgs
            {
                ClusterName = cluster.Name,
                ResourceGroupName = resourceGroup,
                ClientProtocol = settings.Cache.ClientProtocolString,
                ClusteringPolicy = settings.Cache.ClusteringPolicyString,
                EvictionPolicy = settings.Cache.ManagedRedisEvictionPolicyString,
                Port = settings.Cache.ManagedRedisPort
            };

            if (settings.Cache.EnableGeoReplication && settings.Cache.LinkedDatabaseIds.Count > 0)
            {
                databaseArgs.GeoReplication = new DatabasePropertiesGeoReplicationArgs
                {
                    LinkedDatabases = settings.Cache.LinkedDatabaseIds
                        .Select(id => new LinkedDatabaseArgs { Id = id })
                        .ToList(),
                    GroupNickname = settings.Cache.LinkedDatabaseGroupNickname ?? throw new InvalidOperationException("Group nickname cannot be null"),
                };
            }

            return new RedisEnterpriseDatabase("default", databaseArgs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Azure Managed Redis database (CorrelationId: {CorrelationId}): {Error}",
                correlationId, ex.Message);
            throw;
        }
    }

    private Output<ListDatabaseKeysResult> GetRedisDatabaseKeys(Input<string> resourceGroup, RedisEnterprise cluster, RedisEnterpriseDatabase database, string correlationId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.ResourceType] = "RedisEnterpriseDatabase",
            [LoggingConstants.PropertyNames.Operation] = ServiceConstants.ServiceOperations.ListKeys,
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId
        });

        try
        {
            _logger.LogInformation("Retrieving Azure Managed Redis database keys (CorrelationId: {CorrelationId})", correlationId);

            var databaseKeys = ListDatabaseKeys.Invoke(new ListDatabaseKeysInvokeArgs
            {
                ResourceGroupName = resourceGroup,
                ClusterName = cluster.Name,
                DatabaseName = database.Name
            });

            return databaseKeys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Azure Managed Redis database keys (CorrelationId: {CorrelationId}): {Error}",
                correlationId, ex.Message);
            throw;
        }
    }



    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken) => await CreateAsync(settings, resourceGroup, cancellationToken);

    /// <summary>
    /// Explicit implementation of IInfrastructureService.CreateAsync without CancellationToken
    /// </summary>
    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup) => await CreateAsync(settings, resourceGroup);
}


