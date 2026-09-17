namespace DeploymentKit.Interfaces;

/// <summary>
/// Service for detecting and recovering from Pulumi state drift with Azure resources
/// </summary>
public interface IStateDriftRecoveryService
{
    /// <summary>
    /// Detects if an exception indicates state drift with Azure resources
    /// </summary>
    /// <param name="exception">The exception to analyze</param>
    /// <returns>True if state drift is detected, false otherwise</returns>
    bool IsStateDriftError(Exception exception);

    /// <summary>
    /// Attempts to recover from state drift by refreshing Pulumi state
    /// </summary>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if recovery was successful, false otherwise</returns>
    Task<bool> AttemptStateRecoveryAsync(string? correlationId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes Pulumi state to sync with actual Azure resources
    /// </summary>
    /// <param name="workingDirectory">The working directory containing the Pulumi project</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exit code from the Pulumi refresh command</returns>
    Task<int> RefreshStateAsync(string? workingDirectory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if the environment is CI/CD based on environment variables
    /// </summary>
    /// <returns>True if running in CI/CD, false otherwise</returns>
    bool IsCI();

    /// <summary>
    /// Returns Pulumi process environment variables recommended for the current execution context.
    /// </summary>
    IDictionary<string, string?> GetPulumiEnvironmentVariables(bool forceRefresh = false);

    /// <summary>
    /// Configures Pulumi environment variables for optimal performance
    /// </summary>
    /// <param name="forceRefresh">Force state refresh regardless of environment</param>
    void ConfigurePulumiEnvironment(bool forceRefresh = false);

    /// <summary>
    /// Gets the Azure error code from an exception if present
    /// </summary>
    /// <param name="exception">The exception to analyze</param>
    /// <returns>The Azure error code if found, null otherwise</returns>
    string? GetAzureErrorCode(Exception exception);
}

