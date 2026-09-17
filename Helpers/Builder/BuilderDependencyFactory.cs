using DeploymentKit.Interfaces;
using DeploymentKit.Services;

namespace DeploymentKit.Helpers.Builder;

/// <summary>
/// Helper class for setting up InfrastructureBuilder dependencies.
/// </summary>
public static class BuilderDependencyFactory
{
    /// <summary>
    /// Creates a default resource name validator with minimal logging
    /// </summary>
    public static IResourceNameValidator CreateDefaultResourceNameValidator()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        var registryLogger = loggerFactory.CreateLogger<ResourceNameRegistry>();
        var validatorLogger = loggerFactory.CreateLogger<ResourceNameValidationService>();

        var registry = new ResourceNameRegistry(registryLogger);
        return new ResourceNameValidationService(registry, validatorLogger);
    }

    /// <summary>
    /// Creates a default environment file parser with minimal logging
    /// </summary>
    public static IEnvFileParser CreateDefaultEnvFileParser()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        var logger = loggerFactory.CreateLogger<EnvFileParser>();
        return new EnvFileParser(logger);
    }

    /// <summary>
    /// Creates a default logger for InfrastructureBuilder with console output
    /// </summary>
    public static ILogger<InfrastructureBuilder> CreateDefaultLogger()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        return loggerFactory.CreateLogger<InfrastructureBuilder>();
    }
}

