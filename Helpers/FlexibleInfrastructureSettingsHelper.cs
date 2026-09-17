using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using DeploymentKit.Interfaces;
using DeploymentKit.Settings;

namespace DeploymentKit.Helpers;

/// <summary>
/// Flexible infrastructure settings loader that allows configurable services and credential sources
/// </summary>
public static class FlexibleInfrastructureSettingsHelper
{
    /// <summary>
    /// Creates InfrastructureSettings from a credential provider with configurable services
    /// </summary>
    /// <param name="credentialProvider">The credential provider to use</param>
    /// <param name="enabledServices">The services to enable (null means all services are optional)</param>
    /// <returns>Populated InfrastructureSettings object</returns>
    public static InfrastructureSettings FromCredentialProvider(
        ICredentialProvider credentialProvider,
        HashSet<string>? enabledServices = null)
    {
        try
        {
            var settings = new InfrastructureSettings
            {
                Environment = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Core.Environment, InfrastructureConstants.Defaults.Environment),
                Location = GetCredentialWithFallback(credentialProvider, [EnvironmentVariableNames.Core.Location, "AZURE_LOCATION", "DEPLOYMENT_LOCATION"], InfrastructureConstants.Defaults.Location),
                NamingPrefix = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Core.NamingPrefix, InfrastructureConstants.Defaults.NamingPrefix),
                SubscriptionId = GetRequiredCredentialWithFallback(credentialProvider, [EnvironmentVariableNames.Core.SubscriptionId, "AZURE_SUBSCRIPTION_ID", "ARM_SUBSCRIPTION_ID"]),
                ResourceGroupName = GetRequiredCredential(credentialProvider, EnvironmentVariableNames.Core.ResourceGroupName)
            };

            // Load optional services based on configuration
            if (IsServiceEnabled(enabledServices, "Database") && HasDatabaseCredentials(credentialProvider))
            {
                settings.Database = LoadDatabaseSettings(credentialProvider);
            }

            if (IsServiceEnabled(enabledServices, "Container"))
            {
                settings.Container = LoadContainerSettings(credentialProvider);
            }

            if (IsServiceEnabled(enabledServices, "Monitoring"))
            {
                settings.Monitoring = LoadMonitoringSettings(credentialProvider);
            }

            if (IsServiceEnabled(enabledServices, "Cache") && HasCacheCredentials())
            {
                settings.Cache = LoadCacheSettings(credentialProvider);
            }

            if (IsServiceEnabled(enabledServices, "EventHubs") && HasEventHubsCredentials())
            {
                settings.EventHubs = LoadEventHubsSettings(credentialProvider);
            }

            if (IsServiceEnabled(enabledServices, "Network"))
            {
                settings.Network = LoadNetworkSettings(credentialProvider);
            }

            if (IsServiceEnabled(enabledServices, "KeyVault"))
            {
                settings.KeyVault = LoadKeyVaultSettings();
            }

            if (IsServiceEnabled(enabledServices, "Api"))
            {
                settings.Api = LoadApiSettings(credentialProvider);
            }

            if (IsServiceEnabled(enabledServices, "GreenBlueDeployment"))
            {
                settings.GreenBlueDeployment = LoadGreenBlueDeploymentSettings(credentialProvider);
                settings.GreenSlot = LoadSlotSettings(credentialProvider, DeploymentSlotType.Green.ToStringValue());
                settings.BlueSlot = LoadSlotSettings(credentialProvider, DeploymentSlotType.Blue.ToStringValue());
            }

            if (IsServiceEnabled(enabledServices, "Storage") && HasStorageCredentials(credentialProvider))
            {
                settings.Storage = LoadStorageSettings(credentialProvider);
            }

            if (IsServiceEnabled(enabledServices, "FrontDoor"))
            {
                settings.FrontDoor = LoadFrontDoorSettings(credentialProvider);
            }

            return settings;
        }
        catch (Exception ex) when (!(ex is InvalidOperationException or ArgumentException))
        {
            throw new InvalidOperationException("Failed to load infrastructure settings from credential provider. Please check your configuration.", ex);
        }
    }

    private static bool IsServiceEnabled(HashSet<string>? enabledServices, string serviceName) => enabledServices == null || enabledServices.Contains(serviceName);

    private static bool HasDatabaseCredentials(ICredentialProvider credentialProvider) =>
        !string.IsNullOrWhiteSpace(GetCredentialValue(credentialProvider, EnvironmentVariableNames.Database.AdminPassword));

    private static bool HasCacheCredentials() => true;

    private static bool HasEventHubsCredentials() => true;

    private static bool HasStorageCredentials(ICredentialProvider credentialProvider) =>
        !string.IsNullOrWhiteSpace(GetCredentialValue(credentialProvider, EnvironmentVariableNames.Storage.ReplicationType));

    private static string GetCredentialWithDefault(ICredentialProvider credentialProvider, string key, string defaultValue) =>
        GetCredentialValue(credentialProvider, key) ?? defaultValue;

    private static string GetCredentialWithFallback(ICredentialProvider credentialProvider, string[] keys, string defaultValue)
    {
        foreach (var key in keys)
        {
            var value = GetCredentialValue(credentialProvider, key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }
        return defaultValue;
    }

    private static string GetRequiredCredential(ICredentialProvider credentialProvider, string key) =>
        GetCredentialValue(credentialProvider, key)
        ?? throw new InvalidOperationException($"Required credential '{key}' is not set or is empty.");

    private static string GetRequiredCredentialWithFallback(ICredentialProvider credentialProvider, string[] keys)
    {
        foreach (var key in keys)
        {
            var value = GetCredentialValue(credentialProvider, key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        var keysList = string.Join(", ", keys);
        throw new InvalidOperationException($"None of the required credentials are set: {keysList}");
    }

    private static int GetIntCredential(ICredentialProvider credentialProvider, string key, int defaultValue)
    {
        var value = GetCredentialValue(credentialProvider, key);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    private static double GetDoubleCredential(ICredentialProvider credentialProvider, string key, double defaultValue)
    {
        var value = GetCredentialValue(credentialProvider, key);
        return double.TryParse(value, out var result) ? result : defaultValue;
    }

    private static bool GetBoolCredential(ICredentialProvider credentialProvider, string key, bool defaultValue)
    {
        var value = GetCredentialValue(credentialProvider, key);
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    private static DatabaseSettings LoadDatabaseSettings(ICredentialProvider credentialProvider)
    {
        var adminPassword = GetRequiredCredential(credentialProvider, EnvironmentVariableNames.Database.AdminPassword);
        var adminUsername = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Database.AdminUsername, InfrastructureConstants.Defaults.DatabaseAdminUser);

        return new DatabaseSettings
        {
            AdminUser = adminUsername,
            AdminUsername = adminUsername,
            AdminPassword = adminPassword,
            Password = adminPassword,
            VersionString = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Database.Version, InfrastructureConstants.Defaults.DatabaseVersion),
            StorageSizeGb = GetIntCredential(credentialProvider, EnvironmentVariableNames.Database.StorageSizeGb, InfrastructureConstants.Defaults.DatabaseStorageSizeGb),
            SkuNameString = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Database.SkuName, InfrastructureConstants.Defaults.DatabaseSkuName),
            AvailabilityZone = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Database.AvailabilityZone, InfrastructureConstants.Defaults.DatabaseAvailabilityZone),
            EnableHighAvailability = GetBoolCredential(credentialProvider, EnvironmentVariableNames.Database.EnableHighAvailability, false),
            BackupRetentionDays = GetIntCredential(credentialProvider, EnvironmentVariableNames.Database.BackupRetentionDays, 7),
            EnableGeoRedundantBackup = GetBoolCredential(credentialProvider, EnvironmentVariableNames.Database.EnableGeoRedundantBackup, false)
        };
    }

    private static ContainerSettings LoadContainerSettings(ICredentialProvider credentialProvider) =>
        new()
        {
            ApiImageTag = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Container.ApiImageTag, InfrastructureConstants.Defaults.ApiImageTag),
            JobsImageTag = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Container.JobsImageTag, InfrastructureConstants.Defaults.JobsImageTag),
            CpuLimit = GetDoubleCredential(credentialProvider, EnvironmentVariableNames.Container.CpuLimit, 0.5),
            MemoryLimitString = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Container.MemoryLimit, "1Gi"),
            MinReplicas = GetIntCredential(credentialProvider, EnvironmentVariableNames.Container.MinReplicas, 1),
            MaxReplicas = GetIntCredential(credentialProvider, EnvironmentVariableNames.Container.MaxReplicas, 10),
            EnableDapr = GetBoolCredential(credentialProvider, EnvironmentVariableNames.Container.EnableDapr, false),
            RegistryNameOverride = GetCredentialValue(credentialProvider, EnvironmentVariableNames.Container.RegistryNameOverride)
        };

    private static MonitoringSettings LoadMonitoringSettings(ICredentialProvider credentialProvider)
    {
        var appInsightsTypeString = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Monitoring.ApplicationInsightsType, "web");
        var appInsightsType = appInsightsTypeString.TryToEnum<ApplicationInsightsType>(out var result) ? result : ApplicationInsightsType.Web;

        return new MonitoringSettings
        {
            LogRetentionDays = GetIntCredential(credentialProvider, EnvironmentVariableNames.Monitoring.LogRetentionDays, 30),
            ApplicationInsightsType = appInsightsType,
            EnableDetailedMetrics = GetBoolCredential(credentialProvider, EnvironmentVariableNames.Monitoring.EnableDetailedMetrics, true),
            EnableLiveMetrics = GetBoolCredential(credentialProvider, EnvironmentVariableNames.Monitoring.EnableLiveMetrics, true)
        };
    }

    private static CacheSettings LoadCacheSettings(ICredentialProvider credentialProvider)
    {
        var skuFamilyString = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Cache.SkuFamily, "C");
        var maxMemoryPolicyString = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Cache.MaxMemoryPolicy, "allkeys-lru");

        return new CacheSettings
        {
            SkuNameString = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Cache.SkuName, "Basic"),
            SkuFamily = skuFamilyString switch
            {
                "C" => RedisSkuFamilyType.C,
                "P" => RedisSkuFamilyType.P,
                _ => RedisSkuFamilyType.C
            },
            SkuCapacity = GetIntCredential(credentialProvider, EnvironmentVariableNames.Cache.SkuCapacity, 0),
            EnableNonSslPort = GetBoolCredential(credentialProvider, EnvironmentVariableNames.Cache.EnableNonSslPort, false),
            MaxMemoryPolicy = maxMemoryPolicyString switch
            {
                "noeviction" => RedisMaxMemoryPolicyType.NoEviction,
                "allkeys-lru" => RedisMaxMemoryPolicyType.AllKeysLru,
                "volatile-lru" => RedisMaxMemoryPolicyType.VolatileLru,
                "allkeys-random" => RedisMaxMemoryPolicyType.AllKeysRandom,
                "volatile-random" => RedisMaxMemoryPolicyType.VolatileRandom,
                "volatile-ttl" => RedisMaxMemoryPolicyType.VolatileTtl,
                _ => RedisMaxMemoryPolicyType.AllKeysLru
            }
        };
    }

    private static EventHubsSettings LoadEventHubsSettings(ICredentialProvider credentialProvider)
    {
        var skuNameString = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.EventHubs.SkuName, "Standard");
        var skuName = skuNameString switch
        {
            "Basic" => EventHubsSkuType.Basic,
            "Standard" => EventHubsSkuType.Standard,
            "Premium" => EventHubsSkuType.Premium,
            _ => EventHubsSkuType.Standard
        };

        return new EventHubsSettings
        {
            SkuName = skuName,
            SkuCapacity = GetIntCredential(credentialProvider, EnvironmentVariableNames.EventHubs.SkuCapacity, 1),
            EnableAutoInflate = GetBoolCredential(credentialProvider, EnvironmentVariableNames.EventHubs.EnableAutoInflate, false),
            MaximumThroughputUnits = GetIntCredential(credentialProvider, EnvironmentVariableNames.EventHubs.MaximumThroughputUnits, 20),
            MessageRetentionDays = GetIntCredential(credentialProvider, EnvironmentVariableNames.EventHubs.MessageRetentionDays, 1),
            PartitionCount = GetIntCredential(credentialProvider, EnvironmentVariableNames.EventHubs.PartitionCount, 2)
        };
    }

    private static NetworkSettings LoadNetworkSettings(ICredentialProvider credentialProvider) =>
        new()
        {
            VirtualNetworkAddressSpace = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Network.VirtualNetworkAddressSpace, "10.0.0.0/16"),
            ContainerAppsSubnetAddressSpace = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Network.ContainerAppsSubnetAddressSpace, "10.0.0.0/23"),
            DatabaseSubnetAddressSpace = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Network.DatabaseSubnetAddressSpace, "10.0.2.0/24"),
            CacheSubnetAddressSpace = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Network.CacheSubnetAddressSpace, "10.0.5.0/24"),
            PrivateEndpointsSubnet = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Network.PrivateEndpointsSubnetAddressSpace, "10.0.3.0/24"),
            ApplicationGatewaySubnetAddressSpace = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Network.ApplicationGatewaySubnetAddressSpace, "10.0.4.0/24"),
            EnableDdosProtection = GetBoolCredential(credentialProvider, EnvironmentVariableNames.Network.EnableDdosProtection, false),
            CustomDomain = GetCredentialValue(credentialProvider, EnvironmentVariableNames.Network.CustomDomain)
        };

    private static KeyVaultSettings LoadKeyVaultSettings() => new() { Secrets = new Dictionary<string, string>() };

    private static ApiSettings LoadApiSettings(ICredentialProvider credentialProvider) =>
        new()
        {
            RateLimitRequestsPerMinute = GetIntCredential(credentialProvider, EnvironmentVariableNames.Api.RateLimitRequestsPerMinute, 1000),
            CorsOrigins = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Api.CorsOrigins, "")
        };

    private static GreenBlueDeploymentSettings LoadGreenBlueDeploymentSettings(ICredentialProvider credentialProvider) =>
        new()
        {
            Enabled = GetBoolCredential(credentialProvider, EnvironmentVariableNames.GreenBlueDeployment.Enable, false),
            TrafficSplitPercentage = GetIntCredential(credentialProvider, EnvironmentVariableNames.GreenBlueDeployment.TrafficSplitPercentage, 100),
            HealthCheckPath = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.GreenBlueDeployment.HealthCheckPath, "/health")
        };

    private static SlotSettings LoadSlotSettings(ICredentialProvider credentialProvider, string slotType) =>
        new()
        {
            SlotName = slotType.ToLowerInvariant(),
            TrafficPercentage = GetIntCredential(credentialProvider, $"INFRA_{slotType.ToUpperInvariant()}_SLOT_TRAFFIC_PERCENTAGE", slotType.Equals("green", StringComparison.OrdinalIgnoreCase) ? 100 : 0)
        };

    private static FrontDoorSettings LoadFrontDoorSettings(ICredentialProvider credentialProvider)
    {
        var enabled = GetBoolCredential(credentialProvider, EnvironmentVariableNames.FrontDoor.Enabled, false);
        if (!enabled)
        {
            return new FrontDoorSettings { Enabled = false };
        }

        var allowedIpRangesRaw = GetCredentialValue(credentialProvider, EnvironmentVariableNames.FrontDoor.AllowedIpRanges);
        var bypassPrefixesRaw = GetCredentialValue(credentialProvider, EnvironmentVariableNames.FrontDoor.WafBypassPathPrefixes);

        return new FrontDoorSettings
        {
            Enabled = true,
            SkuName = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.FrontDoor.SkuName, "Standard_AzureFrontDoor"),
            EndpointNameOverride = GetCredentialValue(credentialProvider, EnvironmentVariableNames.FrontDoor.EndpointNameOverride),
            HealthProbePath = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.FrontDoor.HealthProbePath, "/health"),
            EnableCustomDomain = GetBoolCredential(credentialProvider, EnvironmentVariableNames.FrontDoor.EnableCustomDomain, true),
            CustomDomainHostName = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.FrontDoor.CustomDomainHostName, "dev.example.com"),
            EnableWaf = GetBoolCredential(credentialProvider, EnvironmentVariableNames.FrontDoor.EnableWaf, true),
            AllowedIpRanges = string.IsNullOrWhiteSpace(allowedIpRangesRaw)
                ? []
                : allowedIpRangesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            WafBypassPathPrefixes = string.IsNullOrWhiteSpace(bypassPrefixesRaw)
                ? new FrontDoorSettings().WafBypassPathPrefixes
                : bypassPrefixesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
        };
    }

    private static StorageSettings LoadStorageSettings(ICredentialProvider credentialProvider)
    {
        var replicationTypeString = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Storage.ReplicationType, "Standard_LRS");
        var minimumTlsVersionString = GetCredentialWithDefault(credentialProvider, EnvironmentVariableNames.Storage.MinimumTlsVersion, "TLS1_2");

        return new StorageSettings
        {
            ReplicationType = replicationTypeString switch
            {
                "Standard_LRS" => StorageReplicationType.StandardLrs,
                "Standard_GRS" => StorageReplicationType.StandardGrs,
                "Standard_RAGRS" => StorageReplicationType.StandardRagrs,
                "Standard_ZRS" => StorageReplicationType.StandardZrs,
                "Premium_LRS" => StorageReplicationType.PremiumLrs,
                _ => StorageReplicationType.StandardLrs
            },
            EnableHttpsTrafficOnly = GetBoolCredential(credentialProvider, EnvironmentVariableNames.Storage.EnableHttpsTrafficOnly, true),
            MinimumTlsVersion = minimumTlsVersionString switch
            {
                "TLS1_0" => TlsVersionType.Tls10,
                "TLS1_1" => TlsVersionType.Tls11,
                _ => TlsVersionType.Tls12
            }
        };
    }

    private static string? GetCredentialValue(ICredentialProvider credentialProvider, string key)
    {
        return credentialProvider.GetCredential(key);
    }
}

