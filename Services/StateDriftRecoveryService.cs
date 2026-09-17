using DeploymentKit.Interfaces;
using System.Diagnostics;
using System.Text;

namespace DeploymentKit.Services;

/// <summary>
/// Service for detecting and recovering from Pulumi state drift with Azure resources
/// </summary>
public class StateDriftRecoveryService : IStateDriftRecoveryService
{
    private readonly ILogger<StateDriftRecoveryService> _logger;
    private readonly ICorrelationIdService _correlationIdService;

    /// <summary>
    /// Azure error codes that indicate state drift
    /// </summary>
    private static readonly string[] StateDriftErrorCodes =
    [
        "ResourceGroupNotFound",
        "ResourceNotFound",
        "SubscriptionNotFound",
        "ParentResourceNotFound",
        "AuthorizationFailed",
        "ResourceGroupBeingDeleted",
        "ResourceGroupNotProvisioned",
        "ResourceGroupDeletionBlocked",
        "InvalidResourceGroup",
        "MissingSubscriptionRegistration"
    ];

    public StateDriftRecoveryService(
        ILogger<StateDriftRecoveryService> logger,
        ICorrelationIdService correlationIdService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
    }

    /// <inheritdoc />
    public bool IsStateDriftError(Exception exception)
    {
        if (exception == null)
            return false;

        var message = exception.ToString();
        var errorCode = GetAzureErrorCode(exception);

        // Check for specific Azure error codes
        if (!string.IsNullOrEmpty(errorCode) && StateDriftErrorCodes.Contains(errorCode))
        {
            _logger.LogWarning("Detected state drift error with Azure error code: {ErrorCode}", errorCode);
            return true;
        }

        // Check for error codes in the message
        foreach (var code in StateDriftErrorCodes)
        {
            if (message.Contains($"Code=\"{code}\"") ||
                message.Contains($"\"code\":\"{code}\"") ||
                message.Contains($"ErrorCode: {code}"))
            {
                _logger.LogWarning("Detected state drift error in exception message with code: {ErrorCode}", code);
                return true;
            }
        }

        // Check for deletion failures which indicate state drift
        if (message.Contains("deleting failed") && message.Contains("Code="))
        {
            _logger.LogWarning("Detected state drift error due to deletion failure");
            return true;
        }

        // Check for Pulumi-specific state drift indicators
        if (message.Contains("refresh to update the stack") ||
            message.Contains("resource has been deleted") ||
            message.Contains("no longer exists in Azure") ||
            message.Contains("state is out of sync"))
        {
            _logger.LogWarning("Detected Pulumi state drift indicator in exception message");
            return true;
        }

        // Recursively check inner exceptions
        if (exception.InnerException != null)
        {
            return IsStateDriftError(exception.InnerException);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> AttemptStateRecoveryAsync(string? correlationId = null, CancellationToken cancellationToken = default)
    {
        correlationId ??= _correlationIdService.GetOrGenerateCorrelationId();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Operation"] = "StateDriftRecovery"
        });

        try
        {
            _logger.LogWarning("🔄 Checking Pulumi state consistency for CorrelationId: {CorrelationId}...", correlationId);
            _logger.LogInformation("🔧 Attempting automatic recovery for CorrelationId: {CorrelationId}...", correlationId);

            LogRecoverySteps();

            // Skip if explicitly disabled
            if (Environment.GetEnvironmentVariable("SKIP_PULUMI_REFRESH") == "true")
            {
                _logger.LogWarning("⚠️  State refresh skipped due to SKIP_PULUMI_REFRESH=true for CorrelationId: {CorrelationId}", correlationId);
                return false;
            }

            var exitCode = await RefreshStateAsync(cancellationToken: cancellationToken);

            if (exitCode == 0)
            {
                _logger.LogInformation("✅ State recovery successful for CorrelationId: {CorrelationId}, continuing deployment", correlationId);
                return true;
            }
            else
            {
                _logger.LogWarning("⚠️  State refresh completed with warnings (exit code: {ExitCode}) for CorrelationId: {CorrelationId}, attempting deployment anyway",
                    exitCode, correlationId);
                return true; // Still return true to attempt deployment
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ State recovery failed for CorrelationId: {CorrelationId}", correlationId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> RefreshStateAsync(string? workingDirectory = null, CancellationToken cancellationToken = default)
    {
        workingDirectory ??= Directory.GetCurrentDirectory();

        _logger.LogInformation("🔄 Refreshing Pulumi state with Azure for directory: {WorkingDirectory}...", workingDirectory);

        var executable = GetPulumiExecutable();

        var processInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = "refresh --yes --suppress-outputs",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        // Add environment variables for better performance
        processInfo.Environment["PULUMI_SKIP_UPDATE_CHECK"] = "true";
        processInfo.Environment["PULUMI_CI"] = IsCI() ? "true" : "false";

        // Pass through Pulumi config passphrase if set
        var passphrase = Environment.GetEnvironmentVariable("PULUMI_CONFIG_PASSPHRASE");
        if (!string.IsNullOrEmpty(passphrase))
        {
            processInfo.Environment["PULUMI_CONFIG_PASSPHRASE"] = passphrase;
            _logger.LogDebug("Using PULUMI_CONFIG_PASSPHRASE from environment");
        }

        using var process = new Process { StartInfo = processInfo };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
                _logger.LogDebug("Pulumi refresh output: {Output}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorBuilder.AppendLine(e.Data);

                // Log warnings and errors differently
                if (e.Data.Contains("warning", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Pulumi refresh warning: {Warning}", e.Data);
                }
                else if (e.Data.Contains("error", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("Pulumi refresh error: {Error}", e.Data);
                }
                else
                {
                    _logger.LogDebug("Pulumi refresh stderr: {Output}", e.Data);
                }
            }
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            _logger.LogInformation("Pulumi refresh completed with exit code: {ExitCode}", process.ExitCode);

            if (process.ExitCode != 0)
            {
                var error = errorBuilder.ToString();

                // Check for specific error conditions
                if (error.Contains("invalid ciphertext") || error.Contains("unable to decrypt"))
                {
                    _logger.LogError("❌ Pulumi refresh failed due to encryption issue. " +
                        "Please ensure PULUMI_CONFIG_PASSPHRASE is set correctly or check your Pulumi login status. " +
                        "Error: {Error}", error);

                    _logger.LogWarning("💡 To fix: Set PULUMI_CONFIG_PASSPHRASE environment variable or run 'pulumi login'");
                }
                else
                {
                    _logger.LogWarning("Pulumi refresh exited with code {ExitCode}. Error output: {Error}",
                        process.ExitCode, error);
                }
            }

            return process.ExitCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Pulumi refresh command");
            throw;
        }
    }

    /// <inheritdoc />
    public bool IsCI()
    {
        var ciVars = new[]
        {
            "CI",
            "GITHUB_ACTIONS",
            "TF_BUILD", // Azure DevOps
            "JENKINS_URL",
            "GITLAB_CI",
            "CIRCLECI",
            "TRAVIS",
            "APPVEYOR",
            "CODEBUILD_BUILD_ID" // AWS CodeBuild
        };

        return ciVars.Any(varName =>
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(varName)));
    }

    /// <inheritdoc />
    public IDictionary<string, string?> GetPulumiEnvironmentVariables(bool forceRefresh = false)
    {
        var environmentVariables = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var isCI = IsCI();

        if (isCI || forceRefresh)
        {
            environmentVariables["PULUMI_PARALLEL"] = "32";
            environmentVariables["PULUMI_REFRESH"] = "true";
            environmentVariables["PULUMI_VERBOSE"] = "7";
            environmentVariables["PULUMI_CI"] = "true";
            environmentVariables["PULUMI_SKIP_UPDATE_CHECK"] = "true";
        }
        else
        {
            environmentVariables["PULUMI_PARALLEL"] = "16";
            if (forceRefresh)
            {
                environmentVariables["PULUMI_REFRESH"] = "true";
            }
        }

        environmentVariables["PULUMI_SKIP_CONFIRMATIONS"] = "true";
        environmentVariables["PULUMI_DISABLE_AUTOMATIC_PLUGIN_DOWNLOADS"] = "false";
        return environmentVariables;
    }

    /// <inheritdoc />
    public void ConfigurePulumiEnvironment(bool forceRefresh = false)
    {
        var isCI = IsCI();

        _logger.LogInformation("Configuring Pulumi environment (CI: {IsCI}, ForceRefresh: {ForceRefresh})",
            isCI, forceRefresh);

        foreach (var (key, value) in GetPulumiEnvironmentVariables(forceRefresh))
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        if (isCI || forceRefresh)
        {
            _logger.LogInformation("Configured Pulumi for CI/CD environment with parallel=32, verbose=7");
        }
        else
        {
            _logger.LogInformation("Configured Pulumi for local development with parallel=16");
        }
    }

    /// <inheritdoc />
    public string? GetAzureErrorCode(Exception exception)
    {
        if (exception == null)
            return null;

        var message = exception.Message;

        // Try to extract Azure error code from different formats
        // Format 1: Code="ResourceGroupNotFound"
        var match = System.Text.RegularExpressions.Regex.Match(message, @"Code=""([^""]+)""");
        if (match.Success)
            return match.Groups[1].Value;

        // Format 2: "code":"ResourceGroupNotFound"
        match = System.Text.RegularExpressions.Regex.Match(message, @"""code"":""([^""]+)""");
        if (match.Success)
            return match.Groups[1].Value;

        // Format 3: ErrorCode: ResourceGroupNotFound
        match = System.Text.RegularExpressions.Regex.Match(message, @"ErrorCode:\s*([^\s,]+)");
        if (match.Success)
            return match.Groups[1].Value;

        // Check inner exception
        if (exception.InnerException != null)
        {
            return GetAzureErrorCode(exception.InnerException);
        }

        return null;
    }

    private void LogRecoverySteps()
    {
        _logger.LogInformation("📋 State Recovery Steps:");
        _logger.LogInformation("  1. Refreshing Pulumi state with Azure...");
        _logger.LogInformation("  2. Removing orphaned resources from state...");
        _logger.LogInformation("  3. Retrying deployment...");
    }

    private static string GetPulumiExecutable()
    {
        // Check if pulumi is in PATH
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];

        var pulumiInPath = paths.Any(path =>
            File.Exists(Path.Combine(path, "pulumi.exe")) || File.Exists(Path.Combine(path, "pulumi")));

        if (pulumiInPath)
        {
            return "pulumi";
        }

        // Try common installation locations
        var commonPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".pulumi", "bin", "pulumi"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Pulumi", "bin", "pulumi.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Pulumi", "bin", "pulumi.exe"),
            "/usr/local/bin/pulumi",
            "/usr/bin/pulumi"
        };

        var executable = commonPaths.FirstOrDefault(File.Exists);
        if (!string.IsNullOrEmpty(executable))
        {
            return executable;
        }

        // Fall back to hoping it's in PATH
        return "pulumi";
    }
}

