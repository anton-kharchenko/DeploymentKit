using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

public interface IFrontDoorDeployer
{
    Task<FrontDoorOutputs?> CreateFoundationAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        CancellationToken cancellationToken = default);

    Task<FrontDoorOutputs?> ConfigureRoutingAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        FrontDoorOutputs foundation,
        StorageOutputs storage,
        ContainerAppsOutputs containerApps,
        CancellationToken cancellationToken = default);
}


