using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service interface for managing database migrations
/// Handles schema migrations using various providers (EF Core, FluentMigrator, SQL scripts)
/// </summary>
public interface IDatabaseMigrationService
{
    /// <summary>
    /// Executes database migrations based on the provided settings
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings including migration configuration</param>
    /// <param name="databaseOutputs">Database outputs containing connection information</param>
    /// <param name="resourceGroup">Azure resource group name where migration resources will be deployed</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Migration execution outputs containing status, logs, and applied migrations count</returns>
    Task<MigrationOutputs> RunMigrationsAsync(
        InfrastructureSettings settings,
        DatabaseOutputs databaseOutputs,
        Input<string> resourceGroup,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates migration configuration before execution
    /// </summary>
    /// <param name="settings">Migration settings to validate</param>
    /// <returns>Validation result with any error messages</returns>
    Task<(bool IsValid, List<string> Errors)> ValidateMigrationSettingsAsync(MigrationSettings settings);

    /// <summary>
    /// Creates a Container App Job for running migrations
    /// </summary>
    /// <param name="settings">Infrastructure configuration settings</param>
    /// <param name="databaseOutputs">Database outputs containing connection information</param>
    /// <param name="resourceGroup">Azure resource group name</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Container App Job outputs</returns>
    Task<MigrationOutputs> CreateMigrationJobAsync(
        InfrastructureSettings settings,
        DatabaseOutputs databaseOutputs,
        Input<string> resourceGroup,
        CancellationToken cancellationToken = default);
}

