using DeploymentKit.Interfaces;

namespace DeploymentKit.Services;

/// <summary>
/// Default implementation of ICredentialProvider that supports multiple credential sources
/// </summary>
public class CredentialProvider : ICredentialProvider
{
    private readonly Dictionary<string, string> _credentials = new();
    private readonly bool _useEnvironmentVariables;

    /// <summary>
    /// Creates a new CredentialProvider
    /// </summary>
    /// <param name="useEnvironmentVariables">Whether to fall back to environment variables when credentials are not found</param>
    public CredentialProvider(bool useEnvironmentVariables = true)
    {
        _useEnvironmentVariables = useEnvironmentVariables;
    }

    /// <summary>
    /// Creates a new CredentialProvider with initial credentials
    /// </summary>
    /// <param name="initialCredentials">Initial credentials to set</param>
    /// <param name="useEnvironmentVariables">Whether to fall back to environment variables when credentials are not found</param>
    public CredentialProvider(Dictionary<string, string> initialCredentials, bool useEnvironmentVariables = true)
    {
        _useEnvironmentVariables = useEnvironmentVariables;
        foreach (var (key, value) in initialCredentials)
        {
            _credentials[key] = value;
        }
    }

    public string? GetCredential(string key)
    {
        if (_credentials.TryGetValue(key, out var value))
        {
            return value;
        }

        if (_useEnvironmentVariables)
        {
            return Environment.GetEnvironmentVariable(key);
        }

        return null;
    }

    public string GetRequiredCredential(string key)
    {
        var value = GetCredential(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Required credential '{key}' is not set or is empty.");
        }
        return value;
    }

    public bool HasCredential(string key)
    {
        return !string.IsNullOrWhiteSpace(GetCredential(key));
    }

    public void SetCredential(string key, string value)
    {
        _credentials[key] = value;
    }
}
