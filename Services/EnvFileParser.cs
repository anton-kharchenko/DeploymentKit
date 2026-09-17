using DeploymentKit.Interfaces;
using System.Text.RegularExpressions;

namespace DeploymentKit.Services;

/// <summary>
/// Service for parsing .env files and extracting key-value pairs for Key Vault secrets
/// Supports standard .env file format with comments and empty lines
/// </summary>
public class EnvFileParser(ILogger<EnvFileParser> logger) : IEnvFileParser
{
    private readonly ILogger<EnvFileParser> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private static readonly Regex _envLineRegex = new(@"^(?<key>[A-Za-z_][A-Za-z0-9_.-]*)\s*=\s*(?<value>.*)$", RegexOptions.Compiled);
    private static readonly Regex _commentRegex = new(@"^\s*#.*$", RegexOptions.Compiled);
    private static readonly Regex _emptyLineRegex = new(@"^\s*$", RegexOptions.Compiled);

    private static readonly HashSet<string> SensitiveKeyPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "SECRET", "PASSWORD", "KEY", "TOKEN", "AUTH", "CREDENTIAL"
    };

    /// <summary>
    /// Parses a .env file and returns a dictionary of key-value pairs
    /// </summary>
    /// <param name="filePath">Path to the .env file</param>
    /// <returns>Dictionary containing the parsed environment variables</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file format is invalid</exception>
    public async Task<Dictionary<string, string>> ParseAsync(string filePath)
    {
        return await ParseAsync(filePath, []);
    }

    /// <summary>
    /// Parses a .env file and returns a dictionary of key-value pairs with exclude patterns
    /// </summary>
    /// <param name="filePath">Path to the .env file</param>
    /// <param name="excludePatterns">Patterns to exclude (e.g., "TEMP_*", "DEBUG_*")</param>
    /// <returns>Dictionary containing the parsed environment variables</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file format is invalid</exception>
    public async Task<Dictionary<string, string>> ParseAsync(string filePath, IEnumerable<string> excludePatterns)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        _logger.LogInformation("Parsing .env file: {FilePath}", filePath);

        string[] lines;
        try
        {
            lines = await File.ReadAllLinesAsync(filePath);
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException($"Environment file not found: {filePath}", ex);
        }

        var result = ProcessLines(lines, filePath, excludePatterns);

        _logger.LogInformation("Successfully parsed {Count} environment variables from {FilePath}", result.Count, filePath);
        return result;
    }

    /// <summary>
    /// Parses a .env file synchronously
    /// </summary>
    /// <param name="filePath">Path to the .env file</param>
    /// <returns>Dictionary containing the parsed environment variables</returns>
    public Dictionary<string, string> Parse(string filePath)
    {
        return ParseSync(filePath, []);
    }

    /// <summary>
    /// Parses a .env file synchronously with exclude patterns
    /// </summary>
    /// <param name="filePath">Path to the .env file</param>
    /// <param name="excludePatterns">Patterns to exclude</param>
    /// <returns>Dictionary containing the parsed environment variables</returns>
    private Dictionary<string, string> ParseSync(string filePath, IEnumerable<string> excludePatterns)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        _logger.LogInformation("Parsing .env file: {FilePath}", filePath);

        try
        {
            var lines = File.ReadAllLines(filePath);
            return ProcessLines(lines, filePath, excludePatterns);
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException($"Environment file not found: {filePath}", ex);
        }
    }

    private Dictionary<string, string> ProcessLines(string[] lines, string filePath, IEnumerable<string> excludePatterns)
    {
        var result = new Dictionary<string, string>();
        var lineNumber = 0;
        var patterns = excludePatterns?.ToList() ?? new List<string>();

        foreach (var line in lines)
        {
            lineNumber++;

            try
            {
                var parsedEntry = ParseLine(line, lineNumber);
                if (parsedEntry.HasValue)
                {
                    var (key, value) = parsedEntry.Value;

                    // Skip if key matches any exclude pattern
                    if (patterns.Any(pattern => IsPatternMatch(key, pattern)))
                    {
                        _logger.LogDebug("Excluding key '{Key}' due to exclude pattern", key);
                        continue;
                    }

                    // Skip empty values
                    if (string.IsNullOrEmpty(value))
                    {
                        _logger.LogDebug("Skipping key '{Key}' with empty value", key);
                        continue;
                    }

                    if (result.ContainsKey(key))
                    {
                        var logKey = IsSensitiveKey(key) ? "[REDACTED]" : key;
                        _logger.LogWarning("Duplicate key '{Key}' found at line {LineNumber}. Overwriting previous value.", logKey, lineNumber);
                    }

                    result[key] = value;
                    var logKeyParsed = IsSensitiveKey(key) ? "[REDACTED]" : key;
                    _logger.LogDebug("Parsed environment variable: {Key} (line {LineNumber})", logKeyParsed, lineNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing line {LineNumber} in file {FilePath}", lineNumber, filePath);
                throw new InvalidOperationException($"Invalid .env file format at line {lineNumber}", ex);
            }
        }

        return result;
    }

    /// <summary>
    /// Validates that all keys in the .env file are valid Key Vault secret names after transformation
    /// </summary>
    /// <param name="envVariables">Dictionary of environment variables</param>
    /// <returns>List of validation errors, empty if all keys are valid after transformation</returns>
    public List<string> ValidateKeyVaultSecretNames(Dictionary<string, string> envVariables)
    {
        var errors = new List<string>();

        foreach (var kvp in envVariables)
        {
            var transformedKey = TransformToKeyVaultSecretName(kvp.Key);
            var validationError = ValidateKeyVaultSecretName(transformedKey);
            if (!string.IsNullOrEmpty(validationError))
            {
                errors.Add($"Key '{kvp.Key}' (transformed to '{transformedKey}'): {validationError}");
            }
        }

        return errors;
    }

    /// <summary>
    /// Filters environment variables to include only those suitable for Key Vault secrets
    /// </summary>
    /// <param name="envVariables">Dictionary of environment variables</param>
    /// <param name="excludePatterns">Patterns to exclude (e.g., "TEMP_*", "DEBUG_*")</param>
    /// <returns>Filtered dictionary of environment variables with transformed keys</returns>
    public Dictionary<string, string> FilterForKeyVault(Dictionary<string, string> envVariables, IEnumerable<string>? excludePatterns = null)
    {
        ArgumentNullException.ThrowIfNull(envVariables, "variables");

        var filtered = new Dictionary<string, string>();
        var patterns = excludePatterns?.ToList() ?? new List<string>();

        foreach (var kvp in envVariables)
        {
            // Skip if key matches any exclude pattern
            if (patterns.Any(pattern => IsPatternMatch(kvp.Key, pattern)))
            {
                var logKey = IsSensitiveKey(kvp.Key) ? "[REDACTED]" : kvp.Key;
                _logger.LogDebug("Excluding key '{Key}' due to exclude pattern", logKey);
                continue;
            }

            // Transform key to be Key Vault compatible
            var transformedKey = TransformToKeyVaultSecretName(kvp.Key);

            // Skip if transformed key is still not valid for Key Vault
            if (!string.IsNullOrEmpty(ValidateKeyVaultSecretName(transformedKey)))
            {
                var logKey = IsSensitiveKey(kvp.Key) ? "[REDACTED]" : kvp.Key;
                var logTransformedKey = IsSensitiveKey(transformedKey) ? "[REDACTED]" : transformedKey;
                _logger.LogDebug("Excluding key '{Key}' (transformed to '{TransformedKey}') due to Key Vault naming restrictions", logKey, logTransformedKey);
                continue;
            }

            // Skip empty values
            if (string.IsNullOrWhiteSpace(kvp.Value))
            {
                var logKey = IsSensitiveKey(kvp.Key) ? "[REDACTED]" : kvp.Key;
                _logger.LogDebug("Excluding key '{Key}' due to empty value", logKey);
                continue;
            }

            if (transformedKey != kvp.Key)
            {
                var logKey = IsSensitiveKey(kvp.Key) ? "[REDACTED]" : kvp.Key;
                var logTransformedKey = IsSensitiveKey(transformedKey) ? "[REDACTED]" : transformedKey;
                _logger.LogDebug("Transformed key '{OriginalKey}' to '{TransformedKey}' for Key Vault compatibility", logKey, logTransformedKey);
            }

            filtered[transformedKey] = kvp.Value;
        }

        _logger.LogInformation("Filtered {OriginalCount} variables to {FilteredCount} suitable for Key Vault",
            envVariables.Count, filtered.Count);

        return filtered;
    }

    /// <summary>
    /// Validates if a secret name is valid for Key Vault
    /// </summary>
    /// <param name="secretName">The secret name to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValidKeyVaultSecretName(string secretName)
    {
        return string.IsNullOrEmpty(ValidateKeyVaultSecretName(secretName));
    }

    #region Private Helper Methods

    private static (string Key, string Value)? ParseLine(string line, int lineNumber)
    {
        // Skip empty lines
        if (_emptyLineRegex.IsMatch(line))
        {
            return null;
        }

        // Skip comment lines
        if (_commentRegex.IsMatch(line))
        {
            return null;
        }

        // Parse key-value pairs
        var match = _envLineRegex.Match(line);
        if (!match.Success)
        {
            throw new FormatException($"Invalid .env format at line {lineNumber}");
        }

        var key = match.Groups["key"].Value.Trim();
        var value = match.Groups["value"].Value.Trim();

        // Handle quoted values
        value = UnquoteValue(value);

        return (key, value);
    }

    private static string UnquoteValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        // Handle single quotes
        if (value.StartsWith("'") && value.EndsWith("'") && value.Length >= 2)
        {
            return value[1..^1];
        }

        // Handle double quotes with escape sequences
        if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
        {
            var unquoted = value[1..^1];
            // Handle common escape sequences
            unquoted = unquoted.Replace("\\n", "\n")
                              .Replace("\\r", "\r")
                              .Replace("\\t", "\t")
                              .Replace("\\\"", "\"")
                              .Replace("\\\\", "\\");
            return unquoted;
        }

        return value;
    }

    private static string? ValidateKeyVaultSecretName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Secret name cannot be empty";
        }

        if (name.Length > 127)
        {
            return "Secret name cannot exceed 127 characters";
        }

        if (!Regex.IsMatch(name, @"^[a-zA-Z0-9-]+$"))
        {
            return "Secret name can only contain alphanumeric characters and hyphens";
        }

        if (name.StartsWith("-") || name.EndsWith("-"))
        {
            return "Secret name cannot start or end with a hyphen";
        }

        return null;
    }

    private static bool IsPatternMatch(string input, string pattern)
    {
        // Simple wildcard pattern matching
        if (pattern.Contains('*'))
        {
            var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(input, regexPattern, RegexOptions.IgnoreCase);
        }

        return string.Equals(input, pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSensitiveKey(string key)
    {
        return SensitiveKeyPatterns.Any(p => key.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Transforms an environment variable name to be compatible with Key Vault secret naming requirements
    /// </summary>
    /// <param name="name">The original environment variable name</param>
    /// <returns>Transformed name suitable for Key Vault</returns>
    private static string TransformToKeyVaultSecretName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        // Convert underscores to hyphens for Key Vault compatibility
        var transformed = name.Replace('_', '-');

        // Remove any characters that are not alphanumeric or hyphens
        transformed = Regex.Replace(transformed, @"[^a-zA-Z0-9-]", "");

        // Remove leading/trailing hyphens
        transformed = transformed.Trim('-');

        // Ensure it's not empty after transformation
        if (string.IsNullOrEmpty(transformed))
        {
            return name; // Return original if transformation results in empty string
        }

        return transformed;
    }

    #endregion
}

