using DeploymentKit.Models.Outputs;

namespace DeploymentKit.Components;

public sealed class DeploymentKitNetwork : ComponentResource
{
    private DeploymentKitNetwork(string name, ComponentResourceOptions? options = null)
        : base(ComponentResourceTypeNames.Network, name, options)
    {
    }

    public static async Task<NetworkOutputs> CreateAsync(string name, Func<Task<NetworkOutputs>> resourceFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(resourceFactory);

        var component = new DeploymentKitNetwork(name);
        using var scope = ComponentResourceScope.Use(component);

        NetworkOutputs outputs = await resourceFactory();
        component.RegisterOutputs(new Dictionary<string, object?>
        {
            [ComponentOutputKeys.VirtualNetworkId] = outputs.VirtualNetworkId,
            [ComponentOutputKeys.VirtualNetworkName] = outputs.VirtualNetworkName,
            [ComponentOutputKeys.ContainerAppsSubnetId] = outputs.ContainerAppsSubnetId,
            [ComponentOutputKeys.DatabaseSubnetId] = outputs.DatabaseSubnetId,
            [ComponentOutputKeys.PrivateEndpointsSubnetId] = outputs.PrivateEndpointsSubnetId,
            [ComponentOutputKeys.ApplicationGatewaySubnetId] = outputs.ApplicationGatewaySubnetId,
            [ComponentOutputKeys.ContainerAppsNsgId] = outputs.ContainerAppsNsgId,
            [ComponentOutputKeys.DatabaseNsgId] = outputs.DatabaseNsgId,
            [ComponentOutputKeys.ContainerAppsPrivateDnsZoneId] = outputs.ContainerAppsPrivateDnsZoneId,
            [ComponentOutputKeys.DatabasePrivateDnsZoneId] = outputs.DatabasePrivateDnsZoneId
        });

        return outputs;
    }
}
