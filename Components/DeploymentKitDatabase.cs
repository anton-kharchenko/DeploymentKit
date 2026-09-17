using DeploymentKit.Models.Outputs;

namespace DeploymentKit.Components;

public sealed class DeploymentKitDatabase : ComponentResource
{
    private DeploymentKitDatabase(string name, ComponentResourceOptions? options = null)
        : base(ComponentResourceTypeNames.Database, name, options)
    {
    }

    public static async Task<DatabaseOutputs> CreateAsync(string name, Func<Task<DatabaseOutputs>> resourceFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(resourceFactory);

        var component = new DeploymentKitDatabase(name);
        using var scope = ComponentResourceScope.Use(component);

        DatabaseOutputs outputs = await resourceFactory();
        component.RegisterOutputs(new Dictionary<string, object?>
        {
            [ComponentOutputKeys.ServerName] = outputs.ServerName,
            [ComponentOutputKeys.DatabaseName] = outputs.DatabaseName,
            [ComponentOutputKeys.AdminUsername] = outputs.AdminUsername,
            [ComponentOutputKeys.HostName] = outputs.HostName,
            [ComponentOutputKeys.FullyQualifiedDomainName] = outputs.FullyQualifiedDomainName,
            [ComponentOutputKeys.ConnectionString] = outputs.ConnectionString,
            [ComponentOutputKeys.Password] = outputs.Password
        });

        return outputs;
    }
}
