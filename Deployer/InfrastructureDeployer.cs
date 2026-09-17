using DeploymentKit.Constants;
using DeploymentKit.Exceptions;
using DeploymentKit.Extensions;
using DeploymentKit.Infrastructure;
using DeploymentKit.Interfaces;
using DeploymentKit.Services;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;

namespace DeploymentKit.Deployer;

public static class InfrastructureDeployer
{
    public static IInfrastructureBuilder CreateBuilder(Action<ILoggingBuilder>? configureLogging = null)
    {
        ServiceProvider serviceProvider = BuildDefaultProvider(configureLogging);
        IResourceNameValidator nameValidator = serviceProvider.GetRequiredService<IResourceNameValidator>();
        IEnvFileParser envFileParser = serviceProvider.GetRequiredService<IEnvFileParser>();
        ILogger<InfrastructureBuilder> logger = serviceProvider.GetRequiredService<ILogger<InfrastructureBuilder>>();

        return new InfrastructureBuilder(nameValidator, envFileParser, logger);
    }

    public static async Task<Models.Outputs.InfrastructureDeploymentOutputs> DeployAsync(
        Action<IInfrastructureBuilder> builderAction,
        CancellationToken cancellationToken = default,
        Action<ILoggingBuilder>? configureLogging = null,
        Config? pulumiConfig = null,
        Dictionary<string, string[]>? secretMappings = null)
    {
        ArgumentNullException.ThrowIfNull(builderAction);

        return await WithProviderAsync(async serviceProvider =>
        {
            ILogger logger = CreateDeployerLogger(serviceProvider);
            IStateDriftRecoveryService stateDriftRecovery = serviceProvider.GetRequiredService<IStateDriftRecoveryService>();
            ICorrelationIdService correlationIdService = serviceProvider.GetRequiredService<ICorrelationIdService>();
            IAzureAuthenticationService azureAuth = serviceProvider.GetRequiredService<IAzureAuthenticationService>();
            IPulumiConfigLoader configLoader = serviceProvider.GetRequiredService<IPulumiConfigLoader>();
            ILegacyEnvironmentBridge legacyEnvironmentBridge = serviceProvider.GetRequiredService<ILegacyEnvironmentBridge>();

            IDictionary<string, string?> scopedEnvironment = await BuildScopedEnvironmentAsync(
                configLoader,
                stateDriftRecovery,
                azureAuth,
                pulumiConfig,
                secretMappings,
                cancellationToken);

            using IDisposable environmentScope = legacyEnvironmentBridge.Apply(scopedEnvironment);

            IInfrastructureBuilder builder = CreateBuilder(configureLogging);
            builderAction(builder);
            InfrastructureSettings settings = await builder.BuildAsync();

            ValidateConfigurationAndLog(settings, logger);

            string correlationId = correlationIdService.GenerateCorrelationId();
            correlationIdService.SetCorrelationId(correlationId);

            return await ExecuteWithStateDriftRecoveryAsync(
                serviceProvider,
                settings,
                correlationId,
                cancellationToken);
        }, configureLogging);
    }

    public static async Task<Models.Outputs.InfrastructureDeploymentOutputs> DeployAsync(
        InfrastructureSettings settings,
        CancellationToken cancellationToken = default,
        Action<ILoggingBuilder>? configureLogging = null)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return await WithProviderAsync(async serviceProvider =>
        {
            ILogger logger = CreateDeployerLogger(serviceProvider);
            IStateDriftRecoveryService stateDriftRecovery = serviceProvider.GetRequiredService<IStateDriftRecoveryService>();
            ICorrelationIdService correlationIdService = serviceProvider.GetRequiredService<ICorrelationIdService>();
            IAzureAuthenticationService azureAuth = serviceProvider.GetRequiredService<IAzureAuthenticationService>();
            ILegacyEnvironmentBridge legacyEnvironmentBridge = serviceProvider.GetRequiredService<ILegacyEnvironmentBridge>();

            logger.LogInformation(InfrastructureConstants.LogMessages.StartingDeployment);

            Dictionary<string, string?> scopedEnvironment = BuildBaseScopedEnvironment(stateDriftRecovery, azureAuth, cancellationToken);
            using IDisposable environmentScope = legacyEnvironmentBridge.Apply(scopedEnvironment);

            ValidateConfigurationAndLog(settings, logger);

            string correlationId = correlationIdService.GenerateCorrelationId();
            correlationIdService.SetCorrelationId(correlationId);

            Models.Outputs.InfrastructureDeploymentOutputs outputs = await ExecuteWithStateDriftRecoveryAsync(
                serviceProvider,
                settings,
                correlationId,
                cancellationToken);

            logger.LogInformation(InfrastructureConstants.LogMessages.DeploymentCompleted);
            return outputs;
        }, configureLogging);
    }

    private static ServiceProvider BuildDefaultProvider(Action<ILoggingBuilder>? configureLogging = null)
    {
        ServiceCollection services = new ServiceCollection();
        services.AddInfrastructureServices(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
            configureLogging?.Invoke(builder);
        });

        return services.BuildServiceProvider();
    }

    private static void EnsureServicesValid(IServiceProvider serviceProvider)
    {
        if (!serviceProvider.ValidateInfrastructureServices())
        {
            throw new InvalidOperationException(InfrastructureConstants.LogMessages.ServiceRegistrationFailed);
        }
    }

    private static ILogger CreateDeployerLogger(IServiceProvider serviceProvider) =>
        serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("InfrastructureDeployer");

    private static async Task<IDictionary<string, string?>> BuildScopedEnvironmentAsync(
        IPulumiConfigLoader configLoader,
        IStateDriftRecoveryService stateDriftRecovery,
        IAzureAuthenticationService azureAuth,
        Config? pulumiConfig,
        IDictionary<string, string[]>? secretMappings,
        CancellationToken cancellationToken)
    {
        Dictionary<string, string?> scopedEnvironment = BuildBaseScopedEnvironment(stateDriftRecovery, azureAuth, cancellationToken);

        if (secretMappings == null || secretMappings.Count == 0)
        {
            return scopedEnvironment;
        }

        InfrastructureDeploymentOrchestratorOptions options = new()
        {
            StackName = "legacy",
            ProjectName = "DeploymentKit-deployment",
            WorkingDirectory = Directory.GetCurrentDirectory(),
            PulumiConfig = pulumiConfig,
            EnvironmentFallbackMappings = new Dictionary<string, string[]>(secretMappings, StringComparer.OrdinalIgnoreCase),
            RequiredConfigKeys = new HashSet<string>(secretMappings.Keys, StringComparer.OrdinalIgnoreCase)
        };

        Models.InfrastructureConfigurationSourceResult loadedConfiguration = await configLoader.LoadConfigurationAsync(options, cancellationToken);
        foreach ((string key, string? value) in configLoader.CreateEnvironmentOverrides(loadedConfiguration, secretMappings))
        {
            scopedEnvironment[key] = value;
        }

        return scopedEnvironment;
    }

    private static Dictionary<string, string?> BuildBaseScopedEnvironment(
        IStateDriftRecoveryService stateDriftRecovery,
        IAzureAuthenticationService azureAuth,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Dictionary<string, string?> scopedEnvironment = new(StringComparer.OrdinalIgnoreCase);

        foreach ((string key, string? value) in stateDriftRecovery.GetPulumiEnvironmentVariables())
        {
            scopedEnvironment[key] = value;
        }

        foreach ((string key, string? value) in azureAuth.GetServicePrincipalEnvironmentVariables())
        {
            scopedEnvironment[key] = value;
        }

        return scopedEnvironment;
    }

    private static void ValidateConfigurationAndLog(InfrastructureSettings settings, ILogger logger)
    {
        if (!ConfigurationValidator.ValidateSettings(settings, logger))
        {
            throw new InvalidOperationException(InfrastructureConstants.LogMessages.ConfigurationValidationFailed);
        }

        if (!ConfigurationValidator.ValidateNamingConventions(settings, logger))
        {
            logger.LogWarning(InfrastructureConstants.LogMessages.NamingValidationWarning);
        }

        if (!ConfigurationValidator.ValidateAzureLocation(settings.Location, logger))
        {
            logger.LogWarning(InfrastructureConstants.LogMessages.LocationValidationWarning);
        }

        ConfigurationValidator.ValidateResourceLimits(settings, logger);
    }

    private static async Task<T> WithProviderAsync<T>(Func<ServiceProvider, Task<T>> action, Action<ILoggingBuilder>? configureLogging = null)
    {
        await using ServiceProvider serviceProvider = BuildDefaultProvider(configureLogging);
        EnsureServicesValid(serviceProvider);
        return await action(serviceProvider).ConfigureAwait(false);
    }

    private static async Task<Models.Outputs.InfrastructureDeploymentOutputs> ExecuteWithStateDriftRecoveryAsync(
        IServiceProvider serviceProvider,
        InfrastructureSettings settings,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        ILogger logger = CreateDeployerLogger(serviceProvider);
        IStateDriftRecoveryService stateDriftRecovery = serviceProvider.GetRequiredService<IStateDriftRecoveryService>();
        InfrastructureOrchestrator orchestrator = serviceProvider.GetRequiredService<InfrastructureOrchestrator>();

        using IDisposable? scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Environment"] = settings.Environment,
            ["Operation"] = "DeploymentWithRecovery"
        });

        try
        {
            if (stateDriftRecovery.IsCI() && Environment.GetEnvironmentVariable("SKIP_PULUMI_REFRESH") != "true")
            {
                logger.LogInformation("CI environment detected. Performing state refresh for {CorrelationId}.", correlationId);
                await stateDriftRecovery.RefreshStateAsync(cancellationToken: cancellationToken);
            }

            logger.LogInformation("Starting deployment for {CorrelationId}.", correlationId);
            return await orchestrator.DeployAsync(settings, cancellationToken);
        }
        catch (Exception ex) when (stateDriftRecovery.IsStateDriftError(ex))
        {
            string? azureErrorCode = stateDriftRecovery.GetAzureErrorCode(ex);

            logger.LogWarning(ex, "Detected state drift issue for {CorrelationId}. Azure error code: {ErrorCode}",
                correlationId, azureErrorCode ?? "Unknown");

            bool recoverySuccessful = await stateDriftRecovery.AttemptStateRecoveryAsync(correlationId, cancellationToken);

            if (!recoverySuccessful)
            {
                throw new StateDriftException(
                    "State drift recovery failed. Manual intervention may be required. Try running 'pulumi refresh --yes' manually or inspect Azure resource state.",
                    ex,
                    azureErrorCode,
                    correlationId,
                    recoveryAttempted: true,
                    recoverySuccessful: false);
            }

            try
            {
                logger.LogInformation("Retrying deployment after state recovery for {CorrelationId}.", correlationId);
                return await orchestrator.DeployAsync(settings, cancellationToken);
            }
            catch (Exception retryEx)
            {
                logger.LogError(retryEx, "Deployment failed after state recovery attempt for {CorrelationId}.", correlationId);

                throw new StateDriftException(
                    "Deployment failed after a successful state recovery. Inspect the inner exception for the underlying infrastructure error.",
                    retryEx,
                    azureErrorCode,
                    correlationId,
                    recoveryAttempted: true,
                    recoverySuccessful: true);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Deployment failed for {CorrelationId}.", correlationId);
            throw;
        }
    }
}

