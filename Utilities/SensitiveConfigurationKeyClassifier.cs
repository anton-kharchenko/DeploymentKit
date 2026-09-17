namespace DeploymentKit.Utilities;

/// <summary>
/// Classifies Pulumi configuration keys that should be encrypted in stack state.
/// </summary>
public static class SensitiveConfigurationKeyClassifier
{
    private const string PasswordMarker = "password";
    private const string SecretMarker = "secret";
    private const string TokenMarker = "token";
    private const string ApiKeyMarker = "apikey";
    private const string AccessKeyMarker = "accesskey";
    private const string ConnectionStringMarker = "connectionstring";
    private const string ClientSecretMarker = "clientsecret";
    private const string PrivateKeyMarker = "privatekey";
    private const string ConfigurationPathSeparator = ":";
    private const string EnvironmentVariableSeparator = "_";
    private const string ObjectPathSeparator = ".";
    private const string WordSeparator = "-";

    private static readonly string[] SensitiveMarkers =
    [
        PasswordMarker,
        SecretMarker,
        TokenMarker,
        ApiKeyMarker,
        AccessKeyMarker,
        ConnectionStringMarker,
        ClientSecretMarker,
        PrivateKeyMarker
    ];

    private static readonly string[] SeparatorsToRemove =
    [
        ConfigurationPathSeparator,
        EnvironmentVariableSeparator,
        ObjectPathSeparator,
        WordSeparator
    ];

    public static bool IsSensitive(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        string normalized = NormalizeKey(key);
        return SensitiveMarkers.Any(marker => normalized.Contains(marker, StringComparison.Ordinal));
    }

    private static string NormalizeKey(string key) => 
        SeparatorsToRemove.Aggregate(key, (current, separator) => current.Replace(separator, string.Empty, StringComparison.Ordinal)).ToLowerInvariant();
}
