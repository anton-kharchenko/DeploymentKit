using DeploymentKit.Components;
using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Exceptions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.ContainerRegistry;
using Pulumi.AzureNative.ContainerRegistry.Inputs;
using Task = System.Threading.Tasks.Task;

namespace DeploymentKit.Services;

public class ContainerRegistryService(ILogger<ContainerRegistryService> logger, IResourceNamingService namingService, ICorrelationIdService correlationIdService, IResourceNameValidator resourceNameValidator) : IContainerRegistryService
{
    private readonly ILogger<ContainerRegistryService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IResourceNamingService _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
    private readonly IResourceNameValidator _resourceNameValidator = resourceNameValidator ?? throw new ArgumentNullException(nameof(resourceNameValidator));
    public Task<ContainerRegistryOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        var componentName = $"{settings.NamingPrefix}-app-registry-{settings.Environment}";
        return DeploymentKitApp.CreateContainerRegistryAsync(componentName, () => CreateCoreAsync(settings, resourceGroup, cancellationToken));
    }

    private Task<ContainerRegistryOutputs> CreateCoreAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        if (string.IsNullOrEmpty(settings.NamingPrefix))
            throw new ArgumentException("NamingPrefix cannot be null or empty");

        if (string.IsNullOrEmpty(settings.Environment))
            throw new ArgumentException("Environment cannot be null or empty");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation(ServiceConstants.ContainerRegistry.CreationStartMessage, settings.Environment);

            var acrName = settings.Container?.RegistryNameOverride ?? _namingService.GenerateContainerRegistryName(settings.NamingPrefix, settings.Environment);

            // Map environment to EnvironmentType enum
            var enumType = MapEnvironmentToEnumType(settings.Environment);

            // Validate resource name uniqueness before creation
            _resourceNameValidator.ValidateAndThrowIfDuplicate(
                acrName,
                ResourceType.ContainerRegistry,
                enumType,
                _correlationIdService.GetOrGenerateCorrelationId());

            // Create Azure Container Registry
            var acr = new Registry(acrName, new RegistryArgs
            {
                RegistryName = acrName,
                ResourceGroupName = resourceGroup,
                Location = settings.Location,
                AdminUserEnabled = true,
                Sku = new SkuArgs
                {
                    Name = SkuName.Basic
                },
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, ServiceConstants.ContainerRegistry.ResourceType)
            }, ComponentResourceScope.CreateChildOptions(acrName));

            // Get ACR credentials
            var acrCreds = ListRegistryCredentials.Invoke(new ListRegistryCredentialsInvokeArgs
            {
                ResourceGroupName = resourceGroup,
                RegistryName = acr.Name,
            });

            var outputs = new ContainerRegistryOutputs
            {
                LoginServer = acr.LoginServer,
                Username = acrCreds.Apply(c => c.Username ?? string.Empty),
                Password = Output.CreateSecret(acrCreds.Apply(c => c.Passwords.Length > 0 ? c.Passwords[0].Value ?? string.Empty : string.Empty)),
                Name = acrName,
                ResourceId = acr.Id
            };

            // Map environment to EnvironmentType enum
            var envType = MapEnvironmentToEnumType(settings.Environment);

            // Register the validated resource name to avoid future duplicates in the same execution context
            _resourceNameValidator.RegisterValidatedResourceName(
                acrName,
                ResourceType.ContainerRegistry,
                envType,
                _correlationIdService.GetOrGenerateCorrelationId());

            _logger.LogInformation(ServiceConstants.ContainerRegistry.CreationSuccessMessage, acrName);
            return Task.FromResult(outputs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ServiceConstants.ContainerRegistry.CreationFailedMessage, settings.Environment);
            throw new ResourceCreationException(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.ContainerRegistry.CreationFailedMessage, settings.Environment),
                ex,
                ServiceConstants.ResourceTypes.ContainerRegistry,
                ServiceConstants.ResourceTypes.Registry,
                settings.Environment,
                _correlationIdService.GetOrGenerateCorrelationId(),
                ServiceConstants.ErrorCodes.AcrCreationFailed);
        }
    }

    /// <summary>
    /// Maps environment string to EnvironmentType enum
    /// </summary>
    private static EnvironmentType MapEnvironmentToEnumType(string environment)
    {
        return environment.ToLowerInvariant() switch
        {
            "dev" or "development" => EnvironmentType.Development,
            "prod" or "production" => EnvironmentType.Production,
            "test" => EnvironmentType.Development, // Map test to Development
            "staging" => EnvironmentType.Production, // Map staging to Production
            _ => EnvironmentType.Development // Default to Development
        };
    }

    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken) => await CreateAsync(settings, resourceGroup, cancellationToken);

    /// <summary>
    /// Explicit implementation of IInfrastructureService.CreateAsync without CancellationToken
    /// </summary>
    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup) => await CreateAsync(settings, resourceGroup);
}
