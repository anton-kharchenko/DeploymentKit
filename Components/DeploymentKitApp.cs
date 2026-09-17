using DeploymentKit.Models.Outputs;

namespace DeploymentKit.Components;

public sealed class DeploymentKitApp : ComponentResource
{
    private DeploymentKitApp(string name, ComponentResourceOptions? options = null)
        : base(ComponentResourceTypeNames.App, name, options)
    {
    }

    public static async Task<ContainerRegistryOutputs> CreateContainerRegistryAsync(
        string name,
        Func<Task<ContainerRegistryOutputs>> resourceFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(resourceFactory);

        var component = new DeploymentKitApp(name);
        using var scope = ComponentResourceScope.Use(component);

        ContainerRegistryOutputs outputs = await resourceFactory();
        component.RegisterOutputs(new Dictionary<string, object?>
        {
            [ComponentOutputKeys.AcrName] = outputs.Name,
            [ComponentOutputKeys.LoginServer] = outputs.LoginServer,
            [ComponentOutputKeys.Username] = outputs.Username,
            [ComponentOutputKeys.Password] = outputs.Password,
            [ComponentOutputKeys.ResourceId] = outputs.ResourceId
        });

        return outputs;
    }

    public static async Task<ContainerAppsOutputs> CreateContainerAppsAsync(
        string name,
        Func<Task<ContainerAppsOutputs>> resourceFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(resourceFactory);

        var component = new DeploymentKitApp(name);
        using var scope = ComponentResourceScope.Use(component);

        ContainerAppsOutputs outputs = await resourceFactory();
        component.RegisterOutputs(new Dictionary<string, object?>
        {
            [ComponentOutputKeys.EnvironmentName] = outputs.EnvironmentName,
            [ComponentOutputKeys.EnvironmentId] = outputs.EnvironmentId,
            [ComponentOutputKeys.ApiAppName] = outputs.ApiAppName,
            [ComponentOutputKeys.ApiAppUrl] = outputs.ApiAppUrl,
            [ComponentOutputKeys.JobsAppName] = outputs.JobsAppName,
            [ComponentOutputKeys.JobsInternalFqdn] = outputs.JobsInternalFqdn,
            [ComponentOutputKeys.BotAppName] = outputs.BotAppName,
            [ComponentOutputKeys.BotAppUrl] = outputs.BotAppUrl
        });

        return outputs;
    }
}
