using DeploymentKit.Constants;
using DeploymentKit.Extensions;
using DeploymentKit.Helpers;
using DeploymentKit.Interfaces;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Partial class for InfrastructureBuilder containing storage-related resource configuration methods.
/// </summary>
public partial class InfrastructureBuilder
{
    private bool _addRedis;
    private CacheSettings? _cacheSettings;

    private bool _addStorage;
    private StorageSettings? _storageSettings;

    private bool _addBlobStorage;
    private BlobStorageSettings? _blobStorageSettings;

    private bool _addTableStorage;
    private TableStorageSettings? _tableStorageSettings;

    /// <summary>
    /// Adds Redis Cache with default settings.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddRedis()
    {
        _addRedis = true;
        _cacheSettings = InfrastructureDefaultSettingsFactory.GetDefaultCacheSettings();
        _logger.LogInformation(BuilderConstants.LoggingMessages.RedisDefault);
        return this;
    }

    /// <summary>
    /// Adds Redis Cache with custom settings.
    /// </summary>
    /// <param name="cacheSettings">Custom Redis Cache settings.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddRedis(CacheSettings cacheSettings)
    {
        _addRedis = true;
        _cacheSettings = cacheSettings ?? throw new ArgumentNullException(nameof(cacheSettings));

        // Ensure SkuNameString is populated from enum if it's empty
        if (string.IsNullOrWhiteSpace(_cacheSettings.SkuNameString))
        {
            _cacheSettings.SkuNameString = _cacheSettings.SkuName.ToStringValue();
        }

        _logger.LogInformation(BuilderConstants.LoggingMessages.RedisCustom);
        return this;
    }

    /// <summary>
    /// Adds Storage Account with default settings.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddStorage()
    {
        _addStorage = true;
        _storageSettings = InfrastructureDefaultSettingsFactory.GetDefaultStorageSettings();
        _logger.LogInformation(BuilderConstants.LoggingMessages.StorageDefault);
        return this;
    }

    /// <summary>
    /// Adds Storage Account with custom settings.
    /// </summary>
    /// <param name="storageSettings">Custom storage settings.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddStorage(StorageSettings storageSettings)
    {
        _addStorage = true;
        _storageSettings = storageSettings ?? throw new ArgumentNullException(nameof(storageSettings));
        _logger.LogInformation(BuilderConstants.LoggingMessages.StorageCustom);
        return this;
    }

    /// <summary>
    /// Adds Blob Storage with default settings.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddBlobStorage()
    {
        _addBlobStorage = true;
        _blobStorageSettings = InfrastructureDefaultSettingsFactory.GetDefaultBlobStorageSettings();
        _logger.LogInformation(BuilderConstants.LoggingMessages.BlobStorageDefault);
        return this;
    }

    /// <summary>
    /// Adds Blob Storage with custom settings.
    /// </summary>
    /// <param name="blobStorageSettings">Custom blob storage settings.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddBlobStorage(BlobStorageSettings blobStorageSettings)
    {
        _addBlobStorage = true;
        _blobStorageSettings = blobStorageSettings ?? throw new ArgumentNullException(nameof(blobStorageSettings));
        _logger.LogInformation(BuilderConstants.LoggingMessages.BlobStorageCustom);
        return this;
    }

    /// <summary>
    /// Adds Table Storage with default settings.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddTableStorage()
    {
        _addTableStorage = true;
        _tableStorageSettings = InfrastructureDefaultSettingsFactory.GetDefaultTableStorageSettings();
        _logger.LogInformation(BuilderConstants.LoggingMessages.TableStorageDefault);
        return this;
    }

    /// <summary>
    /// Adds Table Storage with custom settings.
    /// </summary>
    /// <param name="tableStorageSettings">Custom table storage settings.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder AddTableStorage(TableStorageSettings tableStorageSettings)
    {
        _addTableStorage = true;
        _tableStorageSettings = tableStorageSettings ?? throw new ArgumentNullException(nameof(tableStorageSettings));
        _logger.LogInformation(BuilderConstants.LoggingMessages.TableStorageCustom);
        return this;
    }
}

