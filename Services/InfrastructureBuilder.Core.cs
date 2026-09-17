using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Exceptions;
using DeploymentKit.Extensions;
using DeploymentKit.Helpers.Builder;
using DeploymentKit.Interfaces;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Builder implementation for configuring DeploymentKit infrastructure deployment with fluent API
/// Provides validation, default configurations, and chainable methods for setting up Azure resources
/// </summary>
public partial class InfrastructureBuilder : IInfrastructureBuilder
{
    private readonly IResourceNameValidator _resourceNameValidator;
    private readonly IEnvFileParser _envFileParser;
    private readonly ILogger<InfrastructureBuilder> _logger;
    private readonly List<string> _validationErrors = [];

    /// <summary>
    /// Creates a new InfrastructureBuilder with default implementations.
    /// This is the recommended constructor for most use cases.
    /// </summary>
    public InfrastructureBuilder()
    {
        _resourceNameValidator = BuilderDependencyFactory.CreateDefaultResourceNameValidator();
        _envFileParser = BuilderDependencyFactory.CreateDefaultEnvFileParser();
        _logger = BuilderDependencyFactory.CreateDefaultLogger();
    }

    /// <summary>
    /// Creates a new InfrastructureBuilder with custom dependencies.
    /// This constructor is primarily for dependency injection scenarios and testing.
    /// </summary>
    /// <param name="resourceNameValidator">Custom resource name validator</param>
    /// <param name="envFileParser">Custom environment file parser</param>
    /// <param name="logger">Custom logger</param>
    public InfrastructureBuilder(
        IResourceNameValidator resourceNameValidator,
        IEnvFileParser envFileParser,
        ILogger<InfrastructureBuilder> logger)
    {
        _resourceNameValidator = resourceNameValidator ?? throw new ArgumentNullException(nameof(resourceNameValidator));
        _envFileParser = envFileParser ?? throw new ArgumentNullException(nameof(envFileParser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new InfrastructureBuilder with custom logger but default implementations for other dependencies.
    /// Useful when you want to control logging but don't need custom validation or parsing logic.
    /// </summary>
    /// <param name="logger">Custom logger</param>
    public InfrastructureBuilder(ILogger<InfrastructureBuilder> logger)
    {
        _resourceNameValidator = BuilderDependencyFactory.CreateDefaultResourceNameValidator();
        _envFileParser = BuilderDependencyFactory.CreateDefaultEnvFileParser();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Core configuration
    private string? _deploymentName;
    private string? _environment;
    private string? _location;
    private string? _subscriptionId;
    private string? _resourceGroupName;
    private string? _namingPrefix;
    private InfrastructureSettings? _existingSettings;

    public IInfrastructureBuilder SetName(string deploymentName)
    {
        if (string.IsNullOrWhiteSpace(deploymentName))
        {
            throw new ArgumentException(BuilderConstants.ErrorMessages.DeploymentNameRequired, nameof(deploymentName));
        }

        // Validate name format (alphanumeric, hyphens allowed, 3-50 characters)
        if (!IsValidDeploymentName(deploymentName))
        {
            _validationErrors.Add(BuilderConstants.ErrorMessages.DeploymentNameInvalidFormat);
            return this;
        }

        // Check if name is available using the validation service
        // For deployment names, we'll use a general resource type
        var environment = BuilderHelper.ParseEnvironmentType(_environment ?? InfrastructureConstants.Defaults.Environment);

        try
        {
            _resourceNameValidator.ValidateAndThrowIfDuplicate(deploymentName, ResourceType.ContainerApp, environment);
            _deploymentName = deploymentName;
            _logger.LogInformation(BuilderConstants.Logs.DeploymentNameValidated, deploymentName);
        }
        catch (DuplicateResourceNameException)
        {
            // Re-throw duplicate name exceptions so they can be caught by tests
            throw;
        }
        catch (Exception ex)
        {
            _validationErrors.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, BuilderConstants.ErrorMessages.DeploymentNameNotAvailable, deploymentName, ex.Message));
            _logger.LogWarning(BuilderConstants.Logs.DeploymentNameValidationFailed, deploymentName, ex.Message);
        }

        return this;
    }

    public IInfrastructureBuilder SetEnvironment(string environment)
    {
        if (string.IsNullOrWhiteSpace(environment))
        {
            _validationErrors.Add(BuilderConstants.ErrorMessages.EnvironmentRequired);
            return this;
        }

        var normalizedEnv = environment.ToLowerInvariant();
        if (!normalizedEnv.Equals(EnvironmentType.Development.ToStringValue(), StringComparison.InvariantCultureIgnoreCase) &&
            !normalizedEnv.Equals(EnvironmentType.Production.ToStringValue(), StringComparison.InvariantCultureIgnoreCase))
        {
            _validationErrors.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, BuilderConstants.ErrorMessages.EnvironmentInvalid, EnvironmentType.Development.ToStringValue(), EnvironmentType.Production.ToStringValue()));
            return this;
        }

        _environment = normalizedEnv;
        _logger.LogInformation(BuilderConstants.Logs.EnvironmentSet, _environment);
        return this;
    }

    public IInfrastructureBuilder SetEnvironment(EnvironmentType environment)
    {
        _environment = environment.ToStringValue();
        _logger.LogInformation(BuilderConstants.Logs.EnvironmentSet, _environment);
        return this;
    }

    public IInfrastructureBuilder SetLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new ArgumentException(BuilderConstants.ErrorMessages.LocationRequired, nameof(location));
        }

        _location = location;
        _logger.LogInformation(BuilderConstants.Logs.LocationSet, _location);
        return this;
    }

    public IInfrastructureBuilder SetLocation(AzureLocationType locationType)
    {
        _location = locationType.ToAzureRegion();
        _logger.LogInformation(BuilderConstants.Logs.LocationSetRegion, locationType, _location);
        return this;
    }

    public IInfrastructureBuilder SetSubscriptionId(string subscriptionId)
    {
        if (string.IsNullOrWhiteSpace(subscriptionId))
        {
            throw new ArgumentException(BuilderConstants.ErrorMessages.SubscriptionIdRequired, nameof(subscriptionId));
        }

        _subscriptionId = subscriptionId;
        _logger.LogInformation(BuilderConstants.Logs.SubscriptionIdSet);
        return this;
    }

    public IInfrastructureBuilder SetResourceGroupName(string resourceGroupName)
    {
        if (string.IsNullOrWhiteSpace(resourceGroupName))
        {
            throw new ArgumentException(BuilderConstants.ErrorMessages.ResourceGroupNameRequired, nameof(resourceGroupName));
        }

        _resourceGroupName = resourceGroupName;
        _logger.LogInformation(BuilderConstants.Logs.ResourceGroupNameSet, _resourceGroupName);
        return this;
    }

    public IInfrastructureBuilder SetNamingPrefix(string namingPrefix)
    {
        if (string.IsNullOrWhiteSpace(namingPrefix))
        {
            throw new ArgumentException(BuilderConstants.ErrorMessages.NamingPrefixRequired, nameof(namingPrefix));
        }

        _namingPrefix = namingPrefix;
        _logger.LogInformation(BuilderConstants.Logs.NamingPrefixSet, _namingPrefix);
        return this;
    }

    public IInfrastructureBuilder UseSettings(InfrastructureSettings settings)
    {
        _existingSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger.LogInformation(BuilderConstants.Logs.UsingExistingSettings);
        return this;
    }

}


