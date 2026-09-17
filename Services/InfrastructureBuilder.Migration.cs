using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Interfaces;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Partial class for InfrastructureBuilder containing migration-related resource configuration methods.
/// </summary>
public partial class InfrastructureBuilder
{
    private bool _configureMigrations;
    private MigrationSettings? _migrationSettings;

    /// <summary>
    /// Configures database migrations using EF Core.
    /// </summary>
    /// <param name="migrationAssembly">The assembly containing migrations.</param>
    /// <param name="dbContextTypeName">The DbContext type name.</param>
    /// <param name="autoRunOnDeployment">Whether to run migrations automatically on deployment.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder ConfigureDatabaseMigrations(string migrationAssembly, string dbContextTypeName, bool autoRunOnDeployment = false)
    {
        if (string.IsNullOrWhiteSpace(migrationAssembly))
        {
            throw new ArgumentException(ValidationConstants.MigrationAssemblyRequired, nameof(migrationAssembly));
        }

        if (string.IsNullOrWhiteSpace(dbContextTypeName))
        {
            throw new ArgumentException(ValidationConstants.DbContextTypeRequired, nameof(dbContextTypeName));
        }

        _configureMigrations = true;
        _migrationSettings = new MigrationSettings
        {
            Enabled = true,
            MigrationType = MigrationType.EfCore,
            MigrationAssembly = migrationAssembly,
            DbContextTypeName = dbContextTypeName,
            AutoRunOnDeployment = autoRunOnDeployment
        };

        _logger.LogInformation(
            "Database migrations configured with EF Core: Assembly={Assembly}, DbContext={DbContext}, AutoRun={AutoRun}",
            migrationAssembly, dbContextTypeName, autoRunOnDeployment);

        return this;
    }

    /// <summary>
    /// Configures database migrations with custom settings.
    /// </summary>
    /// <param name="migrationSettings">Custom migration settings.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder ConfigureDatabaseMigrations(MigrationSettings migrationSettings)
    {
        _configureMigrations = true;
        _migrationSettings = migrationSettings ?? throw new ArgumentNullException(nameof(migrationSettings));
        _logger.LogInformation("Database migrations configured with custom settings: Type={Type}", migrationSettings.MigrationTypeString);
        return this;
    }

    /// <summary>
    /// Configures SQL script-based migrations.
    /// </summary>
    /// <param name="sqlScriptPath">Path to the SQL script.</param>
    /// <param name="autoRunOnDeployment">Whether to run migrations automatically on deployment.</param>
    /// <returns>The builder instance.</returns>
    public IInfrastructureBuilder ConfigureSqlMigrations(string sqlScriptPath, bool autoRunOnDeployment = false)
    {
        if (string.IsNullOrWhiteSpace(sqlScriptPath))
        {
            throw new ArgumentException(ValidationConstants.SqlScriptPathRequired, nameof(sqlScriptPath));
        }

        _configureMigrations = true;
        _migrationSettings = new MigrationSettings
        {
            Enabled = true,
            MigrationType = MigrationType.SqlScript,
            SqlScriptPath = sqlScriptPath,
            AutoRunOnDeployment = autoRunOnDeployment
        };

        _logger.LogInformation(
            "SQL migrations configured: ScriptPath={ScriptPath}, AutoRun={AutoRun}",
            sqlScriptPath, autoRunOnDeployment);

        return this;
    }
}

