namespace DeploymentKit.Interfaces;

/// <summary>
/// Interface for parsing .env files and extracting key-value pairs for Key Vault secrets
/// </summary>
public interface IEnvFileParser
{
    /// <summary>
    /// Parses a .env file asynchronously and returns a dictionary of key-value pairs
    /// </summary>
    /// <param name="filePath">Path to the .env file</param>
    /// <returns>Dictionary containing the parsed environment variables</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file format is invalid</exception>
    Task<Dictionary<string, string>> ParseAsync(string filePath);

    /// <summary>
    /// Parses a .env file and returns a dictionary of key-value pairs with exclude patterns
    /// </summary>
    /// <param name="filePath">Path to the .env file</param>
    /// <param name="excludePatterns">Patterns to exclude (e.g., "TEMP_*", "DEBUG_*")</param>
    /// <returns>Dictionary containing the parsed environment variables</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file format is invalid</exception>
    Task<Dictionary<string, string>> ParseAsync(string filePath, IEnumerable<string> excludePatterns);

    /// <summary>
    /// Parses a .env file synchronously
    /// </summary>
    /// <param name="filePath">Path to the .env file</param>
    /// <returns>Dictionary containing the parsed environment variables</returns>
    Dictionary<string, string> Parse(string filePath);

    /// <summary>
    /// Validates that all keys in the .env file are valid Key Vault secret names
    /// </summary>
    /// <param name="envVariables">Dictionary of environment variables</param>
    /// <returns>List of validation errors, empty if all keys are valid</returns>
    List<string> ValidateKeyVaultSecretNames(Dictionary<string, string> envVariables);

    /// <summary>
    /// Validates if a secret name is valid for Key Vault
    /// </summary>
    /// <param name="secretName">The secret name to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool IsValidKeyVaultSecretName(string secretName);

    /// <summary>
    /// Filters environment variables to include only those suitable for Key Vault secrets
    /// </summary>
    /// <param name="envVariables">Dictionary of environment variables</param>
    /// <param name="excludePatterns">Patterns to exclude (e.g., "TEMP_*", "DEBUG_*")</param>
    /// <returns>Filtered dictionary of environment variables</returns>
    Dictionary<string, string> FilterForKeyVault(Dictionary<string, string> envVariables, IEnumerable<string>? excludePatterns = null);
}
