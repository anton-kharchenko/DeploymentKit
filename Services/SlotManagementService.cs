using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Exceptions;
using DeploymentKit.Extensions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing container app slots.
/// </summary>
public class SlotManagementService : ISlotManagementService
{
    private readonly ILogger<SlotManagementService> _logger;
    private readonly IResourceNamingService _namingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotManagementService"/> class.
    /// </summary>
    public SlotManagementService(
        ILogger<SlotManagementService> logger,
        IResourceNamingService namingService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
    }

    /// <inheritdoc/>
    public Task<SlotOutputs> CreateSlotContainerAppAsync(
        string slotName,
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        ManagedEnvironment environment,
        ContainerRegistryOutputs containerRegistry,
        DatabaseOutputs database,
        CacheOutputs cache,
        MonitoringOutputs monitoring,
        SlotSettings slotSettings)
    {
        var appName = _namingService.GenerateContainerAppName(settings.NamingPrefix, $"{GreenBlueConstants.ApiPrefix}{slotName}", settings.Environment);

        var containerApp = new ContainerApp(appName, new ContainerAppArgs
        {
            ResourceGroupName = resourceGroup,
            Location = settings.Location,
            ManagedEnvironmentId = environment.Id,
            Identity = new ManagedServiceIdentityArgs
            {
                Type = ManagedServiceIdentityType.SystemAssigned
            },
            Configuration = new ConfigurationArgs
            {
                Ingress = new IngressArgs
                {
                    External = true,
                    TargetPort = 8080,
                    AllowInsecure = false, // Secure by default
                    Traffic = new[]
                    {
                        new TrafficWeightArgs
                        {
                            Weight = slotSettings.TrafficPercentage,
                            LatestRevision = true
                        }
                    }
                },
                Registries = new[]
                {
                    new RegistryCredentialsArgs
                    {
                        Server = containerRegistry.LoginServer,
                        Username = containerRegistry.Username,
                        PasswordSecretRef = ContainerAppConstants.AcrPasswordSecretName
                    }
                },
                Secrets = new[]
                {
                    new SecretArgs { Name = ContainerAppConstants.AcrPasswordSecretName, Value = containerRegistry.Password },
                    new SecretArgs { Name = ContainerAppConstants.DbPasswordSecretName, Value = Output.CreateSecret(settings.Database?.Password ?? throw new InvalidOperationException()) },
                    new SecretArgs { Name = ContainerAppConstants.PostgresConnectionStringSecretName, Value = database.ConnectionString }
                }
            },
            Template = new TemplateArgs
            {
                Containers = new[]
                {
                    new ContainerArgs
                    {
                        Name = $"{GreenBlueConstants.ApiPrefix}{slotName}",
                        Image = Output.Format($"{containerRegistry.LoginServer}/{slotSettings.ImageTag}"),
                        Env = CreateEnvironmentVariables(settings, cache, monitoring, slotSettings),
                        Resources = new ContainerResourcesArgs
                        {
                            Cpu = slotSettings.CpuAllocation,
                            Memory = slotSettings.MemoryAllocation
                        }
                    }
                },
                Scale = new ScaleArgs
                {
                    MinReplicas = slotSettings.MinReplicas,
                    MaxReplicas = slotSettings.MaxReplicas,
                    Rules = new[]
                    {
                        new ScaleRuleArgs
                        {
                            Name = ContainerAppConstants.HttpScalingRuleName,
                            Http = new HttpScaleRuleArgs
                            {
                                Metadata = { [ContainerAppConstants.ConcurrentRequestsMetadata] = ContainerAppConstants.DefaultConcurrentRequests }
                            }
                        },
                        new ScaleRuleArgs
                        {
                            Name = ContainerAppConstants.CpuScalingRuleName,
                            Custom = new CustomScaleRuleArgs
                            {
                                Type = ContainerAppConstants.CpuScalingType,
                                Metadata =
                                {
                                    ["type"] = ContainerAppConstants.CpuUtilizationType,
                                    ["value"] = ContainerAppConstants.DefaultCpuThreshold
                                }
                            }
                        }
                    }
                }
            },
            Tags = ResourceTagHelper.GetStandardTags(settings.Environment, $"{GreenBlueConstants.ContainerAppPrefix}{slotName}")
        });

        return Task.FromResult(new SlotOutputs
        {
            SlotName = slotName,
            AppName = appName,
            AppId = containerApp.Id,
            AppUrl = containerApp.Configuration.Apply(c => $"{ContainerAppConstants.HttpsScheme}{c?.Ingress?.Fqdn}"),
            InternalFqdn = containerApp.Configuration.Apply(c => c?.Ingress?.Fqdn ?? string.Empty),
            TrafficPercentage = slotSettings.TrafficPercentage,
            IsActive = slotSettings.IsActive,
            ImageTag = slotSettings.ImageTag,
            DeploymentTimestamp = DateTime.UtcNow,
            Version = slotSettings.VersionString,
            HealthCheckUrl = containerApp.Configuration.Apply(c => $"{ContainerAppConstants.HttpsScheme}{c?.Ingress?.Fqdn}{settings.GreenBlueDeployment?.HealthCheckPath}"),
            IsHealthy = false
        });
    }

    /// <inheritdoc/>
    public Task<SlotOutputs> UpdateSlotAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        string slotName,
        string imageTag,
        Dictionary<string, string>? environmentVariables = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Updating slot {SlotName} with image tag {ImageTag}", slotName, imageTag);

            // Validate slot name
            if (!slotName.Equals(DeploymentSlotType.Green.ToStringValue(), StringComparison.OrdinalIgnoreCase) &&
                !slotName.Equals(DeploymentSlotType.Blue.ToStringValue(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid slot name: {slotName}. Must be '{DeploymentSlotType.Green.ToStringValue()}' or '{DeploymentSlotType.Blue.ToStringValue()}'.", nameof(slotName));
            }

            // Generate container app name for the slot
            var appName = _namingService.GenerateContainerAppName(settings.NamingPrefix, $"{GreenBlueConstants.ApiPrefix}{slotName.ToLowerInvariant()}", settings.Environment);

            _logger.LogDebug("Updating container app: {AppName}", appName);

            // Build environment variables
            var envVars = new List<EnvironmentVarArgs>
            {
                new() { Name = ContainerAppConstants.AspNetCoreUrls, Value = ContainerAppConstants.DefaultAspNetCoreUrls },
                new() { Name = ContainerAppConstants.AspNetCoreEnvironment, Value = settings.Environment },
                new() { Name = ContainerAppConstants.SlotName, Value = slotName.ToLowerInvariant() },
                new() { Name = ContainerAppConstants.DeploymentTimestamp, Value = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture) }
            };

            // Add custom environment variables if provided
            if (environmentVariables != null)
            {
                envVars.AddRange(environmentVariables.Select(envVar => new EnvironmentVarArgs { Name = envVar.Key, Value = envVar.Value }));
            }

            // Get the managed environment name
            var environmentName = _namingService.GenerateContainerAppsEnvironmentName(settings.NamingPrefix, settings.Environment);

            // Create or update the container app for this slot
            var containerApp = new ContainerApp(appName, new ContainerAppArgs
            {
                ResourceGroupName = resourceGroup,
                Location = settings.Location,
                ManagedEnvironmentId = Output.Format($"/subscriptions/{settings.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.App/managedEnvironments/{environmentName}"),
                Identity = new ManagedServiceIdentityArgs
                {
                    Type = ManagedServiceIdentityType.SystemAssigned
                },
                Template = new TemplateArgs
                {
                    Containers = new[]
                    {
                        new ContainerArgs
                        {
                            Name = $"{GreenBlueConstants.ApiPrefix}{slotName.ToLowerInvariant()}",
                            Image = imageTag,
                            Env = envVars.ToArray(),
                            Resources = new ContainerResourcesArgs
                            {
                                Cpu = double.Parse(GreenBlueConstants.DefaultCpuAllocation, System.Globalization.CultureInfo.InvariantCulture),
                                Memory = GreenBlueConstants.DefaultMemoryAllocation
                            }
                        }
                    },
                    Scale = new ScaleArgs
                    {
                        MinReplicas = 1,
                        MaxReplicas = 10
                    }
                },
                Configuration = new ConfigurationArgs
                {
                    Ingress = new IngressArgs
                    {
                        External = true,
                        TargetPort = 8080,
                        Transport = NetworkProtocolType.Http.ToStringValue(),
                        AllowInsecure = false,
                        Traffic = new[]
                        {
                            new TrafficWeightArgs
                            {
                                Weight = ContainerAppConstants.FullTrafficPercentage,
                                LatestRevision = true
                            }
                        }
                    }
                },
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, ContainerAppConstants.ContainerAppType, new Dictionary<string, string>
                {
                    [ContainerAppConstants.SlotKey] = slotName.ToLowerInvariant(),
                    [ContainerAppConstants.ImageTagKey] = imageTag,
                    [ContainerAppConstants.DeploymentTypeKey] = ContainerAppConstants.GreenBlueDeploymentType
                })
            });

            // Create slot outputs
            var slotOutputs = new SlotOutputs
            {
                SlotName = slotName.ToLowerInvariant(),
                AppName = appName,
                AppId = containerApp.Id,
                AppUrl = containerApp.Configuration.Apply(c => c?.Ingress?.Fqdn ?? string.Empty),
                InternalFqdn = containerApp.Configuration.Apply(c => c?.Ingress?.Fqdn ?? string.Empty),
                ImageTag = imageTag,
                Version = imageTag,
                DeploymentTimestamp = DateTime.UtcNow,
                IsHealthy = false, // Will be determined by health checks
                HealthCheckUrl = containerApp.Configuration.Apply(c => !string.IsNullOrEmpty(c?.Ingress?.Fqdn) ? $"{ContainerAppConstants.HttpsScheme}{c.Ingress.Fqdn}/health" : string.Empty)
            };

            _logger.LogInformation("Successfully updated slot {SlotName} with image tag {ImageTag}", slotName, imageTag);
            return Task.FromResult(slotOutputs);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Slot update operation was cancelled for slot: {SlotName}", slotName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update slot {SlotName} with image tag {ImageTag}", slotName, imageTag);
            throw new ResourceCreationException($"Failed to update slot {slotName}: {ex.Message}", ex);
        }
    }

    private static EnvironmentVarArgs[] CreateEnvironmentVariables(InfrastructureSettings settings, CacheOutputs cache, MonitoringOutputs monitoring, SlotSettings slotSettings)
    {
        var baseEnvVars = new List<EnvironmentVarArgs>
        {
            new() { Name = ContainerAppConstants.AspNetCoreUrls, Value = ContainerAppConstants.DefaultAspNetCoreUrls },
            new() { Name = ContainerAppConstants.AspNetCoreEnvironment, Value = settings.Environment },
            new() { Name = ContainerAppConstants.ConnectionStringsDb, SecretRef = ContainerAppConstants.PostgresConnectionStringSecretName },
            new() { Name = ContainerAppConstants.ConnectionStringsRedis, Value = cache.ConnectionString },
            new() { Name = ContainerAppConstants.ApplicationInsightsConnectionString, Value = monitoring.ApplicationInsightsConnectionString },
            new() { Name = ContainerAppConstants.OtelExporterOtlpEndpoint, Value = ContainerAppConstants.LocalhostOtlpEndpoint },
            new() { Name = ContainerAppConstants.DeploymentSlot, Value = slotSettings.SlotName },
            new() { Name = ContainerAppConstants.DeploymentVersion, Value = slotSettings.VersionString }
        };
        baseEnvVars.AddRange(slotSettings.EnvironmentVariables.Select(envVar => new EnvironmentVarArgs { Name = envVar.Key, Value = envVar.Value }));

        return baseEnvVars.ToArray();
    }
}



