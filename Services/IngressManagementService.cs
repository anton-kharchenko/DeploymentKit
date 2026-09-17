using DeploymentKit.Constants;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing ingress.
/// </summary>
public class IngressManagementService : IIngressManagementService
{
    private readonly IResourceNamingService _namingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IngressManagementService"/> class.
    /// </summary>
    public IngressManagementService(IResourceNamingService namingService)
    {
        _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
    }

    /// <inheritdoc/>
    public Task<ContainerApp> CreateMainIngressAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        ManagedEnvironment environment,
        SlotOutputs greenSlot,
        SlotOutputs blueSlot)
    {
        var mainAppName = _namingService.GenerateContainerAppName(settings.NamingPrefix, GreenBlueConstants.MainApiName, settings.Environment);

        return Task.FromResult(new ContainerApp(mainAppName, new ContainerAppArgs
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
                            Weight = settings.GreenSlot.TrafficPercentage,
                            RevisionName = greenSlot.AppName
                        },
                        new TrafficWeightArgs
                        {
                            Weight = settings.BlueSlot.TrafficPercentage,
                            RevisionName = blueSlot.AppName
                        }
                    }
                }
            },
            Tags = ResourceTagHelper.GetStandardTags(settings.Environment, GreenBlueConstants.MainContainerAppTag)
        }));
    }
}



