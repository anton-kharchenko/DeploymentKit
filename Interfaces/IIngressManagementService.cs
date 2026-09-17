using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service for managing ingress.
/// </summary>
public interface IIngressManagementService
{
    /// <summary>
    /// Creates the main ingress container app.
    /// </summary>
    Task<ContainerApp> CreateMainIngressAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        ManagedEnvironment environment,
        SlotOutputs greenSlot,
        SlotOutputs blueSlot);
}



