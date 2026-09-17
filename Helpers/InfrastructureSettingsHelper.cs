using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using DeploymentKit.Settings;
using System.Collections;

namespace DeploymentKit.Helpers;

/// <summary>
/// Loads InfrastructureSettings from environment variables for deployment automation
/// </summary>
public static class InfrastructureSettingsHelper
{
    /// <summary>
    /// Creates InfrastructureSettings from environment variables using INFRA_ prefix convention.
    /// </summary>
    /// <returns>Populated InfrastructureSettings object</returns>
    /// <exception cref="InvalidOperationException">Thrown when required environment variables are missing or invalid</exception>
    /// <exception cref="ArgumentException">Thrown when environment variable values are invalid</exception>
    public static InfrastructureSettings FromEnvironment()
    {
        try
        {
            var settings = new InfrastructureSettings
            {
                Environment = GetEnvironmentVariable(EnvironmentVariableNames.Core.Environment, InfrastructureConstants.Defaults.Environment),
                Location = GetEnvironmentVariableWithFallback([EnvironmentVariableNames.Core.Location, "AZURE_LOCATION", "DEPLOYMENT_LOCATION"], InfrastructureConstants.Defaults.Location),
                NamingPrefix = GetEnvironmentVariable(EnvironmentVariableNames.Core.NamingPrefix, InfrastructureConstants.Defaults.NamingPrefix),
                SubscriptionId = GetRequiredEnvironmentVariableWithFallback([EnvironmentVariableNames.Core.SubscriptionId, "AZURE_SUBSCRIPTION_ID", "ARM_SUBSCRIPTION_ID"]),
                ResourceGroupName = GetRequiredEnvironmentVariable(EnvironmentVariableNames.Core.ResourceGroupName),

                Database = LoadDatabaseSettings(),
                Container = LoadContainerSettings(),
                Monitoring = LoadMonitoringSettings(),
                Cache = LoadCacheSettings(),
                EventHubs = LoadEventHubsSettings(),
                Network = LoadNetworkSettings(),
                KeyVault = LoadKeyVaultSettings(),
                Api = LoadApiSettings(),
                GreenBlueDeployment = LoadGreenBlueDeploymentSettings(),
                GreenSlot = LoadSlotSettings(DeploymentSlotType.Green.ToStringValue()),
                BlueSlot = LoadSlotSettings(DeploymentSlotType.Blue.ToStringValue()),
                FrontDoor = LoadFrontDoorSettings()
            };

            // Optional storage settings
            if (HasStorageEnvironmentVariables())
            {
                settings.Storage = LoadStorageSettings();
            }

            ValidateSettings(settings);
            return settings;
        }
        catch (Exception ex) when (!(ex is InvalidOperationException || ex is ArgumentException))
        {
            throw new InvalidOperationException("Failed to load infrastructure settings from environment variables. Please check your environment configuration.", ex);
        }
    }

    private static DatabaseSettings LoadDatabaseSettings()
    {
        var adminPassword = GetRequiredEnvironmentVariable(EnvironmentVariableNames.Database.AdminPassword);
        var adminUsername = GetEnvironmentVariable(EnvironmentVariableNames.Database.AdminUsername, InfrastructureConstants.Defaults.DatabaseAdminUser);
        var versionString = GetEnvironmentVariable(EnvironmentVariableNames.Database.Version, InfrastructureConstants.Defaults.DatabaseVersion);
        var skuNameString = GetEnvironmentVariable(EnvironmentVariableNames.Database.SkuName, InfrastructureConstants.Defaults.DatabaseSkuName);

        var settings = new DatabaseSettings
        {
            AdminUser = adminUsername,
            AdminUsername = adminUsername,
            AdminPassword = adminPassword,
            Password = adminPassword,
            StorageSizeGb = GetIntEnvironmentVariable(EnvironmentVariableNames.Database.StorageSizeGb, InfrastructureConstants.Defaults.DatabaseStorageSizeGb),
            AvailabilityZone = GetEnvironmentVariable(EnvironmentVariableNames.Database.AvailabilityZone, InfrastructureConstants.Defaults.DatabaseAvailabilityZone),
            EnableHighAvailability = GetBoolEnvironmentVariable(EnvironmentVariableNames.Database.EnableHighAvailability, false),
            BackupRetentionDays = GetIntEnvironmentVariable(EnvironmentVariableNames.Database.BackupRetentionDays, 7),
            EnableGeoRedundantBackup = GetBoolEnvironmentVariable(EnvironmentVariableNames.Database.EnableGeoRedundantBackup, false)
        };

        // Set version string and enum - this will synchronize both properties
        settings.VersionString = versionString;

        // Set SKU name string and enum - this will synchronize both properties
        settings.SkuNameString = skuNameString;

        return settings;
    }

    private static ContainerSettings LoadContainerSettings()
    {
        var settings = new ContainerSettings
        {
            ApiImageTag = GetEnvironmentVariable(EnvironmentVariableNames.Container.ApiImageTag, InfrastructureConstants.Defaults.ApiImageTag),
            JobsImageTag = GetEnvironmentVariable(EnvironmentVariableNames.Container.JobsImageTag, InfrastructureConstants.Defaults.JobsImageTag),
            CpuLimit = GetDoubleEnvironmentVariable(EnvironmentVariableNames.Container.CpuLimit, 0.5),
            MemoryLimitString = GetEnvironmentVariable(EnvironmentVariableNames.Container.MemoryLimit, "1Gi"),
            MinReplicas = GetIntEnvironmentVariable(EnvironmentVariableNames.Container.MinReplicas, 1),
            MaxReplicas = GetIntEnvironmentVariable(EnvironmentVariableNames.Container.MaxReplicas, 10),
            EnableDapr = GetBoolEnvironmentVariable(EnvironmentVariableNames.Container.EnableDapr, false),
            RegistryNameOverride = GetEnvironmentVariable(EnvironmentVariableNames.Container.RegistryNameOverride, null)
        };

        // Load ingress settings if IP restrictions are provided
        var ipRestrictions = GetEnvironmentVariable(EnvironmentVariableNames.Container.IngressIpRestrictions, null);
        if (!string.IsNullOrWhiteSpace(ipRestrictions))
        {
            settings.IngressSettings = new IngressSettings
            {
                External = GetBoolEnvironmentVariable(EnvironmentVariableNames.Container.IngressExternal, true),
                TargetPort = GetIntEnvironmentVariable(EnvironmentVariableNames.Container.IngressTargetPort, 8080),
                AllowInsecure = GetBoolEnvironmentVariable(EnvironmentVariableNames.Container.IngressAllowInsecure, false),
                IpSecurityRestrictions = ParseIpRestrictions(ipRestrictions)
            };
        }

        return settings;
    }

    private static List<IpSecurityRestrictionSettings> ParseIpRestrictions(string ipRestrictionsString)
    {
        var restrictions = new List<IpSecurityRestrictionSettings>();
        var ipRanges = ipRestrictionsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (int i = 0; i < ipRanges.Length; i++)
        {
            var range = ipRanges[i].Trim();
            restrictions.Add(new IpSecurityRestrictionSettings
            {
                Name = $"AllowedIP{i + 1}",
                IpAddressRange = range,
                Action = "Allow",
                Description = $"Allowed IP range from environment configuration"
            });
        }

        return restrictions;
    }

    private static MonitoringSettings LoadMonitoringSettings()
    {
        var appInsightsTypeString = GetEnvironmentVariable(EnvironmentVariableNames.Monitoring.ApplicationInsightsType, InfrastructureConstants.Monitoring.DefaultApplicationInsightsType);
        var appInsightsType = appInsightsTypeString.TryToEnum<ApplicationInsightsType>(out var result) ? result : ApplicationInsightsType.Web;

        return new MonitoringSettings
        {
            LogRetentionDays = GetIntEnvironmentVariable(EnvironmentVariableNames.Monitoring.LogRetentionDays, InfrastructureConstants.Monitoring.DefaultLogRetentionDays),
            ApplicationInsightsType = appInsightsType,
            EnableDetailedMetrics = GetBoolEnvironmentVariable(EnvironmentVariableNames.Monitoring.EnableDetailedMetrics, true),
            EnableLiveMetrics = GetBoolEnvironmentVariable(EnvironmentVariableNames.Monitoring.EnableLiveMetrics, true)
        };
    }

    private static CacheSettings LoadCacheSettings()
    {
        var skuFamilyString = GetEnvironmentVariable(EnvironmentVariableNames.Cache.SkuFamily, InfrastructureConstants.Redis.BasicStandardFamily);
        var maxMemoryPolicyString = GetEnvironmentVariable(EnvironmentVariableNames.Cache.MaxMemoryPolicy, "allkeys-lru");

        return new CacheSettings
        {
            SkuNameString = GetEnvironmentVariable(EnvironmentVariableNames.Cache.SkuName, InfrastructureConstants.Redis.BasicSku),
            SkuFamily = skuFamilyString switch
            {
                "C" => RedisSkuFamilyType.C,
                "P" => RedisSkuFamilyType.P,
                _ => RedisSkuFamilyType.C
            },
            SkuCapacity = GetIntEnvironmentVariable(EnvironmentVariableNames.Cache.SkuCapacity, InfrastructureConstants.Defaults.RedisSkuCapacity),
            EnableNonSslPort = GetBoolEnvironmentVariable(EnvironmentVariableNames.Cache.EnableNonSslPort, InfrastructureConstants.Redis.DefaultEnableNonSslPort),
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

    private static EventHubsSettings LoadEventHubsSettings()
    {
        return new EventHubsSettings
        {
            SkuNameString = GetEnvironmentVariable(EnvironmentVariableNames.EventHubs.SkuName, "Standard"),
            SkuCapacity = GetIntEnvironmentVariable(EnvironmentVariableNames.EventHubs.SkuCapacity, 1),
            EnableAutoInflate = GetBoolEnvironmentVariable(EnvironmentVariableNames.EventHubs.EnableAutoInflate, false),
            MaximumThroughputUnits = GetIntEnvironmentVariable(EnvironmentVariableNames.EventHubs.MaximumThroughputUnits, 20),
            MessageRetentionDays = GetIntEnvironmentVariable(EnvironmentVariableNames.EventHubs.MessageRetentionDays, 1),
            PartitionCount = GetIntEnvironmentVariable(EnvironmentVariableNames.EventHubs.PartitionCount, 2)
        };
    }

    private static NetworkSettings LoadNetworkSettings()
    {
        var vnetAddressSpace = GetEnvironmentVariable(EnvironmentVariableNames.Network.VirtualNetworkAddressSpace, "10.0.0.0/16");
        var containerAppsSubnet = GetEnvironmentVariable(EnvironmentVariableNames.Network.ContainerAppsSubnetAddressSpace, "10.0.0.0/23");
        var databaseSubnet = GetEnvironmentVariable(EnvironmentVariableNames.Network.DatabaseSubnetAddressSpace, "10.0.2.0/24");
        var cacheSubnet = GetEnvironmentVariable(EnvironmentVariableNames.Network.CacheSubnetAddressSpace, "10.0.5.0/24");
        var appGatewaySubnet = GetEnvironmentVariable(EnvironmentVariableNames.Network.ApplicationGatewaySubnetAddressSpace, "10.0.4.0/24");
        var privateEndpointsSubnet = GetEnvironmentVariable(EnvironmentVariableNames.Network.PrivateEndpointsSubnetAddressSpace, "10.0.3.0/24");

        return new NetworkSettings
        {
            VirtualNetworkAddressSpace = vnetAddressSpace,
            VNetAddressSpace = vnetAddressSpace,
            ContainerAppsSubnetAddressSpace = containerAppsSubnet,
            ContainerAppsSubnet = containerAppsSubnet,
            DatabaseSubnetAddressSpace = databaseSubnet,
            DatabaseSubnet = databaseSubnet,
            CacheSubnetAddressSpace = cacheSubnet,
            ApplicationGatewaySubnetAddressSpace = appGatewaySubnet,
            ApplicationGatewaySubnet = appGatewaySubnet,
            PrivateEndpointsSubnet = privateEndpointsSubnet,
            EnableDdosProtection = GetBoolEnvironmentVariable(EnvironmentVariableNames.Network.EnableDdosProtection, false),
            CustomDomain = GetEnvironmentVariable(EnvironmentVariableNames.Network.CustomDomain, "api.example.com"),
            SslCertificateName = GetEnvironmentVariable(EnvironmentVariableNames.Network.SslCertificateName, "app-ssl-cert")
        };
    }

    private static KeyVaultSettings LoadKeyVaultSettings()
    {
        return new KeyVaultSettings
        {
            SkuNameString = GetEnvironmentVariable(EnvironmentVariableNames.KeyVault.SkuName, "standard"),
            EnableSoftDelete = GetBoolEnvironmentVariable(EnvironmentVariableNames.KeyVault.EnableSoftDelete, true),
            SoftDeleteRetentionDays = GetIntEnvironmentVariable(EnvironmentVariableNames.KeyVault.SoftDeleteRetentionDays, 90),
            EnablePurgeProtection = GetBoolEnvironmentVariable(EnvironmentVariableNames.KeyVault.EnablePurgeProtection, false),
            EnableRbacAuthorization = GetBoolEnvironmentVariable(EnvironmentVariableNames.KeyVault.EnableRbacAuthorization, true)
        };
    }

    private static ApiSettings LoadApiSettings()
    {
        return new ApiSettings
        {
            EnableHttps = GetBoolEnvironmentVariable(EnvironmentVariableNames.Api.EnableHttps, true),
            EnableSwagger = GetBoolEnvironmentVariable(EnvironmentVariableNames.Api.EnableSwagger, true),
            CorsOrigins = GetEnvironmentVariable(EnvironmentVariableNames.Api.CorsOrigins, "https://example.com,https://dev.example.com"),
            RateLimitRequestsPerMinute = GetIntEnvironmentVariable(EnvironmentVariableNames.Api.RateLimitRequestsPerMinute, 100),
            EnableDetailedErrors = GetBoolEnvironmentVariable(EnvironmentVariableNames.Api.EnableDetailedErrors, false)
        };
    }

    private static GreenBlueDeploymentSettings LoadGreenBlueDeploymentSettings()
    {
        return new GreenBlueDeploymentSettings
        {
            EnableGreenBlueDeployment = GetBoolEnvironmentVariable(EnvironmentVariableNames.GreenBlueDeployment.Enable, true),
            TrafficSplitPercentage = GetIntEnvironmentVariable(EnvironmentVariableNames.GreenBlueDeployment.TrafficSplitPercentage, 50),
            HealthCheckPath = GetEnvironmentVariable(EnvironmentVariableNames.GreenBlueDeployment.HealthCheckPath, "/health"),
            HealthCheckIntervalSeconds = GetIntEnvironmentVariable(EnvironmentVariableNames.GreenBlueDeployment.HealthCheckIntervalSeconds, 30),
            HealthCheckTimeoutSeconds = GetIntEnvironmentVariable(EnvironmentVariableNames.GreenBlueDeployment.HealthCheckTimeoutSeconds, 10)
        };
    }

    private static SlotSettings LoadSlotSettings(string slotType)
    {
        var prefix = slotType.ToUpperInvariant() == "GREEN" ? EnvironmentVariableNames.Slot.GreenPrefix : EnvironmentVariableNames.Slot.BluePrefix;
        return new SlotSettings
        {
            SlotName = slotType.ToLowerInvariant(),
            ImageTag = GetEnvironmentVariable($"{prefix}{EnvironmentVariableNames.Slot.ImageTagSuffix}", InfrastructureConstants.Defaults.ApiImageTag),
            CpuLimit = GetDoubleEnvironmentVariable($"{prefix}{EnvironmentVariableNames.Slot.CpuLimitSuffix}", 0.5),
            MemoryLimitString = GetEnvironmentVariable($"{prefix}{EnvironmentVariableNames.Slot.MemoryLimitSuffix}", "1Gi"),
            MinReplicas = GetIntEnvironmentVariable($"{prefix}{EnvironmentVariableNames.Slot.MinReplicasSuffix}", 1),
            MaxReplicas = GetIntEnvironmentVariable($"{prefix}{EnvironmentVariableNames.Slot.MaxReplicasSuffix}", 5),
            EnvironmentVariables = LoadSlotEnvironmentVariables(slotType)
        };
    }

    private static FrontDoorSettings LoadFrontDoorSettings()
    {
        var enabled = GetBoolEnvironmentVariable(EnvironmentVariableNames.FrontDoor.Enabled, false);
        if (!enabled)
        {
            return new FrontDoorSettings { Enabled = false };
        }

        var allowedIpRanges = GetEnvironmentVariable(EnvironmentVariableNames.FrontDoor.AllowedIpRanges, null);
        var bypassPrefixes = GetEnvironmentVariable(EnvironmentVariableNames.FrontDoor.WafBypassPathPrefixes, null);

        return new FrontDoorSettings
        {
            Enabled = true,
            SkuName = GetEnvironmentVariable(EnvironmentVariableNames.FrontDoor.SkuName, "Standard_AzureFrontDoor"),
            EndpointNameOverride = GetEnvironmentVariable(EnvironmentVariableNames.FrontDoor.EndpointNameOverride, null),
            HealthProbePath = GetEnvironmentVariable(EnvironmentVariableNames.FrontDoor.HealthProbePath, "/health"),
            EnableCustomDomain = GetBoolEnvironmentVariable(EnvironmentVariableNames.FrontDoor.EnableCustomDomain, true),
            CustomDomainHostName = GetEnvironmentVariable(EnvironmentVariableNames.FrontDoor.CustomDomainHostName, "dev.example.com"),
            EnableWaf = GetBoolEnvironmentVariable(EnvironmentVariableNames.FrontDoor.EnableWaf, true),
            AllowedIpRanges = string.IsNullOrWhiteSpace(allowedIpRanges)
                ? []
                : allowedIpRanges.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            WafBypassPathPrefixes = string.IsNullOrWhiteSpace(bypassPrefixes)
                ? new FrontDoorSettings().WafBypassPathPrefixes
                : bypassPrefixes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
        };
    }

    private static Dictionary<string, string> LoadSlotEnvironmentVariables(string slotType)
    {
        var envVars = new Dictionary<string, string>();
        var prefix = slotType.ToUpperInvariant() == "GREEN"
            ? $"{EnvironmentVariableNames.Slot.GreenPrefix}{EnvironmentVariableNames.Slot.EnvironmentVariablePrefix}"
            : $"{EnvironmentVariableNames.Slot.BluePrefix}{EnvironmentVariableNames.Slot.EnvironmentVariablePrefix}";

        foreach (DictionaryEntry envVar in Environment.GetEnvironmentVariables())
        {
            var key = envVar.Key as string;
            var value = envVar.Value as string;

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value) && key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var actualKey = key.Substring(prefix.Length);
                envVars[actualKey] = value;
            }
        }

        return envVars;
    }

    private static bool HasStorageEnvironmentVariables()
    {
        return !string.IsNullOrEmpty(GetEnvironmentVariableValue(EnvironmentVariableNames.Storage.ReplicationType)) ||
               !string.IsNullOrEmpty(GetEnvironmentVariableValue(EnvironmentVariableNames.Storage.EnableBlobPublicAccess)) ||
               !string.IsNullOrEmpty(GetEnvironmentVariableValue(EnvironmentVariableNames.Storage.EnableHttpsTrafficOnly)) ||
               !string.IsNullOrEmpty(GetEnvironmentVariableValue(EnvironmentVariableNames.Storage.MinimumTlsVersion)) ||
               !string.IsNullOrEmpty(GetEnvironmentVariableValue(EnvironmentVariableNames.Storage.EnableVersioning));
    }

    private static StorageSettings LoadStorageSettings()
    {
        var replicationTypeString = GetEnvironmentVariable(EnvironmentVariableNames.Storage.ReplicationType, "Standard_LRS");
        var minimumTlsVersionString = GetEnvironmentVariable(EnvironmentVariableNames.Storage.MinimumTlsVersion, "TLS1_2");

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
            AllowBlobPublicAccess = GetBoolEnvironmentVariable(EnvironmentVariableNames.Storage.EnableBlobPublicAccess, false),
            EnableHttpsTrafficOnly = GetBoolEnvironmentVariable(EnvironmentVariableNames.Storage.EnableHttpsTrafficOnly, true),
            MinimumTlsVersion = minimumTlsVersionString switch
            {
                "TLS1_0" => TlsVersionType.Tls10,
                "TLS1_1" => TlsVersionType.Tls11,
                _ => TlsVersionType.Tls12
            },
            EnableVersioning = GetBoolEnvironmentVariable(EnvironmentVariableNames.Storage.EnableVersioning, false)
        };
    }

    private static string GetRequiredEnvironmentVariable(string name)
    {
        var value = GetEnvironmentVariableValue(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Required environment variable '{name}' is not set or is empty.", nameof(name));
        }
        return value;
    }

    private static string GetRequiredEnvironmentVariableWithFallback(string[] names)
    {
        if (names == null || names.Length == 0)
        {
            throw new ArgumentException("At least one environment variable name must be provided.", nameof(names));
        }

        foreach (var name in names)
        {
            var value = GetEnvironmentVariableValue(name);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        var namesList = string.Join(", ", names);
        throw new InvalidOperationException($"None of the required environment variables are set: {namesList}");
    }

    private static string GetEnvironmentVariableWithFallback(string[] names, string defaultValue)
    {
        if (names == null || names.Length == 0)
        {
            throw new ArgumentException("At least one environment variable name must be provided.", nameof(names));
        }

        foreach (var name in names)
        {
            var value = GetEnvironmentVariableValue(name);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return defaultValue;
    }

    private static string GetEnvironmentVariable(string name, string? defaultValue)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Environment variable name cannot be null or empty.", nameof(name));
        }

        var value = GetEnvironmentVariableValue(name);
        return (string.IsNullOrWhiteSpace(value) ? defaultValue : value) ?? throw new InvalidOperationException("Environment variable name is not set.");
    }

    private static int GetIntEnvironmentVariable(string name, int defaultValue)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Environment variable name cannot be null or empty.", nameof(name));
        }

        var value = GetEnvironmentVariableValue(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new FormatException($"Environment variable '{name}' has invalid integer value: '{value}'");
    }

    private static double GetDoubleEnvironmentVariable(string name, double defaultValue)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Environment variable name cannot be null or empty.", nameof(name));
        }

        var value = GetEnvironmentVariableValue(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new FormatException($"Environment variable '{name}' has invalid double value: '{value}'");
    }

    private static bool GetBoolEnvironmentVariable(string name, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Environment variable name cannot be null or empty.", nameof(name));
        }

        var value = GetEnvironmentVariableValue(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        // Support various boolean representations
        var normalizedValue = value.Trim().ToLowerInvariant();
        return normalizedValue switch
        {
            "true" or "1" or "yes" or "on" or "enabled" => true,
            "false" or "0" or "no" or "off" or "disabled" => false,
            _ => throw new FormatException($"Environment variable '{name}' has invalid boolean value: '{value}'. Valid values are: true/false, 1/0, yes/no, on/off, enabled/disabled")
        };
    }

    private static string? GetEnvironmentVariableValue(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }

    private static void ValidateSettings(InfrastructureSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings), "Infrastructure settings cannot be null");
        }

        // Validate core settings
        if (string.IsNullOrWhiteSpace(settings.Environment))
        {
            throw new ArgumentException("Environment cannot be null or empty", nameof(settings));
        }

        if (string.IsNullOrWhiteSpace(settings.Location))
        {
            throw new ArgumentException("Location cannot be null or empty", nameof(settings));
        }

        if (string.IsNullOrWhiteSpace(settings.ResourceGroupName))
        {
            throw new ArgumentException("Resource group name cannot be null or empty", nameof(settings));
        }

        if (string.IsNullOrWhiteSpace(settings.NamingPrefix))
        {
            throw new ArgumentException("Naming prefix cannot be null or empty", nameof(settings));
        }

        // Validate database settings
        if (settings.Database != null)
        {
            ValidateDatabaseSettings(settings.Database);
        }

        // Validate container settings
        if (settings.Container != null)
        {
            ValidateContainerSettings(settings.Container);
        }

        // Validate monitoring settings
        if (settings.Monitoring != null)
        {
            ValidateMonitoringSettings(settings.Monitoring);
        }

        // Validate cache settings
        if (settings.Cache != null)
        {
            ValidateCacheSettings(settings.Cache);
        }

        // Validate network settings
        if (settings.Network != null)
        {
            ValidateNetworkSettings(settings.Network);
        }

        // Validate API settings
        ValidateApiSettings(settings.Api);
    }

    private static void ValidateDatabaseSettings(DatabaseSettings database)
    {
        if (database == null)
        {
            throw new ArgumentException("Database settings cannot be null");
        }

        if (string.IsNullOrWhiteSpace(database.AdminUsername))
        {
            throw new ArgumentException("Database admin username cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(database.AdminPassword))
        {
            throw new ArgumentException("Database admin password cannot be null or empty");
        }

        if (database.AdminPassword.Length < 8)
        {
            throw new ArgumentException("Database admin password must be at least 8 characters long");
        }
    }

    private static void ValidateContainerSettings(ContainerSettings container)
    {
        if (container == null)
        {
            throw new ArgumentException("Container settings cannot be null");
        }

        if (string.IsNullOrWhiteSpace(container.ApiImageTag))
        {
            throw new ArgumentException("API image tag cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(container.JobsImageTag))
        {
            throw new ArgumentException("Jobs image tag cannot be null or empty");
        }

        if (!string.IsNullOrWhiteSpace(container.RegistryNameOverride))
        {
            if (container.RegistryNameOverride.Length < 5 || container.RegistryNameOverride.Length > 50)
            {
                throw new ArgumentException("Registry name override must be between 5 and 50 characters");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(container.RegistryNameOverride, @"^[a-zA-Z0-9]+$"))
            {
                throw new ArgumentException("Registry name override must contain only alphanumeric characters");
            }
        }
    }

    private static void ValidateMonitoringSettings(MonitoringSettings monitoring)
    {
        if (monitoring == null)
        {
            throw new ArgumentException("Monitoring settings cannot be null");
        }

        if (monitoring.LogRetentionDays <= 0)
        {
            throw new ArgumentException("Log retention days must be greater than 0");
        }
    }

    private static void ValidateCacheSettings(CacheSettings cache)
    {
        if (cache == null)
        {
            throw new ArgumentException("Cache settings cannot be null");
        }

        if (cache.SkuCapacity < 0 || cache.SkuCapacity > 6)
        {
            throw new ArgumentException("Cache SKU capacity must be between 0 and 6 (C0-C6 for Basic/Standard or P0-P6 for Premium)");
        }
    }

    private static void ValidateNetworkSettings(NetworkSettings network)
    {
        if (network == null)
        {
            throw new ArgumentException("Network settings cannot be null");
        }

        // Custom domain validation is optional, but if provided should be valid
        if (!string.IsNullOrWhiteSpace(network.CustomDomain))
        {
            if (!Uri.TryCreate($"https://{network.CustomDomain}", UriKind.Absolute, out _))
            {
                throw new ArgumentException($"Invalid custom domain format: {network.CustomDomain}");
            }
        }
    }

    private static void ValidateApiSettings(ApiSettings api)
    {
        if (api == null)
        {
            throw new ArgumentException("API settings cannot be null");
        }

        if (api.RateLimitRequestsPerMinute <= 0)
        {
            throw new ArgumentException("API rate limit requests per minute must be greater than 0");
        }
    }
}


