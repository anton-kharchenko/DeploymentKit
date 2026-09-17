using DeploymentKit.Constants;
using DeploymentKit.Interfaces;
using DeploymentKit.Services;
using DeploymentKit.Settings;

namespace DeploymentKit.Validators;

public class PreDeploymentValidator(ILogger<PreDeploymentValidator> logger, IResourceNamingService namingService, IAzureResourceStateValidator? azureStateValidator = null, INamingConsistencyValidator? namingConsistencyValidator = null, ISubscriptionResourceGroupValidator? subscriptionResourceGroupValidator = null) : IPreDeploymentValidator
{
    private readonly ILogger<PreDeploymentValidator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IResourceNamingService _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));

    public PreDeploymentValidationResult ValidateAllResourceNames(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var result = new PreDeploymentValidationResult();

        _logger.LogInformation("Starting pre-deployment resource naming validation for environment: {Environment}", settings.Environment);

        ValidateResourceGroupName(settings, result);
        ValidateStorageAccountName(settings, result);
        ValidateLogAnalyticsWorkspaceName(settings, result);
        ValidateContainerRegistryName(settings, result);
        ValidateKeyVaultName(settings, result);
        ValidateVirtualNetworkName(settings, result);
        ValidatePostgreSqlServerName(settings, result);
        ValidateRedisCacheName(settings, result);
        ValidateApplicationInsightsName(settings, result);
        ValidateContainerAppNames(settings, result);
        ValidateSubnetNames(settings, result);

        result.IsValid = result.Errors.Count == 0;

        if (result.IsValid)
        {
            _logger.LogInformation("✅ Pre-deployment validation PASSED. All {Count} resource names are valid.", result.ValidatedResources.Count);
        }
        else
        {
            _logger.LogError("❌ Pre-deployment validation FAILED with {ErrorCount} errors and {WarningCount} warnings.",
                result.Errors.Count, result.Warnings.Count);

            foreach (var error in result.Errors)
            {
                _logger.LogError("  ❌ {Error}", error);
            }
        }

        if (result.Warnings.Count > 0)
        {
            foreach (var warning in result.Warnings)
            {
                _logger.LogWarning("  ⚠️  {Warning}", warning);
            }
        }

        return result;
    }

    public PreDeploymentValidationResult ValidateSettings(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var result = new PreDeploymentValidationResult();

        _logger.LogInformation("Starting pre-deployment settings validation for environment: {Environment}", settings.Environment);

        ValidateCoreSettings(settings, result);
        ValidateDatabaseSettingsIfProvided(settings, result);
        ValidateContainerSettingsIfProvided(settings, result);
        ValidateMonitoringSettingsIfProvided(settings, result);
        ValidateCacheSettingsIfProvided(settings, result);
        ValidateNetworkSettingsIfProvided(settings, result);
        ValidateKeyVaultSettingsIfProvided(settings, result);
        ValidateMigrationSettingsIfProvided(settings, result);

        result.IsValid = result.Errors.Count == 0;

        if (result.IsValid)
        {
            _logger.LogInformation("✅ Settings validation PASSED. All {Count} settings are valid.", result.ValidatedResources.Count);
        }
        else
        {
            _logger.LogError("❌ Settings validation FAILED with {ErrorCount} errors and {WarningCount} warnings.",
                result.Errors.Count, result.Warnings.Count);

            foreach (var error in result.Errors)
            {
                _logger.LogError("  ❌ {Error}", error);
            }
        }

        if (result.Warnings.Count > 0)
        {
            foreach (var warning in result.Warnings)
            {
                _logger.LogWarning("  ⚠️  {Warning}", warning);
            }
        }

        return result;
    }

    public async Task<PreDeploymentValidationResult> ValidateAllAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _logger.LogInformation("Starting comprehensive async pre-deployment validation for environment: {Environment} with ValidationMode: {ValidationMode}", 
            settings.Environment, settings.ValidationMode);

        // Check validation mode
        if (settings.ValidationMode == Enums.ValidationMode.Skip)
        {
            _logger.LogWarning("⚠️  Validation mode is set to Skip - bypassing all validation checks");
            return new PreDeploymentValidationResult 
            { 
                IsValid = true,
                Warnings = { "⚠️  Validation was skipped - ValidationMode set to Skip" }
            };
        }

        var combinedResult = new PreDeploymentValidationResult();

        // Always perform basic settings and naming validation (unless Skip mode)
        if (settings.ValidationMode != Enums.ValidationMode.Minimal)
        {
            var settingsResult = ValidateSettings(settings);
            var namingResult = ValidateAllResourceNames(settings);

            combinedResult.Errors.AddRange(settingsResult.Errors);
            combinedResult.Errors.AddRange(namingResult.Errors);

            combinedResult.Warnings.AddRange(settingsResult.Warnings);
            combinedResult.Warnings.AddRange(namingResult.Warnings);

            combinedResult.ValidatedResources.AddRange(settingsResult.ValidatedResources);
            combinedResult.ValidatedResources.AddRange(namingResult.ValidatedResources);
        }
        else
        {
            _logger.LogInformation("Validation mode is Minimal - skipping detailed settings and naming validation");
        }

        // Only run Azure connectivity checks if ValidationMode is Full
        if (settings.ValidationMode == Enums.ValidationMode.Full)
        {
            // Add Azure state validation if available
            if (subscriptionResourceGroupValidator != null && !settings.SkipAzureAuthValidation)
            {
                _logger.LogInformation("Running Azure subscription and resource group validation...");
                var subscriptionResult = await subscriptionResourceGroupValidator.ValidateSubscriptionAndResourceGroupAsync(settings);

                combinedResult.Errors.AddRange(subscriptionResult.Errors);
                combinedResult.Warnings.AddRange(subscriptionResult.Warnings);

                if (subscriptionResult.IsValid)
                {
                    combinedResult.ValidatedResources.Add("✅ Azure subscription and resource group validation passed");
                }
            }
            else if (settings.SkipAzureAuthValidation)
            {
                _logger.LogInformation("Skipping Azure authentication validation (SkipAzureAuthValidation = true)");
                combinedResult.Warnings.Add("⚠️  Azure authentication validation skipped");
            }

            // Add naming consistency validation if available
            if (namingConsistencyValidator != null)
            {
                _logger.LogInformation("Running naming consistency validation...");
                var namingConsistencyResult = await namingConsistencyValidator.ValidateNamingConsistencyAsync(settings);

                combinedResult.Errors.AddRange(namingConsistencyResult.Errors);
                combinedResult.Warnings.AddRange(namingConsistencyResult.Warnings);

                if (namingConsistencyResult.IsValid)
                {
                    combinedResult.ValidatedResources.Add("✅ Naming consistency validation passed");
                }
            }

            // Add Azure resource state validation if available
            if (azureStateValidator != null && !settings.SkipAzureAuthValidation)
            {
                _logger.LogInformation("Running Azure resource state validation...");
                var azureStateResult = await azureStateValidator.ValidatePreDeploymentStateAsync(settings);

                combinedResult.Errors.AddRange(azureStateResult.Errors);
                combinedResult.Warnings.AddRange(azureStateResult.Warnings);

                if (azureStateResult.IsValid)
                {
                    combinedResult.ValidatedResources.Add("✅ Azure resource state validation passed");
                }
            }
        }
        else
        {
            _logger.LogInformation("Validation mode is {ValidationMode} - skipping Azure connectivity checks", settings.ValidationMode);
            combinedResult.Warnings.Add($"⚠️  Azure connectivity validation skipped (ValidationMode: {settings.ValidationMode})");
        }

        combinedResult.IsValid = combinedResult.Errors.Count == 0;

        if (combinedResult.IsValid)
        {
            _logger.LogInformation("✅ Comprehensive async pre-deployment validation PASSED");
        }
        else
        {
            _logger.LogError("❌ Comprehensive async pre-deployment validation FAILED with {ErrorCount} errors and {WarningCount} warnings.",
                combinedResult.Errors.Count, combinedResult.Warnings.Count);
        }

        return combinedResult;
    }

    private void ValidateResourceGroupName(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        var name = _namingService.GenerateResourceGroupName(settings.NamingPrefix, settings.Environment);
        ValidateResourceName(name, "Resource Group", AzureResourceNamingConstraints.ResourceGroup.IsValid,
            AzureResourceNamingConstraints.ResourceGroup.Description, result);
    }

    private void ValidateStorageAccountName(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        var name = _namingService.GenerateStorageAccountName(settings.NamingPrefix, settings.Environment);

        if (!AzureResourceNamingConstraints.StorageAccount.IsValid(name))
        {
            result.Errors.Add($"❌ Storage Account name '{name}' is INVALID. " +
                $"Length: {name.Length} chars (max {AzureResourceNamingConstraints.StorageAccount.MaxLength}). " +
                $"{AzureResourceNamingConstraints.StorageAccount.Description}");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Storage Account: {name} ({name.Length}/{AzureResourceNamingConstraints.StorageAccount.MaxLength} chars)");
        }

        if (name.Length > 20)
        {
            result.Warnings.Add($"⚠️  Storage Account name '{name}' is {name.Length} characters. " +
                "Consider using a shorter naming prefix for better readability.");
        }
    }

    private void ValidateLogAnalyticsWorkspaceName(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        var name = _namingService.GenerateLogAnalyticsWorkspaceName(settings.NamingPrefix, settings.Environment);
        ValidateResourceName(name, "Log Analytics Workspace", AzureResourceNamingConstraints.LogAnalyticsWorkspace.IsValid,
            AzureResourceNamingConstraints.LogAnalyticsWorkspace.Description, result);
    }

    private void ValidateContainerRegistryName(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        var name = _namingService.GenerateContainerRegistryName(settings.NamingPrefix, settings.Environment);
        ValidateResourceName(name, "Container Registry", AzureResourceNamingConstraints.ContainerRegistry.IsValid,
            AzureResourceNamingConstraints.ContainerRegistry.Description, result);
    }

    private void ValidateKeyVaultName(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        var name = _namingService.GenerateKeyVaultName(settings.NamingPrefix, settings.Environment);

        if (!AzureResourceNamingConstraints.KeyVault.IsValid(name))
        {
            result.Errors.Add($"❌ Key Vault name '{name}' is INVALID. " +
                $"Length: {name.Length} chars (max {AzureResourceNamingConstraints.KeyVault.MaxLength}). " +
                $"{AzureResourceNamingConstraints.KeyVault.Description}");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Key Vault: {name}");
        }
    }

    private void ValidateVirtualNetworkName(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        var name = _namingService.GenerateVirtualNetworkName(settings.NamingPrefix, settings.Environment);
        ValidateResourceName(name, "Virtual Network", AzureResourceNamingConstraints.VirtualNetwork.IsValid,
            AzureResourceNamingConstraints.VirtualNetwork.Description, result);
    }

    private void ValidatePostgreSqlServerName(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        var name = _namingService.GeneratePostgreSqlServerName(settings.NamingPrefix, settings.Environment);
        ValidateResourceName(name, "PostgreSQL Server", AzureResourceNamingConstraints.PostgreSqlServer.IsValid,
            AzureResourceNamingConstraints.PostgreSqlServer.Description, result);
    }

    private void ValidateRedisCacheName(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        var name = _namingService.GenerateRedisCacheName(settings.NamingPrefix, settings.Environment);
        ValidateResourceName(name, "Redis Cache", AzureResourceNamingConstraints.RedisCache.IsValid,
            AzureResourceNamingConstraints.RedisCache.Description, result);
    }

    private void ValidateApplicationInsightsName(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        var name = _namingService.GenerateApplicationInsightsName(settings.NamingPrefix, settings.Environment);
        ValidateResourceName(name, "Application Insights", AzureResourceNamingConstraints.ApplicationInsights.IsValid,
            AzureResourceNamingConstraints.ApplicationInsights.Description, result);
    }

    private void ValidateContainerAppNames(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        var apiName = _namingService.GenerateContainerAppName(settings.NamingPrefix, "api", settings.Environment);
        ValidateResourceName(apiName, "Container App (API)", AzureResourceNamingConstraints.ContainerApp.IsValid,
            AzureResourceNamingConstraints.ContainerApp.Description, result);

        var jobsName = _namingService.GenerateContainerAppName(settings.NamingPrefix, "jobs", settings.Environment);
        ValidateResourceName(jobsName, "Container App (Jobs)", AzureResourceNamingConstraints.ContainerApp.IsValid,
            AzureResourceNamingConstraints.ContainerApp.Description, result);
    }

    private void ValidateSubnetNames(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        var subnetTypes = new[] { "containerapp", "database", "appgateway", "privateendpoints" };

        foreach (var subnetType in subnetTypes)
        {
            var name = _namingService.GenerateSubnetName(subnetType, settings.Environment);
            ValidateResourceName(name, $"Subnet ({subnetType})", AzureResourceNamingConstraints.Subnet.IsValid,
                AzureResourceNamingConstraints.Subnet.Description, result);
        }
    }

    private static void ValidateResourceName(string name, string resourceType, Func<string, bool> validator,
        string description, PreDeploymentValidationResult result)
    {
        if (!validator(name))
        {
            result.Errors.Add($"❌ {resourceType} name '{name}' is INVALID. {description}");
        }
        else
        {
            result.ValidatedResources.Add($"✅ {resourceType}: {name}");
        }
    }

    private static void ValidateCoreSettings(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(settings.Environment))
        {
            result.Errors.Add($"❌ Environment cannot be null or empty");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Environment: {settings.Environment}");
        }

        if (string.IsNullOrWhiteSpace(settings.Location))
        {
            result.Errors.Add($"❌ Location cannot be null or empty");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Location: {settings.Location}");
        }

        if (string.IsNullOrWhiteSpace(settings.NamingPrefix))
        {
            result.Errors.Add($"❌ Naming prefix cannot be null or empty");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Naming Prefix: {settings.NamingPrefix}");
        }

        if (string.IsNullOrWhiteSpace(settings.SubscriptionId))
        {
            result.Errors.Add($"❌ Subscription ID cannot be null or empty");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Subscription ID configured");
        }

        if (string.IsNullOrWhiteSpace(settings.ResourceGroupName))
        {
            result.Errors.Add($"❌ Resource group name cannot be null or empty");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Resource Group Name: {settings.ResourceGroupName}");
        }
    }

    private static void ValidateDatabaseSettingsIfProvided(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        if (settings.Database == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.Database.AdminUsername))
        {
            result.Errors.Add($"❌ Database admin username cannot be null or empty. Please set the {EnvironmentVariableNames.Database.AdminUsername} environment variable.");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Database admin username configured");
        }

        if (string.IsNullOrWhiteSpace(settings.Database.AdminPassword) || string.IsNullOrWhiteSpace(settings.Database.Password))
        {
            result.Errors.Add($"❌ Database password cannot be null or empty. Please set the {EnvironmentVariableNames.Database.AdminPassword} environment variable.");
        }
        else if (settings.Database.AdminPassword.Length < 8 || settings.Database.Password.Length < 8)
        {
            result.Errors.Add($"❌ Database password must be at least 8 characters long. Current length: {Math.Min(settings.Database.AdminPassword.Length, settings.Database.Password.Length)}");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Database password configured (length: {settings.Database.Password.Length} chars)");
        }

        if (settings.Database.StorageSizeGb <= 0)
        {
            result.Errors.Add($"❌ Database storage size must be greater than 0 GB");
        }
        else if (!InfrastructureConstants.Database.ValidStorageSizesGb.Contains(settings.Database.StorageSizeGb))
        {
            var closestValidSize = InfrastructureConstants.Database.ValidStorageSizesGb
                .OrderBy(size => Math.Abs(size - settings.Database.StorageSizeGb))
                .First();
            result.Errors.Add($"❌ Database storage size {settings.Database.StorageSizeGb}GB is not valid. Azure requires: {string.Join(", ", InfrastructureConstants.Database.ValidStorageSizesGb)}GB. Closest: {closestValidSize}GB");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Database storage: {settings.Database.StorageSizeGb} GB");
        }
    }

    private static void ValidateContainerSettingsIfProvided(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        if (settings.Container == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.Container.ApiImageTag))
        {
            result.Errors.Add($"❌ API image tag cannot be null or empty");
        }
        else
        {
            result.ValidatedResources.Add($"✅ API image tag: {settings.Container.ApiImageTag}");
        }

        if (string.IsNullOrWhiteSpace(settings.Container.JobsImageTag))
        {
            result.Errors.Add($"❌ Jobs image tag cannot be null or empty");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Jobs image tag: {settings.Container.JobsImageTag}");
        }

        if (settings.Container.CpuLimit <= 0)
        {
            result.Errors.Add($"❌ Container CPU limit must be greater than 0");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Container CPU limit: {settings.Container.CpuLimit}");
        }

        if (settings.Container.MinReplicas < 0)
        {
            result.Errors.Add($"❌ Minimum replicas cannot be negative");
        }

        if (settings.Container.MaxReplicas < settings.Container.MinReplicas)
        {
            result.Errors.Add($"❌ Maximum replicas ({settings.Container.MaxReplicas}) must be greater than or equal to minimum replicas ({settings.Container.MinReplicas})");
        }
    }

    private static void ValidateMonitoringSettingsIfProvided(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        if (settings.Monitoring == null)
        {
            return;
        }

        if (settings.Monitoring.LogRetentionDays <= 0)
        {
            result.Errors.Add($"❌ Log retention days must be greater than 0");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Log retention: {settings.Monitoring.LogRetentionDays} days");
        }
    }

    private static void ValidateCacheSettingsIfProvided(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        if (settings.Cache == null)
        {
            return;
        }

        if (settings.Cache.SkuCapacity < 0 || settings.Cache.SkuCapacity > 6)
        {
            result.Errors.Add($"❌ Cache SKU capacity must be between 0 and 6 (C0-C6 for Basic/Standard or P0-P6 for Premium)");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Cache capacity: C{settings.Cache.SkuCapacity} ({GetRedisCacheSize(settings.Cache.SkuCapacity)})");
        }

        if (string.IsNullOrWhiteSpace(settings.Cache.SkuNameString))
        {
            result.Errors.Add($"❌ Cache SKU name cannot be null or empty");
        }
    }

    private static string GetRedisCacheSize(int capacity) => capacity switch
    {
        0 => "250MB",
        1 => "1GB",
        2 => "2.5GB",
        3 => "6GB",
        4 => "13GB",
        5 => "26GB",
        6 => "53GB",
        _ => "Unknown"
    };

    private static void ValidateNetworkSettingsIfProvided(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        if (settings.Network == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.Network.VirtualNetworkAddressSpace))
        {
            result.Errors.Add($"❌ Virtual network address space cannot be null or empty");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Virtual network address space: {settings.Network.VirtualNetworkAddressSpace}");
        }

        if (!string.IsNullOrWhiteSpace(settings.Network.CustomDomain))
        {
            if (!Uri.TryCreate($"https://{settings.Network.CustomDomain}", UriKind.Absolute, out _))
            {
                result.Errors.Add($"❌ Invalid custom domain format: {settings.Network.CustomDomain}");
            }
            else
            {
                result.ValidatedResources.Add($"✅ Custom domain: {settings.Network.CustomDomain}");
            }
        }
    }

    private static void ValidateKeyVaultSettingsIfProvided(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        if (settings.KeyVault == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.KeyVault.SkuNameString))
        {
            result.Errors.Add($"❌ Key Vault SKU name cannot be null or empty");
        }
        else
        {
            result.ValidatedResources.Add($"✅ Key Vault SKU: {settings.KeyVault.SkuNameString}");
        }

        if (settings.KeyVault.EnableSoftDelete && settings.KeyVault.SoftDeleteRetentionDays < 7)
        {
            result.Warnings.Add($"⚠️  Soft delete retention days ({settings.KeyVault.SoftDeleteRetentionDays}) is less than recommended minimum of 7 days");
        }
    }

    private static void ValidateMigrationSettingsIfProvided(InfrastructureSettings settings, PreDeploymentValidationResult result)
    {
        if (settings.Migration == null || !settings.Migration.Enabled)
        {
            return;
        }

        result.ValidatedResources.Add($"✅ Migration Type: {settings.Migration.MigrationTypeString}");

        // Validate based on migration type
        switch (settings.Migration.MigrationType)
        {
            case Enums.MigrationType.EfCore:
                if (string.IsNullOrWhiteSpace(settings.Migration.MigrationAssembly))
                {
                    result.Errors.Add("❌ Migration Assembly is required when using EfCore migration type");
                }
                else
                {
                    result.ValidatedResources.Add($"✅ Migration Assembly: {settings.Migration.MigrationAssembly}");
                }

                if (string.IsNullOrWhiteSpace(settings.Migration.DbContextTypeName))
                {
                    result.Errors.Add("❌ DbContext Type Name is required when using EfCore migration type");
                }
                else
                {
                    result.ValidatedResources.Add($"✅ DbContext Type: {settings.Migration.DbContextTypeName}");
                }
                break;

            case Enums.MigrationType.FluentMigrator:
                if (string.IsNullOrWhiteSpace(settings.Migration.MigrationAssembly))
                {
                    result.Errors.Add("❌ Migration Assembly is required when using FluentMigrator migration type");
                }
                else
                {
                    result.ValidatedResources.Add($"✅ Migration Assembly: {settings.Migration.MigrationAssembly}");
                }
                break;

            case Enums.MigrationType.SqlScript:
                if (string.IsNullOrWhiteSpace(settings.Migration.SqlScriptPath))
                {
                    result.Errors.Add("❌ SQL Script Path is required when using SqlScript migration type");
                }
                else
                {
                    result.ValidatedResources.Add($"✅ SQL Script Path: {settings.Migration.SqlScriptPath}");
                }
                break;
        }

        // Validate Container Job configuration if enabled
        if (settings.Migration.UseContainerJob)
        {
            if (string.IsNullOrWhiteSpace(settings.Migration.MigrationContainerImage))
            {
                result.Errors.Add("❌ Migration Container Image is required when UseContainerJob is true");
            }
            else
            {
                result.ValidatedResources.Add($"✅ Migration Container Image: {settings.Migration.MigrationContainerImage}");
            }
        }

        // Validate timeout
        if (settings.Migration.TimeoutSeconds < 30 || settings.Migration.TimeoutSeconds > 3600)
        {
            result.Errors.Add($"❌ Migration timeout ({settings.Migration.TimeoutSeconds}s) must be between 30 and 3600 seconds");
        }

        // Add info about auto-run configuration
        if (settings.Migration.AutoRunOnDeployment)
        {
            result.ValidatedResources.Add("✅ Auto-run migrations on deployment: Enabled");
        }
        else
        {
            result.Warnings.Add("⚠️  Auto-run migrations on deployment is disabled. Migrations must be run manually.");
        }
    }
}

