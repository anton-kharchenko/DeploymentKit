namespace DeploymentKit.Interfaces;

/// <summary>
/// Interface for providing credentials and configuration values for DeploymentKit infrastructure deployment
/// </summary>
public interface ICredentialProvider
{
    /// <summary>
    /// Gets a credential value by key
    /// </summary>
    /// <param name="key">The credential key</param>
    /// <returns>The credential value, or null if not found</returns>
    string? GetCredential(string key);

    /// <summary>
    /// Gets a required credential value by key
    /// </summary>
    /// <param name="key">The credential key</param>
    /// <returns>The credential value</returns>
    /// <exception cref="InvalidOperationException">Thrown when the credential is not found</exception>
    string GetRequiredCredential(string key);

    /// <summary>
    /// Checks if a credential exists
    /// </summary>
    /// <param name="key">The credential key</param>
    /// <returns>True if the credential exists, false otherwise</returns>
    bool HasCredential(string key);

    /// <summary>
    /// Sets a credential value
    /// </summary>
    /// <param name="key">The credential key</param>
    /// <param name="value">The credential value</param>
    void SetCredential(string key, string value);
}
