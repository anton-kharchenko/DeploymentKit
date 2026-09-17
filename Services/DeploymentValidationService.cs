using DeploymentKit.Constants;
using DeploymentKit.Exceptions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Models.Results;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using System.Text.RegularExpressions;

namespace DeploymentKit.Services;

/// <summary>
/// Service for validating deployment configurations and prerequisites
/// </summary>
public class DeploymentValidationService(ILogger<DeploymentValidationService> logger, ICorrelationIdService correlationIdService) : IDeploymentValidationService
{
    private readonly ILogger<DeploymentValidationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
    private static readonly HttpClient _httpClient = new();

    /// <summary>
    /// Validates green-blue deployment configuration
    /// </summary>
    /// <param name="settings">Infrastructure settings to validate</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Validation result</returns>
    public async Task<ValidationResult> ValidateGreenBlueConfigurationAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Validating green-blue deployment configuration for environment: {Environment}",
                settings.Environment);

            // Validate green-blue deployment settings
            ValidateGreenBlueSettings(settings.GreenBlueDeployment, result);

            // Validate slot configurations
            ValidateSlotConfiguration(settings.GreenSlot, "Green", result);
            ValidateSlotConfiguration(settings.BlueSlot, "Blue", result);

            // Validate traffic distribution
            ValidateTrafficDistribution(settings.GreenSlot, settings.BlueSlot, result);

            // Validate container settings compatibility
            ValidateContainerCompatibility(settings.Container, result);

            // Validate environment-specific requirements
            ValidateEnvironmentRequirements(settings, result);

            // Validate resource requirements
            ValidateResourceRequirements(settings, result);

            // Validate image availability (async operation)
            await ValidateImageAvailabilityAsync(settings, result);

            // Perform comprehensive configuration validation
            try
            {
                ConfigurationValidator.ValidateAllSettings(settings, logger, throwOnError: true);
            }
            catch (ConfigurationValidationException ex)
            {
                result.Errors.Add($"Configuration validation failed: {ex.Message}");
                result.IsValid = false;
            }

            result.IsValid = result.Errors.Count == 0;

            _logger.LogInformation("Configuration validation completed. Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}", result.IsValid, result.Errors.Count, result.Warnings.Count);

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Green-blue configuration validation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration validation failed");

            throw new ResourceCreationException(
                "Green-blue configuration validation failed",
                ex,
                "DeploymentValidation",
                "GreenBlueConfiguration",
                settings.Environment,
                _correlationIdService.GetOrGenerateCorrelationId(),
                "VALIDATION_ERROR");
        }
    }

    /// <summary>
    /// Validates that a deployment is ready to proceed
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="currentOutputs">Current deployment state</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Deployment readiness validation result</returns>
    public async Task<ValidationResult> ValidateDeploymentReadinessAsync(InfrastructureSettings settings, GreenBlueDeploymentOutputs? currentOutputs = null, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Validating deployment readiness for environment: {Environment}", settings.Environment);

            // First validate the configuration
            var configResult = await ValidateGreenBlueConfigurationAsync(settings, cancellationToken);
            if (!configResult.IsValid)
            {
                result.Errors.AddRange(configResult.Errors);
                result.Warnings.AddRange(configResult.Warnings);
                result.IsValid = false;
                return result;
            }

            // Validate current deployment state if provided
            if (currentOutputs != null)
            {
                ValidateCurrentDeploymentState(currentOutputs, result);
            }

            // Validate prerequisites
            await ValidateDeploymentPrerequisitesAsync(settings, result, cancellationToken);

            result.IsValid = result.Errors.Count == 0;

            _logger.LogInformation("Deployment readiness validation completed. Ready: {IsReady}", result.IsValid);
            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Deployment readiness validation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deployment readiness validation failed");

            throw new ResourceCreationException(
                "Deployment readiness validation failed",
                ex,
                "DeploymentValidation",
                "Readiness",
                settings.Environment,
                _correlationIdService.GetOrGenerateCorrelationId(),
                "READINESS_VALIDATION_ERROR");
        }
    }

    /// <summary>
    /// Validates that a deployment can be safely rolled back
    /// </summary>
    /// <param name="currentOutputs">Current deployment state</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Rollback validation result</returns>
    public Task<ValidationResult> ValidateRollbackAsync(GreenBlueDeploymentOutputs currentOutputs, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Validating rollback capability");

            // Validate that we have a valid previous state to roll back to
            ValidateRollbackState(currentOutputs, result);

            // Validate that rollback is safe
            ValidateRollbackSafety(currentOutputs, result);

            result.IsValid = result.Errors.Count == 0;

            _logger.LogInformation("Rollback validation completed. Can rollback: {CanRollback}", result.IsValid);
            return Task.FromResult(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Rollback validation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback validation failed");

            throw new ResourceCreationException(
                "Rollback validation failed",
                ex,
                "DeploymentValidation",
                "Rollback",
                null,
                _correlationIdService.GetOrGenerateCorrelationId(),
                "ROLLBACK_VALIDATION_ERROR");
        }
    }

    /// <summary>
    /// Validates that a deployment can be safely rolled back
    /// </summary>
    /// <param name="currentOutputs">Current deployment state</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>Rollback validation result</returns>
    public Task<ValidationResult> ValidateRollbackReadinessAsync(
        GreenBlueDeploymentOutputs currentOutputs,
        CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Validating rollback readiness for deployment");

            var inactiveSlot = currentOutputs.ActiveSlot == "green" ? currentOutputs.BlueSlot : currentOutputs.GreenSlot;

            if (string.IsNullOrEmpty(inactiveSlot.ImageTag))
            {
                result.Errors.Add("No inactive slot available for rollback");
                result.IsValid = false;
                return Task.FromResult(result);
            }

            if (inactiveSlot.Version == currentOutputs.GreenSlot?.Version &&
                inactiveSlot.Version == currentOutputs.BlueSlot?.Version)
            {
                result.Warnings.Add("Both slots are running the same version");
            }

            result.IsValid = result.Errors.Count == 0;

            _logger.LogInformation("Rollback readiness validation completed. Ready: {IsReady}", result.IsValid);
            return Task.FromResult(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Rollback readiness validation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback readiness validation failed");
            result.IsValid = false;
            result.Errors.Add($"Rollback validation error: {ex.Message}");
            return Task.FromResult(result);
        }
    }

    private static void ValidateGreenBlueSettings(GreenBlueDeploymentSettings settings, ValidationResult result)
    {
        if (!settings.Enabled)
        {
            result.Warnings.Add("Green-blue deployment is disabled");
            return;
        }

        if (settings.HealthCheckTimeout <= 0)
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.HealthCheckTimeoutPositive);
        }

        if (settings.TrafficShiftInterval <= 0)
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.TrafficShiftIntervalPositive);
        }

        if (settings.MaxTrafficShiftPercentage is <= 0 or > 100)
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.MaxTrafficShiftPercentageRange);
        }

        if (settings.RollbackThresholdPercentage is < 0 or > 100)
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.RollbackThresholdPercentageRange);
        }
    }

    private static void ValidateSlotConfiguration(SlotSettings slot, string slotName, ValidationResult result)
    {
        if (string.IsNullOrEmpty(slot.SlotName))
        {
            result.Errors.Add($"{slotName} {ServiceConstants.Validation.Messages.SlotNameCannotBeEmpty}");
        }

        if (slot.CpuLimit <= 0)
        {
            result.Errors.Add($"{slotName} {ServiceConstants.Validation.Messages.SlotCpuLimitPositive}");
        }

        if (slot.MemoryLimit <= 0)
        {
            result.Errors.Add($"{slotName} {ServiceConstants.Validation.Messages.SlotMemoryLimitPositive}");
        }

        if (slot.MinReplicas < 0)
        {
            result.Errors.Add($"{slotName} {ServiceConstants.Validation.Messages.SlotMinReplicasNonNegative}");
        }

        if (slot.MaxReplicas < slot.MinReplicas)
        {
            result.Errors.Add($"{slotName} {ServiceConstants.Validation.Messages.SlotMaxReplicasGreaterThanMin}");
        }

        if (slot.TrafficPercentage is < 0 or > 100)
        {
            result.Errors.Add($"{slotName} {ServiceConstants.Validation.Messages.SlotTrafficPercentageRange}");
        }
    }

    private static void ValidateTrafficDistribution(SlotSettings greenSlot, SlotSettings blueSlot, ValidationResult result)
    {
        var totalTraffic = greenSlot.TrafficPercentage + blueSlot.TrafficPercentage;

        if (Math.Abs(totalTraffic - 100) > 0.01) // Allow for small floating point differences
        {
            result.Errors.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.TotalTrafficMustEqual100, totalTraffic));
        }

        // Validate that at least one slot is active
        if (greenSlot.TrafficPercentage == 0 && blueSlot.TrafficPercentage == 0)
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.AtLeastOneSlotMustReceiveTraffic);
        }
    }

    private static void ValidateContainerCompatibility(ContainerSettings container, ValidationResult result)
    {
        if (string.IsNullOrEmpty(container.ApiImageTag))
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.ApiImageTagRequired);
        }

        if (string.IsNullOrEmpty(container.JobsImageTag))
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.JobsImageTagRequired);
        }

        if (container.ApiCpuLimit <= 0)
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.ApiCpuLimitPositive);
        }

        if (container.ApiMemoryLimit <= 0)
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.ApiMemoryLimitPositive);
        }

        // Check for high resource requirements
        if (container.ApiCpuLimit > ServiceConstants.Validation.Limits.HighCpuThreshold)
        {
            result.AddWarning(string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.HighCpuRequirement, container.ApiCpuLimit));
        }

        if (container.ApiMemoryLimit > ServiceConstants.Validation.Limits.HighMemoryThreshold)
        {
            result.AddWarning(string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.HighMemoryRequirement, container.ApiMemoryLimit));
        }

        // Validate image tag formats
        if (!IsValidImageTag(container.ApiImageTag))
        {
            result.AddError(string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.InvalidApiImageTagFormat, container.ApiImageTag));
        }

        if (!IsValidImageTag(container.JobsImageTag))
        {
            result.AddError(string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.InvalidJobsImageTagFormat, container.JobsImageTag));
        }
    }

    private static void ValidateEnvironmentRequirements(InfrastructureSettings settings, ValidationResult result)
    {
        // Environment-specific recommendations
        if (settings.Environment.Equals(ServiceConstants.Validation.ProductionEnvironment, StringComparison.OrdinalIgnoreCase))
        {
            if (!settings.GreenBlueDeployment.Enabled)
            {
                result.AddRecommendation(ServiceConstants.Validation.Messages.GreenBlueRecommendedForProduction);
            }

            if (settings is { BlueSlot: not null, GreenSlot: not null } && (settings.GreenSlot.MinReplicas < 2 || settings.BlueSlot.MinReplicas < 2))
            {
                result.AddRecommendation(ServiceConstants.Validation.Messages.ProductionShouldHaveTwoReplicas);
            }
        }
        else if (settings.Environment.Equals(ServiceConstants.Validation.DevelopmentEnvironment, StringComparison.OrdinalIgnoreCase))
        {
            if (settings.GreenSlot.MaxReplicas > 2 || settings.BlueSlot.MaxReplicas > 2)
            {
                result.AddRecommendation(ServiceConstants.Validation.Messages.DevelopmentDoesntNeedManyReplicas);
            }
        }
    }

    private static void ValidateCurrentDeploymentState(GreenBlueDeploymentOutputs outputs, ValidationResult result)
    {
        // Check if deployment is in a stable state
        if (outputs.DeploymentStatus == ServiceConstants.Validation.InProgressStatus)
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.CannotStartNewDeploymentInProgress);
        }

        // Validate active slot
        if (string.IsNullOrEmpty(outputs.ActiveSlot))
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.ActiveSlotMustBeSpecified);
        }
        else if (outputs.ActiveSlot != ServiceConstants.Validation.GreenSlot &&
                 outputs.ActiveSlot != ServiceConstants.Validation.BlueSlot)
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.ActiveSlotMustBeGreenOrBlue);
        }
    }

    private static async Task ValidateDeploymentPrerequisitesAsync(InfrastructureSettings settings, ValidationResult result, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Validate Azure subscription and resource group
        if (string.IsNullOrEmpty(settings.ResourceGroupName))
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.ResourceGroupNameRequired);
        }

        if (string.IsNullOrEmpty(settings.Location))
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.AzureLocationRequired);
        }

        // Container registry validation is handled through ContainerRegistryOutputs during deployment
        // No need to validate RegistryUrl in ContainerSettings as it's not part of the settings model

        // Validate network settings
        if (settings.Network != null)
        {
            ValidateNetworkSettings(settings.Network, result);
        }

        // Validate storage settings
        if (settings.Storage != null)
        {
            ValidateStorageSettings(settings.Storage, result);
        }

        await Task.CompletedTask;
    }

    private static void ValidateRollbackState(GreenBlueDeploymentOutputs outputs, ValidationResult result)
    {
        // Validate that there's an inactive slot to rollback to
        var inactiveSlot = outputs.ActiveSlot == ServiceConstants.Validation.GreenSlot ? outputs.BlueSlot : outputs.GreenSlot;

        if (string.IsNullOrEmpty(inactiveSlot.ImageTag))
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.NoInactiveSlotForRollback);
        }

        if (string.IsNullOrEmpty(inactiveSlot.Version))
        {
            result.Errors.Add(ServiceConstants.Validation.Messages.InactiveSlotVersionMissing);
        }
    }

    private static void ValidateRollbackSafety(GreenBlueDeploymentOutputs outputs, ValidationResult result)
    {
        // Check if both slots are running the same version
        if (outputs.GreenSlot?.Version == outputs.BlueSlot?.Version)
        {
            result.Warnings.Add(ServiceConstants.Validation.Messages.BothSlotsRunSameVersion);
        }

        // Validate that the inactive slot was previously healthy
        var inactiveSlot = outputs.ActiveSlot == ServiceConstants.Validation.GreenSlot ? outputs.BlueSlot : outputs.GreenSlot;

        if (inactiveSlot?.LastHealthCheckTimestamp != null)
        {
            // We only know the time of the last health check, not the result; keep as informational warning
            result.Warnings.Add(ServiceConstants.Validation.Messages.InactiveSlotHealthCheckWarning);
        }

        // Check deployment timestamp to ensure we're not rolling back to a very old version
        if (inactiveSlot == null)
            return;

        var daysSinceDeployment = (DateTime.UtcNow - inactiveSlot.DeploymentTimestamp).TotalDays;
        if (daysSinceDeployment > ServiceConstants.Validation.Limits.RollbackAgeWarningDays)
        {
            result.Warnings.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.InactiveSlotOldDeployment, daysSinceDeployment));
        }
    }

    private static void ValidateNetworkSettings(NetworkSettings network, ValidationResult result)
    {

        // Validate CIDR blocks if provided
        if (!string.IsNullOrEmpty(network.VNetAddressSpace))
        {
            if (!IsValidCidr(network.VNetAddressSpace))
            {
                result.Errors.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.InvalidVNetAddressSpace, network.VNetAddressSpace));
            }
        }

        if (!string.IsNullOrEmpty(network.ContainerAppsSubnet))
        {
            if (!IsValidCidr(network.ContainerAppsSubnet))
            {
                result.Errors.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.InvalidContainerAppsSubnet, network.ContainerAppsSubnet));
            }
        }

        if (string.IsNullOrEmpty(network.DatabaseSubnet))
        {
            return;
        }

        if (!IsValidCidr(network.DatabaseSubnet))
        {
            result.Errors.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.InvalidDatabaseSubnet, network.DatabaseSubnet));
        }
    }

    private static void ValidateStorageSettings(StorageSettings storage, ValidationResult result)
    {
        // Storage account name is generated automatically, so no validation needed for AccountName

        if (string.IsNullOrEmpty(storage.ReplicationTypeString))
        {
            result.Errors.Add("Storage replication type is required");
        }
        else
        {
            // Validate storage replication type
            var validTypes = new[] {
                StorageConstants.StandardLrs,
                "Standard_GRS",
                "Standard_RAGRS",
                "Standard_ZRS",
                "Premium_LRS"
            };
            if (!validTypes.Contains(storage.ReplicationTypeString))
            {
                result.Errors.Add($"Invalid storage replication type: {storage.ReplicationTypeString}. Valid types are: {string.Join(", ", validTypes)}");
            }
        }
    }

    private static void ValidateResourceRequirements(InfrastructureSettings settings, ValidationResult result)
    {
        // Calculate total resource requirements
        var totalCpu = settings.GreenSlot.CpuLimit * settings.GreenSlot.MaxReplicas + settings.BlueSlot.CpuLimit * settings.BlueSlot.MaxReplicas;

        var totalMemory = settings.GreenSlot.MemoryLimit * settings.GreenSlot.MaxReplicas + settings.BlueSlot.MemoryLimit * settings.BlueSlot.MaxReplicas;

        // Warn about high resource usage
        if (totalCpu > 10) // 10 CPU cores
        {
            result.Warnings.Add($"High CPU requirement detected: {totalCpu} cores");
        }

        if (totalMemory > 20480) // 20GB
        {
            result.Warnings.Add($"High memory requirement detected: {totalMemory}MB");
        }
    }

    private static bool IsValidCidr(string cidr)
    {
        if (string.IsNullOrEmpty(cidr))
            return false;

        var parts = cidr.Split('/');
        if (parts.Length != 2)
            return false;

        // Validate IP address part
        if (!System.Net.IPAddress.TryParse(parts[0], out _))
            return false;

        // Validate subnet mask
        if (!int.TryParse(parts[1], out var mask) || mask < 0 || mask > 32)
            return false;

        return true;
    }

    private static bool IsValidStorageAccountName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        // Storage account names must be between 3 and 24 characters, lowercase letters and numbers only
        if (name.Length < ServiceConstants.Validation.Limits.StorageAccountNameMinLength ||
            name.Length > ServiceConstants.Validation.Limits.StorageAccountNameMaxLength)
            return false;

        return name.All(c => char.IsLower(c) || char.IsDigit(c));
    }

    /// <summary>
    /// Validates image availability in container registry
    /// </summary>
    public async Task<ValidationResult> ValidateImageAvailabilityAsync(
        string registryUrl,
        string imageName,
        string imageTag,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Validating image availability: {RegistryUrl}/{ImageName}:{ImageTag}", registryUrl, imageName, imageTag);

            // Validate input parameters
            if (string.IsNullOrWhiteSpace(registryUrl))
                throw new ArgumentException("Registry URL cannot be null or empty", nameof(registryUrl));
            if (string.IsNullOrWhiteSpace(imageName))
                throw new ArgumentException("Image name cannot be null or empty", nameof(imageName));
            if (string.IsNullOrWhiteSpace(imageTag))
                throw new ArgumentException("Image tag cannot be null or empty", nameof(imageTag));

            var validationItems = new List<HealthCheckItem>();

            // Validate registry URL format
            if (!Uri.TryCreate(registryUrl, UriKind.Absolute, out _))
            {
                validationItems.Add(new HealthCheckItem
                {
                    IsHealthy = false,
                    Status = "Failed",
                    Message = string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.InvalidRegistryUrlFormat, registryUrl)
                });
            }
            else
            {
                validationItems.Add(new HealthCheckItem
                {
                    IsHealthy = true,
                    Status = "Passed",
                    Message = ServiceConstants.Validation.Messages.RegistryUrlFormatValid
                });
            }

            // Validate image tag format
            if (!IsValidImageTag(imageTag))
            {
                validationItems.Add(new HealthCheckItem
                {
                    IsHealthy = false,
                    Status = "Failed",
                    Message = string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.InvalidImageTagFormat, imageTag)
                });
            }
            else
            {
                validationItems.Add(new HealthCheckItem
                {
                    IsHealthy = true,
                    Status = "Passed",
                    Message = ServiceConstants.Validation.Messages.ImageTagFormatValid
                });
            }

            try
            {
                // Attempt to check registry connectivity and image existence
                var registryConnectivityResult = await CheckRegistryConnectivityAsync(registryUrl, cancellationToken);
                validationItems.Add(registryConnectivityResult);

                if (registryConnectivityResult.IsHealthy)
                {
                    // Only check image existence if registry is accessible
                    var imageExistenceResult = await CheckImageExistenceAsync(registryUrl, imageName, imageTag, cancellationToken);
                    validationItems.Add(imageExistenceResult);
                }
                else
                {
                    validationItems.Add(new HealthCheckItem
                    {
                        IsHealthy = false,
                        Status = "Failed",
                        Message = "Skipping image existence check due to registry connectivity failure"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Registry connectivity check failed: {Message}", ex.Message);
                validationItems.Add(new HealthCheckItem
                {
                    IsHealthy = false,
                    Status = "Warning",
                    Message = $"Registry connectivity check failed: {ex.Message}"
                });
            }

            var isValid = validationItems.All(item => item.Status != "Failed");
            var hasWarnings = validationItems.Any(item => item.Status == "Warning");

            _logger.LogInformation("Image availability validation completed. Valid: {IsValid}, Warnings: {HasWarnings}", isValid, hasWarnings);

            return new ValidationResult
            {
                IsValid = isValid,
                Errors = validationItems.Where(item => item.Status == "Failed").Select(item => item.Message).ToList(),
                Warnings = validationItems.Where(item => item.Status == "Warning").Select(item => item.Message).ToList(),
                Recommendations = validationItems.Where(item => item.Status == "Recommendation").Select(item => item.Message).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image availability validation failed");

            throw new ResourceCreationException(
                "Image availability validation failed",
                ex,
                "DeploymentValidation",
                "ImageAvailability",
                null,
                _correlationIdService.GetOrGenerateCorrelationId(),
                "IMAGE_VALIDATION_ERROR");
        }
    }

    private static async Task ValidateImageAvailabilityAsync(InfrastructureSettings settings, ValidationResult result)
    {
        if (!IsValidImageTag(settings.Container.ApiImageTag))
        {
            result.Errors.Add($"Invalid API image tag format: {settings.Container.ApiImageTag}");
        }

        if (!IsValidImageTag(settings.Container.JobsImageTag))
        {
            result.Errors.Add($"Invalid Jobs image tag format: {settings.Container.JobsImageTag}");
        }

        await Task.CompletedTask; // Image tag format validation completed
    }

    private static bool IsValidImageTag(string imageTag)
    {
        if (string.IsNullOrEmpty(imageTag))
            return false;

        // Basic validation - should contain at least image name and tag
        return imageTag.Contains(':') && !imageTag.StartsWith(':') && !imageTag.EndsWith(':');
    }

    /// <summary>
    /// Checks registry connectivity by attempting to access the registry's v2 API endpoint
    /// </summary>
    private async Task<HealthCheckItem> CheckRegistryConnectivityAsync(string registryUrl, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(ServiceConstants.Validation.Limits.RegistryConnectivityTimeoutSeconds));

            var request = new HttpRequestMessage(HttpMethod.Get, $"{registryUrl.TrimEnd('/')}{ServiceConstants.Validation.Endpoints.RegistryV2Api}");
            request.Headers.Add(ServiceConstants.Validation.HttpHeaders.CorrelationId, _correlationIdService.GetOrGenerateCorrelationId());

            _logger.LogDebug("Checking registry connectivity: {Endpoint}", request.RequestUri);

            var response = await _httpClient.SendAsync(request, cts.Token);

            // For ACR, we expect either 200 (if anonymous access) or 401 (authentication required)
            // Both indicate the registry is accessible
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new HealthCheckItem
                {
                    IsHealthy = true,
                    Status = "Passed",
                    Message = string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.RegistryAccessible, registryUrl)
                };
            }

            return new HealthCheckItem
            {
                IsHealthy = false,
                Status = "Failed",
                Message = string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.RegistryConnectivityFailed,
                    (int)response.StatusCode, response.ReasonPhrase)
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return new HealthCheckItem
            {
                IsHealthy = false,
                Status = "Failed",
                Message = ServiceConstants.Validation.Messages.RegistryConnectivityTimeout
            };
        }
        catch (OperationCanceledException)
        {
            return new HealthCheckItem
            {
                IsHealthy = false,
                Status = "Failed",
                Message = ServiceConstants.Validation.Messages.RegistryConnectivityTimeout
            };
        }
        catch (HttpRequestException ex)
        {
            return new HealthCheckItem
            {
                IsHealthy = false,
                Status = "Failed",
                Message = $"Registry connectivity failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Checks if a specific image exists in the registry by attempting to access its manifest
    /// </summary>
    private async Task<HealthCheckItem> CheckImageExistenceAsync(string registryUrl, string imageName, string imageTag, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(ServiceConstants.Validation.Limits.ImageExistenceTimeoutSeconds));

            // Parse image name and tag from the full image reference
            var imageReference = $"{imageName}:{imageTag}";
            if (imageReference.Contains('/'))
            {
                // Handle repository/image:tag format
                var parts = imageReference.Split(':');
                if (parts.Length >= 2)
                {
                    imageName = parts[0];
                    imageTag = parts[1];
                }
            }

            var manifestUrl = string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Endpoints.ManifestPath, imageName, imageTag);
            var fullUrl = $"{registryUrl.TrimEnd('/')}{manifestUrl}";

            var request = new HttpRequestMessage(HttpMethod.Head, fullUrl);
            request.Headers.Add("Accept", ServiceConstants.Validation.HttpHeaders.DockerManifestMediaType);
            request.Headers.Add(ServiceConstants.Validation.HttpHeaders.CorrelationId, _correlationIdService.GetOrGenerateCorrelationId());

            _logger.LogDebug("Checking image existence: {Endpoint}", fullUrl);

            var response = await _httpClient.SendAsync(request, cts.Token);

            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.OK => new HealthCheckItem
                {
                    IsHealthy = true,
                    Status = "Passed",
                    Message = string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.ImageExists, imageName, imageTag)
                },
                System.Net.HttpStatusCode.NotFound => new HealthCheckItem
                {
                    IsHealthy = false,
                    Status = "Failed",
                    Message = string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.ImageNotFound, imageName, imageTag)
                },
                System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden => new HealthCheckItem
                {
                    IsHealthy = false,
                    Status = "Warning",
                    Message = string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.ImageExistenceAuthRequired, imageName, imageTag)
                },
                _ => new HealthCheckItem
                {
                    IsHealthy = false,
                    Status = "Warning",
                    Message = string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.ImageExistenceInconclusive,
                        (int)response.StatusCode, response.ReasonPhrase)
                }
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return new HealthCheckItem
            {
                IsHealthy = false,
                Status = "Warning",
                Message = ServiceConstants.Validation.Messages.ImageExistenceCheckTimeout
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckItem
            {
                IsHealthy = false,
                Status = "Warning",
                Message = string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.Validation.Messages.ImageExistenceCheckFailed, ex.Message)
            };
        }
    }

    public async Task<ValidationResult> ValidateNamingPrefixAsync(InfrastructureSettings settings, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Validating naming prefix for environment: {Environment}", settings.Environment);

            // Basic prefix validation
            if (string.IsNullOrWhiteSpace(settings.NamingPrefix))
            {
                result.AddError("Naming prefix cannot be empty");
            }
            else if (settings.NamingPrefix.Length > 20)
            {
                result.AddError("Naming prefix cannot exceed 20 characters");
            }
            else if (!Regex.IsMatch(settings.NamingPrefix, @"^[a-z][a-z0-9]*$"))
            {
                result.AddError("Naming prefix must start with a lowercase letter and contain only lowercase letters and numbers");
            }

            // Environment validation
            if (string.IsNullOrWhiteSpace(settings.Environment))
            {
                result.AddError("Environment cannot be empty");
            }
            else if (settings.Environment.Length < 3)
            {
                result.AddError("Environment must be at least 3 characters long");
            }
            else if (settings.Environment.Length > 10)
            {
                result.AddError("Environment cannot exceed 10 characters");
            }
            else if (!Regex.IsMatch(settings.Environment, @"^[a-zA-Z0-9]+$"))
            {
                result.AddError("Environment can only contain alphanumeric characters");
            }
            else if (!InfrastructureConstants.Validation.ValidEnvironments.Contains(settings.Environment.ToLowerInvariant()))
            {
                result.AddWarning($"Environment '{settings.Environment}' is not a standard environment name. Consider using: {string.Join(", ", InfrastructureConstants.Validation.ValidEnvironments)}");
            }

            // If basic validation failed, still compute recommendations but mark invalid
            // Compute resource name lengths based on current prefix and environment
            var storageLen = ComputeSanitizedLength(InfrastructureConstants.NamingPatterns.StorageAccount, settings.NamingPrefix, settings.Environment);
            var keyVaultLen = ComputeSanitizedLength(InfrastructureConstants.NamingPatterns.KeyVault, settings.NamingPrefix, settings.Environment);
            var acrLen = ComputeSanitizedLength(InfrastructureConstants.NamingPatterns.ContainerRegistry, settings.NamingPrefix, settings.Environment);

            if (storageLen > InfrastructureConstants.NamingPatterns.MaxStorageAccountNameLength)
            {
                result.AddError($"Computed Storage Account name length ({storageLen}) exceeds maximum allowed ({InfrastructureConstants.NamingPatterns.MaxStorageAccountNameLength}).");
                var overhead = ComputeSanitizedLength(InfrastructureConstants.NamingPatterns.StorageAccount, string.Empty, settings.Environment);
                var allowed = Math.Max(0, InfrastructureConstants.NamingPatterns.MaxStorageAccountNameLength - overhead);
                result.AddRecommendation($"Reduce NamingPrefix to at most {allowed} characters for compliant Storage Account names.");
            }

            if (keyVaultLen > InfrastructureConstants.NamingPatterns.MaxKeyVaultNameLength)
            {
                result.AddError($"Computed Key Vault name length ({keyVaultLen}) exceeds maximum allowed ({InfrastructureConstants.NamingPatterns.MaxKeyVaultNameLength}).");
                var overhead = ComputeSanitizedLength(InfrastructureConstants.NamingPatterns.KeyVault, string.Empty, settings.Environment);
                var allowed = Math.Max(0, InfrastructureConstants.NamingPatterns.MaxKeyVaultNameLength - overhead);
                result.AddRecommendation($"Reduce NamingPrefix to at most {allowed} characters for compliant Key Vault names.");
            }

            if (acrLen > InfrastructureConstants.NamingPatterns.MaxContainerRegistryNameLength)
            {
                result.AddError($"Computed Container Registry name length ({acrLen}) exceeds maximum allowed ({InfrastructureConstants.NamingPatterns.MaxContainerRegistryNameLength}).");
                var overhead = ComputeSanitizedLength(InfrastructureConstants.NamingPatterns.ContainerRegistry, string.Empty, settings.Environment);
                var allowed = Math.Max(0, InfrastructureConstants.NamingPatterns.MaxContainerRegistryNameLength - overhead);
                result.AddRecommendation($"Reduce NamingPrefix to at most {allowed} characters for compliant Container Registry names.");
            }

            result.IsValid = result.Errors.Count == 0;

            _logger.LogInformation("Naming prefix validation completed. Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}",
                result.IsValid, result.Errors.Count, result.Warnings.Count);

            return await Task.FromResult(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Naming prefix validation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Naming prefix validation failed");
            throw new ResourceCreationException(
                "Naming prefix validation failed",
                ex,
                "DeploymentValidation",
                "NamingPrefix",
                settings.Environment,
                _correlationIdService.GetOrGenerateCorrelationId(),
                "VALIDATION_ERROR");
        }
    }

    private static int ComputeSanitizedLength(string pattern, string prefix, string environment)
    {
        var baseName = string.Format(System.Globalization.CultureInfo.InvariantCulture, pattern, prefix, environment);
        var sanitized = Regex.Replace(baseName, "[^a-zA-Z0-9]", "").ToLowerInvariant();
        return sanitized.Length;
    }
}


