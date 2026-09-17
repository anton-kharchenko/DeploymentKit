namespace DeploymentKit.Components;

internal static class ComponentResourceScope
{
    private static readonly AsyncLocal<ComponentResource?> CurrentComponent = new();

    public static IDisposable Use(ComponentResource component)
    {
        ArgumentNullException.ThrowIfNull(component);

        ComponentResource? previous = CurrentComponent.Value;
        CurrentComponent.Value = component;
        return new Scope(previous);
    }

    public static CustomResourceOptions? CreateChildOptions(
        string resourceName,
        Action<CustomResourceOptions>? configure = null)
    {
        ComponentResource? parent = CurrentComponent.Value;
        if (parent == null && configure == null)
        {
            return null;
        }

        var options = new CustomResourceOptions();
        if (parent != null)
        {
            options.Parent = parent;
            options.Aliases =
            [
                new Alias
                {
                    Name = resourceName,
                    NoParent = true
                }
            ];
        }

        configure?.Invoke(options);
        return options;
    }

    private sealed class Scope(ComponentResource? previous) : IDisposable
    {
        public void Dispose() => CurrentComponent.Value = previous;
    }
}
