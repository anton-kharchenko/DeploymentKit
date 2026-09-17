namespace DeploymentKit.Interfaces;

/// <summary>
/// Applies temporary process-level environment overrides and restores previous values on dispose.
/// </summary>
public interface ILegacyEnvironmentBridge
{
    IDisposable Apply(IDictionary<string, string?> values);
}

