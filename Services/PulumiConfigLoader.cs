using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Exceptions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

public class PulumiConfigLoader(
    IEnumerable<IInfrastructureConfigurationSource> configurationSources,
    ILogger<PulumiConfigLoader> logger) : IPulumiConfigLoader
{
    private readonly IReadOnlyList<IInfrastructureConfigurationSource> _configurationSources =
        (configurationSources ?? throw new ArgumentNullException(nameof(configurationSources))).ToList();

    private readonly ILogger<PulumiConfigLoader> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<InfrastructureConfigurationSourceResult> LoadConfigurationAsync(
        DeploymentOrchestratorOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var secretKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in _configurationSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourceResult = await source.LoadAsync(options, cancellationToken);

            foreach (var (key, value) in sourceResult.Values)
            {
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (!values.ContainsKey(key))
                {
                    values[key] = value;
                }
            }

            foreach (var secretKey in sourceResult.SecretKeys)
            {
                secretKeys.Add(secretKey);
            }
        }

        EnsureRequiredKeys(options.RequiredConfigKeys, values);

        _logger.LogInformation("Loaded {Count} configuration key(s) from {SourceCount} source(s)", values.Count, _configurationSources.Count);

        return new InfrastructureConfigurationSourceResult
        {
            Values = values,
            SecretKeys = secretKeys
        };
    }

    public IDictionary<string, string?> CreateEnvironmentOverrides(
        InfrastructureConfigurationSourceResult configuration,
        IDictionary<string, string[]>? mappings)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var overrides = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (mappings == null || mappings.Count == 0)
        {
            return overrides;
        }

        foreach (var (configKey, environmentVariableNames) in mappings)
        {
            if (!configuration.Values.TryGetValue(configKey, out var value) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var targetVariable = environmentVariableNames.FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));
            if (string.IsNullOrWhiteSpace(targetVariable))
            {
                continue;
            }

            overrides[targetVariable] = value;
        }

        _logger.LogInformation("Prepared {Count} environment override(s) from loaded configuration", overrides.Count);
        return overrides;
    }

    public Dictionary<string, string> LoadAllSecretsForKeyVault(
        Dictionary<string, string[]>? keyVaultSecretMappings = null,
        string[]? customPrefixes = null)
    {
        var secrets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var prefixes = GetSecretPrefixes(customPrefixes);
        var autoDiscoveredCount = ScanEnvironmentVariablesByPrefix(secrets, prefixes);

        var explicitCount = 0;
        if (keyVaultSecretMappings != null)
        {
            explicitCount = LoadSecretsFromMappings(secrets, keyVaultSecretMappings);
        }

        _logger.LogInformation("Loaded {Count} secret(s) for Key Vault: {AutoCount} auto-discovered, {ExplicitCount} from mappings",
            secrets.Count, autoDiscoveredCount, explicitCount);

        return secrets;
    }

    private static void EnsureRequiredKeys(
        ISet<string>? requiredKeys,
        IReadOnlyDictionary<string, string> loadedValues)
    {
        if (requiredKeys == null || requiredKeys.Count == 0)
        {
            return;
        }

        var missingKeys = requiredKeys
            .Where(key => !loadedValues.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            .ToArray();

        if (missingKeys.Length == 0)
        {
            return;
        }

        throw new PulumiConfigurationException(
            $"Missing required configuration keys: {string.Join(", ", missingKeys)}",
            ConfigurationIssueType.MissingConfig,
            "Provide missing values through ESC-backed config or environment fallback mappings.");
    }

    private HashSet<string> GetSecretPrefixes(string[]? customPrefixes = null)
    {
        string[] prefixArray;

        if (customPrefixes is { Length: > 0 })
        {
            prefixArray = customPrefixes;
            _logger.LogInformation("Using {Count} secret prefix(es) from client code: {Prefixes}",
                prefixArray.Length, string.Join(", ", prefixArray));
        }
        else
        {
            var prefixesEnv = Environment.GetEnvironmentVariable(EnvironmentVariableNames.KeyVault.SecretPrefixes);
            if (!string.IsNullOrWhiteSpace(prefixesEnv))
            {
                prefixArray = prefixesEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                _logger.LogInformation("Using {Count} secret prefix(es) from {EnvVar}: {Prefixes}",
                    prefixArray.Length, EnvironmentVariableNames.KeyVault.SecretPrefixes, string.Join(", ", prefixArray));
            }
            else
            {
                prefixArray = [];
                _logger.LogDebug("No secret prefixes configured - auto-discovery disabled");
            }
        }

        return new HashSet<string>(prefixArray, StringComparer.OrdinalIgnoreCase);
    }

    private static int ScanEnvironmentVariablesByPrefix(Dictionary<string, string> secrets, HashSet<string> prefixes)
    {
        var count = 0;
        foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            if (entry.Key is not string key || entry.Value is not string value || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!prefixes.Any(prefix => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var keyVaultKey = key.ToLowerInvariant().Replace("_", "-");
            if (secrets.TryAdd(keyVaultKey, value))
            {
                count++;
            }
        }

        return count;
    }

    private static int LoadSecretsFromMappings(Dictionary<string, string> secrets, Dictionary<string, string[]> mappings)
    {
        var count = 0;
        foreach (var (keyVaultKey, envVarNames) in mappings)
        {
            var value = envVarNames
                .Select(Environment.GetEnvironmentVariable)
                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

            if (!string.IsNullOrWhiteSpace(value) && secrets.TryAdd(keyVaultKey, value))
            {
                count++;
            }
        }

        return count;
    }
}

