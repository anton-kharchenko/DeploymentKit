using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using DeploymentKit.Helpers.Builder;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Partial class for InfrastructureBuilder containing build logic.
/// </summary>
public partial class InfrastructureBuilder
{
    /// <summary>
    /// Builds the infrastructure settings synchronously.
    /// </summary>
    /// <returns>The infrastructure settings.</returns>
    public InfrastructureSettings Build()
    {
        // Execute any deferred synchronous configuration
        ApplyKeyVaultSettingsSync();

        return CreateSettings();
    }

    /// <summary>
    /// Builds the infrastructure settings asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the infrastructure settings.</returns>
    public async Task<InfrastructureSettings> BuildAsync(CancellationToken cancellationToken = default)
    {
        // Execute any deferred asynchronous configuration
        await ApplyKeyVaultSettingsAsync();

        return CreateSettings();
    }

    public Task<InfrastructureSettings> BuildAsync()
    {
        return BuildAsync(CancellationToken.None);
    }

    private InfrastructureSettings CreateSettings()
    {
        // If existing settings are provided, use them
        if (_existingSettings != null)
        {
            _logger.LogInformation("Building infrastructure with existing settings");
            return _existingSettings;
        }

        // Validate required configuration
        ValidateRequiredConfiguration();

        if (_validationErrors.Any())
        {
            var errorMessage = string.Join("; ", _validationErrors);
            _logger.LogError("Build failed due to validation errors: {Errors}", errorMessage);
            throw new InvalidOperationException($"Cannot build infrastructure due to validation errors: {errorMessage}");
        }

        return CreateInfrastructureSettings();
    }

    private InfrastructureSettings CreateInfrastructureSettings()
    {
        // Map environment names to validation-compliant values
        var mappedEnvironment = BuilderHelper.MapEnvironmentName(_environment!);

        // Use provided naming prefix or derive from deployment name (sanitized)
        var namingPrefix = _namingPrefix ?? SanitizeNamingPrefix(_deploymentName ?? InfrastructureConstants.Defaults.NamingPrefix);

        // Generate default resource group name using the consistent naming pattern: {prefix}-rg-{environment}
        // This matches InfrastructureConstants.NamingPatterns.ResourceGroup pattern
        var resourceGroupName = _resourceGroupName ?? string.Format(System.Globalization.CultureInfo.InvariantCulture, InfrastructureConstants.NamingPatterns.ResourceGroup, namingPrefix, mappedEnvironment);

        // Build the infrastructure settings
        var settings = new InfrastructureSettings
        {
            Environment = mappedEnvironment,
            Location = _location ?? InfrastructureConstants.Defaults.Location,
            SubscriptionId = _subscriptionId ?? string.Empty,
            ResourceGroupName = resourceGroupName,
            NamingPrefix = namingPrefix.ToLowerInvariant(),
            ValidationMode = _validationMode,
            SkipAzureAuthValidation = _skipAzureAuthValidation,
            Database = _addDatabase ? _databaseSettings : null,
            Container = _addContainerApps ? _containerSettings : null,
            Monitoring = _addInsights ? _monitoringSettings : null,
            Cache = _addRedis ? _cacheSettings : null,
            EventHubs = _addMessageBroker ? _eventHubsSettings : null,
            Network = _addNetworking ? _networkSettings : null,
            KeyVault = _addKeyVault ? _keyVaultSettings : null,
            Storage = _addStorage ? _storageSettings : null,
            BlobStorage = _addBlobStorage ? _blobStorageSettings : null,
            CosmosDb = _addCosmosDb ? _cosmosDbSettings : null,
            TableStorage = _addTableStorage ? _tableStorageSettings : null,
            ApplicationGateway = _addApplicationGateway ? _applicationGatewaySettings : null,
            GreenBlueDeployment = _enableGreenBlueDeployment ? _greenBlueSettings : null,
            GreenSlot = _enableGreenBlueDeployment ? new SlotSettings { SlotName = DeploymentSlotType.Green.ToStringValue() } : null,
            BlueSlot = _enableGreenBlueDeployment ? new SlotSettings { SlotName = DeploymentSlotType.Blue.ToStringValue() } : null,
            Migration = _configureMigrations ? _migrationSettings : null,
            CustomDomain = _configureCustomDomain ? _customDomainSettings : null
        };

        _logger.LogInformation("Infrastructure settings built successfully for deployment '{DeploymentName}'", _deploymentName);
        return settings;
    }
}


