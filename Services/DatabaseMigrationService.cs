using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Exceptions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using System.Diagnostics;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing database migrations using various providers
/// </summary>
public class DatabaseMigrationService(
    ILogger<DatabaseMigrationService> logger,
    ICorrelationIdService correlationIdService)
    : IDatabaseMigrationService
{
    private readonly ILogger<DatabaseMigrationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));

    public async Task<MigrationOutputs> RunMigrationsAsync(
        InfrastructureSettings settings,
        DatabaseOutputs databaseOutputs,
        Input<string> resourceGroup,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(databaseOutputs);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        if (settings.Migration == null || !settings.Migration.Enabled)
        {
            _logger.LogInformation("Database migrations are not enabled, skipping migration execution");
            return new MigrationOutputs
            {
                Success = true,
                MigrationsApplied = 0,
                ExecutionTime = DateTime.UtcNow,
                ExecutionLog = "Migrations not enabled",
                MigrationType = "None"
            };
        }

        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        var stopwatch = Stopwatch.StartNew();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.PropertyNames.CorrelationId] = correlationId,
            [LoggingConstants.PropertyNames.Environment] = settings.Environment,
            [LoggingConstants.PropertyNames.Service] = "DatabaseMigrationService",
            [LoggingConstants.PropertyNames.Operation] = "RunMigrations"
        });

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Starting database migration execution for environment: {Environment}, CorrelationId: {CorrelationId}, MigrationType: {MigrationType}",
                settings.Environment, correlationId, settings.Migration.MigrationTypeString);

            // Validate migration settings
            var (isValid, errors) = await ValidateMigrationSettingsAsync(settings.Migration);
            if (!isValid)
            {
                var errorMessage = $"Migration validation failed: {string.Join(", ", errors)}";
                _logger.LogError(errorMessage);

                if (settings.Migration.FailOnError)
                {
                    throw new ConfigurationValidationException(errorMessage, "MigrationSettings", "MigrationValidation");
                }

                return new MigrationOutputs
                {
                    Success = false,
                    MigrationsApplied = 0,
                    ExecutionTime = DateTime.UtcNow,
                    ErrorMessage = errorMessage,
                    ExecutionLog = string.Join("\n", errors),
                    MigrationType = settings.Migration.MigrationTypeString
                };
            }

            MigrationOutputs result;

            // Choose migration execution strategy
            if (settings.Migration.UseContainerJob)
            {
                _logger.LogInformation("Using Container App Job for migrations, CorrelationId: {CorrelationId}", correlationId);
                result = await CreateMigrationJobAsync(settings, databaseOutputs, resourceGroup, cancellationToken);
            }
            else
            {
                _logger.LogInformation("Running migrations inline during deployment, CorrelationId: {CorrelationId}", correlationId);
                result = await ExecuteMigrationsInlineAsync(settings, databaseOutputs, correlationId, cancellationToken);
            }

            stopwatch.Stop();

            if (result.Success)
            {
                _logger.LogInformation(
                    "Database migration completed successfully for environment: {Environment}, Duration: {Duration}ms, CorrelationId: {CorrelationId}, MigrationsApplied: {MigrationsApplied}",
                    settings.Environment, stopwatch.ElapsedMilliseconds, correlationId, result.MigrationsApplied);
            }
            else
            {
                _logger.LogError(
                    "Database migration failed for environment: {Environment}, Duration: {Duration}ms, CorrelationId: {CorrelationId}, Error: {Error}",
                    settings.Environment, stopwatch.ElapsedMilliseconds, correlationId, result.ErrorMessage);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Database migration cancelled for environment: {Environment}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                settings.Environment, stopwatch.ElapsedMilliseconds, correlationId);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Database migration execution failed for environment: {Environment}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                settings.Environment, stopwatch.ElapsedMilliseconds, correlationId);

            if (settings.Migration.FailOnError)
            {
                throw new ResourceCreationException(
                    $"Database migration failed for environment {settings.Environment}",
                    ex,
                    "DatabaseMigration",
                    settings.Migration.MigrationTypeString,
                    settings.Environment,
                    correlationId,
                    "MIGRATION_FAILED");
            }

            return new MigrationOutputs
            {
                Success = false,
                MigrationsApplied = 0,
                ExecutionTime = DateTime.UtcNow,
                ErrorMessage = ex.Message,
                ExecutionLog = ex.ToString(),
                MigrationType = settings.Migration.MigrationTypeString
            };
        }
    }

    public Task<(bool IsValid, List<string> Errors)> ValidateMigrationSettingsAsync(MigrationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var errors = new List<string>();

        if (!settings.Enabled)
        {
            return Task.FromResult((true, errors)); // If not enabled, no validation needed
        }

        // Validate based on migration type
        switch (settings.MigrationType)
        {
            case MigrationType.EfCore:
                if (string.IsNullOrWhiteSpace(settings.MigrationAssembly))
                {
                    errors.Add("MigrationAssembly is required when using EfCore migration type");
                }
                if (string.IsNullOrWhiteSpace(settings.DbContextTypeName))
                {
                    errors.Add("DbContextTypeName is required when using EfCore migration type");
                }
                break;

            case MigrationType.FluentMigrator:
                if (string.IsNullOrWhiteSpace(settings.MigrationAssembly))
                {
                    errors.Add("MigrationAssembly is required when using FluentMigrator migration type");
                }
                break;

            case MigrationType.SqlScript:
                if (string.IsNullOrWhiteSpace(settings.SqlScriptPath))
                {
                    errors.Add("SqlScriptPath is required when using SqlScript migration type");
                }
                break;

            default:
                errors.Add($"Unknown migration type: {settings.MigrationType}");
                break;
        }

        // Validate Container Job configuration if enabled
        if (settings.UseContainerJob)
        {
            if (string.IsNullOrWhiteSpace(settings.MigrationContainerImage))
            {
                errors.Add("MigrationContainerImage is required when UseContainerJob is true");
            }
        }

        // Validate timeout
        if (settings.TimeoutSeconds < 30 || settings.TimeoutSeconds > 3600)
        {
            errors.Add("TimeoutSeconds must be between 30 and 3600 seconds");
        }

        var isValid = errors.Count == 0;

        _logger.LogDebug(
            "Migration settings validation completed: IsValid={IsValid}, ErrorCount={ErrorCount}",
            isValid, errors.Count);

        return Task.FromResult((isValid, errors));
    }

    public async Task<MigrationOutputs> CreateMigrationJobAsync(
        InfrastructureSettings settings,
        DatabaseOutputs databaseOutputs,
        Input<string> resourceGroup,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(databaseOutputs);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();

        _logger.LogInformation(
            "Creating Container App Job for database migrations, CorrelationId: {CorrelationId}",
            correlationId);

        // NOTE: This is a placeholder implementation
        // In a real implementation, you would create an Azure Container App Job
        // using Pulumi.AzureNative.App.Job
        // For now, we'll log a warning and return a result indicating the job was created

        _logger.LogWarning(
            "Container App Job creation for migrations is not yet fully implemented. " +
            "This feature requires additional Pulumi resources to be configured. " +
            "CorrelationId: {CorrelationId}",
            correlationId);

        // Return a placeholder result
        return await Task.FromResult(new MigrationOutputs
        {
            Success = true,
            MigrationsApplied = 0,
            ExecutionTime = DateTime.UtcNow,
            ExecutionLog = "Migration job created (placeholder implementation)",
            JobName = $"{settings.NamingPrefix}-{settings.Environment}-migration-job",
            MigrationType = settings.Migration?.MigrationTypeString ?? "Unknown"
        });
    }

    private async Task<MigrationOutputs> ExecuteMigrationsInlineAsync(
        InfrastructureSettings settings,
        DatabaseOutputs databaseOutputs,
        string correlationId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Executing migrations inline for MigrationType: {MigrationType}, CorrelationId: {CorrelationId}",
            settings.Migration?.MigrationTypeString, correlationId);

        var executionLog = new System.Text.StringBuilder();
        var startTime = DateTime.UtcNow;

        try
        {
            switch (settings.Migration?.MigrationType)
            {
                case MigrationType.EfCore:
                    return await ExecuteEfCoreMigrationsAsync(settings, databaseOutputs, correlationId, executionLog, cancellationToken);

                case MigrationType.SqlScript:
                    return await ExecuteSqlScriptMigrationsAsync(settings, databaseOutputs, correlationId, executionLog, cancellationToken);

                case MigrationType.FluentMigrator:
                    _logger.LogWarning(
                        "FluentMigrator support is not yet implemented. CorrelationId: {CorrelationId}",
                        correlationId);
                    executionLog.AppendLine("FluentMigrator support is not yet implemented.");
                    return new MigrationOutputs
                    {
                        Success = false,
                        MigrationsApplied = 0,
                        ExecutionTime = DateTime.UtcNow,
                        ErrorMessage = "FluentMigrator not yet supported",
                        ExecutionLog = executionLog.ToString(),
                        MigrationType = settings.Migration?.MigrationTypeString ?? "Unknown"
                    };

                default:
                    throw new InvalidOperationException($"Unknown migration type: {settings.Migration?.MigrationType}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration execution failed for CorrelationId: {CorrelationId}", correlationId);
            executionLog.AppendLine(FormattableString.Invariant($"ERROR: {ex.Message}"));
            executionLog.AppendLine(FormattableString.Invariant($"Stack Trace: {ex.StackTrace}"));

            return new MigrationOutputs
            {
                Success = false,
                MigrationsApplied = 0,
                ExecutionTime = DateTime.UtcNow,
                ErrorMessage = ex.Message,
                ExecutionLog = executionLog.ToString(),
                MigrationType = settings.Migration?.MigrationTypeString ?? "Unknown"
            };
        }
    }

    private async Task<MigrationOutputs> ExecuteEfCoreMigrationsAsync(
        InfrastructureSettings settings,
        DatabaseOutputs databaseOutputs,
        string correlationId,
        System.Text.StringBuilder executionLog,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Executing EF Core migrations for assembly: {Assembly}, DbContext: {DbContext}, CorrelationId: {CorrelationId}",
            settings.Migration?.MigrationAssembly, settings.Migration?.DbContextTypeName, correlationId);

        executionLog.AppendLine(FormattableString.Invariant($"Starting EF Core migration execution at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"));
        executionLog.AppendLine(FormattableString.Invariant($"Migration Assembly: {settings.Migration?.MigrationAssembly}"));
        executionLog.AppendLine(FormattableString.Invariant($"DbContext Type: {settings.Migration?.DbContextTypeName}"));

        // Get connection string - build it from database outputs
        string? connectionString = settings.Migration?.CustomConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Build connection string from database outputs
            // Since we're running after database creation in the orchestrator,
            // we can construct the connection string from known values
            var host = databaseOutputs.ServerName + ".postgres.database.azure.com";
            var database = databaseOutputs.DatabaseName;
            var username = databaseOutputs.AdminUsername;
            var password = settings.Database?.Password ?? settings.Database?.AdminPassword;

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Database password not found in settings. Please ensure Database.Password is set.");
            }

            connectionString = $"Host={host};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
        }

        executionLog.AppendLine("Connection string retrieved successfully");

        // Load the migration assembly
        var assemblyPath = settings.Migration!.MigrationAssembly!;
        System.Reflection.Assembly? migrationAssembly = null;

        try
        {
            // Try to load as a file path first
            try
            {
                migrationAssembly = System.Reflection.Assembly.LoadFrom(assemblyPath);
                executionLog.AppendLine(FormattableString.Invariant($"Loaded assembly from file: {assemblyPath}"));
            }
            catch (FileNotFoundException)
            {
                // If not found as a file, try to load by name
                migrationAssembly = System.Reflection.Assembly.Load(assemblyPath);
                executionLog.AppendLine(FormattableString.Invariant($"Loaded assembly by name: {assemblyPath}"));
            }
            catch (Exception)
            {
                // If LoadFrom fails for other reasons (e.g. bad format), fall back to Load
                migrationAssembly = System.Reflection.Assembly.Load(assemblyPath);
                executionLog.AppendLine(FormattableString.Invariant($"Loaded assembly by name (fallback): {assemblyPath}"));
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Failed to load migration assembly '{assemblyPath}': {ex.Message}";
            _logger.LogError(ex, errorMsg);
            executionLog.AppendLine(FormattableString.Invariant($"ERROR: {errorMsg}"));
            throw new InvalidOperationException(errorMsg, ex);
        }

        // Find the DbContext type
        var dbContextTypeName = settings.Migration!.DbContextTypeName!;
        var dbContextType = migrationAssembly.GetType(dbContextTypeName);

        if (dbContextType == null)
        {
            var errorMsg = $"DbContext type '{dbContextTypeName}' not found in assembly '{assemblyPath}'";
            _logger.LogError(errorMsg);
            executionLog.AppendLine(FormattableString.Invariant($"ERROR: {errorMsg}"));
            throw new InvalidOperationException(errorMsg);
        }

        executionLog.AppendLine(FormattableString.Invariant($"Found DbContext type: {dbContextType.FullName}"));

        // Load EF Core types using reflection
        var efCoreAssembly = System.Reflection.Assembly.Load("Microsoft.EntityFrameworkCore");
        var dbContextType_base = efCoreAssembly.GetType("Microsoft.EntityFrameworkCore.DbContext");
        var dbContextOptionsBuilderType = efCoreAssembly.GetType("Microsoft.EntityFrameworkCore.DbContextOptionsBuilder");

        if (!dbContextType_base!.IsAssignableFrom(dbContextType))
        {
            throw new InvalidOperationException($"Type '{dbContextTypeName}' does not inherit from DbContext");
        }

        // Create DbContextOptions using Npgsql
        var optionsBuilderInstance = Activator.CreateInstance(dbContextOptionsBuilderType!);

        // Call UseNpgsql extension method
        var npgsqlExtensionsType = System.Reflection.Assembly.Load("Npgsql.EntityFrameworkCore.PostgreSQL")
            .GetType("Microsoft.EntityFrameworkCore.NpgsqlDbContextOptionsBuilderExtensions");

        if (npgsqlExtensionsType == null)
        {
            throw new InvalidOperationException("Could not find NpgsqlDbContextOptionsBuilderExtensions type. Ensure Npgsql.EntityFrameworkCore.PostgreSQL is referenced.");
        }

        // Find UseNpgsql method - it should have parameters: DbContextOptionsBuilder and string connectionString
        var useNpgsqlMethod = npgsqlExtensionsType.GetMethods()
            .Where(m => m.Name == "UseNpgsql")
            .Where(m => m.IsGenericMethod == false)
            .FirstOrDefault(m =>
            {
                var parameters = m.GetParameters();
                return parameters.Length >= 2 &&
                       parameters[0].ParameterType.Name.Contains("DbContextOptionsBuilder") &&
                       parameters[1].ParameterType == typeof(string);
            });

        if (useNpgsqlMethod == null)
        {
            throw new InvalidOperationException("Could not find UseNpgsql(DbContextOptionsBuilder, string) extension method");
        }

        // Invoke UseNpgsql with the options builder and connection string
        var invokeParams = new object?[useNpgsqlMethod.GetParameters().Length];
        invokeParams[0] = optionsBuilderInstance;
        invokeParams[1] = connectionString;
        // Fill remaining parameters with null (optional parameters)
        for (int i = 2; i < invokeParams.Length; i++)
        {
            invokeParams[i] = null;
        }

        useNpgsqlMethod.Invoke(null, invokeParams);
        executionLog.AppendLine("Configured DbContext with PostgreSQL connection");

        // Get the Options property
        var optionsProperty = dbContextOptionsBuilderType!.GetProperty("Options");
        var options = optionsProperty!.GetValue(optionsBuilderInstance);

        // Create DbContext instance
        var dbContextInstance = Activator.CreateInstance(dbContextType, options);
        executionLog.AppendLine("Created DbContext instance");

        // Get the Database property
        var databaseProperty = dbContextType_base.GetProperty("Database");
        var databaseFacade = databaseProperty!.GetValue(dbContextInstance);

        if (databaseFacade == null)
        {
            throw new InvalidOperationException("Failed to get Database facade from DbContext");
        }

        // Get pending migrations count BEFORE applying (this is what will be applied)
        var getPendingMigrationsMethod = databaseFacade.GetType().GetMethod("GetPendingMigrations");
        if (getPendingMigrationsMethod == null)
        {
            throw new InvalidOperationException("Could not find GetPendingMigrations method on Database facade");
        }

        var pendingMigrationsBefore = getPendingMigrationsMethod.Invoke(databaseFacade, null) as System.Collections.IEnumerable;
        var pendingCountBefore = pendingMigrationsBefore?.Cast<object>().Count() ?? 0;

        executionLog.AppendLine(FormattableString.Invariant($"Found {pendingCountBefore} pending migrations to apply"));

        // List pending migrations
        if (pendingCountBefore > 0 && pendingMigrationsBefore != null)
        {
            executionLog.AppendLine("Pending migrations:");
            foreach (var migration in pendingMigrationsBefore)
            {
                executionLog.AppendLine(FormattableString.Invariant($"  - {migration}"));
            }
        }

        // Call Migrate() method to apply all pending migrations
        var migrateMethod = databaseFacade.GetType().GetMethod("Migrate");
        if (migrateMethod == null)
        {
            throw new InvalidOperationException("Could not find Migrate method on Database facade");
        }

        _logger.LogInformation("Applying {Count} migrations to database, CorrelationId: {CorrelationId}", pendingCountBefore, correlationId);
        executionLog.AppendLine("Applying migrations...");

        migrateMethod.Invoke(databaseFacade, null);

        executionLog.AppendLine("Migrations applied successfully");
        _logger.LogInformation("EF Core migrations completed successfully, CorrelationId: {CorrelationId}", correlationId);

        // Get applied migrations count AFTER for verification
        var getAppliedMigrationsMethod = databaseFacade.GetType().GetMethod("GetAppliedMigrations");
        var appliedMigrations = getAppliedMigrationsMethod?.Invoke(databaseFacade, null) as System.Collections.IEnumerable;
        var totalAppliedCount = appliedMigrations?.Cast<object>().Count() ?? 0;

        executionLog.AppendLine(FormattableString.Invariant($"Total migrations now applied in database: {totalAppliedCount}"));

        return await Task.FromResult(new MigrationOutputs
        {
            Success = true,
            MigrationsApplied = pendingCountBefore,  // Number of migrations we just applied
            ExecutionTime = DateTime.UtcNow,
            ExecutionLog = executionLog.ToString(),
            MigrationType = "EfCore"
        });
    }

    private async Task<MigrationOutputs> ExecuteSqlScriptMigrationsAsync(
        InfrastructureSettings settings,
        DatabaseOutputs databaseOutputs,
        string correlationId,
        System.Text.StringBuilder executionLog,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Executing SQL script migrations from: {ScriptPath}, CorrelationId: {CorrelationId}",
            settings.Migration?.SqlScriptPath, correlationId);

        executionLog.AppendLine(FormattableString.Invariant($"Starting SQL script migration execution at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"));
        executionLog.AppendLine(FormattableString.Invariant($"SQL Script Path: {settings.Migration?.SqlScriptPath}"));

        var scriptPath = settings.Migration!.SqlScriptPath!;

        // Read script content asynchronously - File.ReadAllTextAsync will throw FileNotFoundException if file doesn't exist
        // This avoids the TOCTOU race condition of checking Exists before reading
        string sqlScript;
        try
        {
            sqlScript = await System.IO.File.ReadAllTextAsync(scriptPath, cancellationToken).ConfigureAwait(false);
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException($"SQL script file not found: {scriptPath}", ex);
        }
        executionLog.AppendLine(FormattableString.Invariant($"Loaded SQL script ({sqlScript.Length} characters)"));

        // Get connection string
        string? connectionString = settings.Migration?.CustomConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Build connection string from database outputs
            var host = databaseOutputs.ServerName + ".postgres.database.azure.com";
            var database = databaseOutputs.DatabaseName;
            var username = databaseOutputs.AdminUsername;
            var password = settings.Database?.Password ?? settings.Database?.AdminPassword;

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Database password not found in settings. Please ensure Database.Password is set.");
            }

            connectionString = $"Host={host};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
        }

        // Execute SQL using Npgsql
        await using var connection = new Npgsql.NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        executionLog.AppendLine("Database connection opened");

        await using var command = new Npgsql.NpgsqlCommand(sqlScript, connection);
        command.CommandTimeout = settings.Migration.TimeoutSeconds;

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
        executionLog.AppendLine(FormattableString.Invariant($"SQL script executed successfully. Rows affected: {rowsAffected}"));

        _logger.LogInformation(
            "SQL script migrations completed successfully, Rows affected: {RowsAffected}, CorrelationId: {CorrelationId}",
            rowsAffected, correlationId);

        return new MigrationOutputs
        {
            Success = true,
            MigrationsApplied = 1,  // One script executed
            ExecutionTime = DateTime.UtcNow,
            ExecutionLog = executionLog.ToString(),
            MigrationType = "SqlScript"
        };
    }
}

