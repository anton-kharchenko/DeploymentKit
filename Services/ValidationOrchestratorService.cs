using DeploymentKit.Interfaces;
using DeploymentKit.Models.Results;
using DeploymentKit.Settings;

namespace DeploymentKit.Services;

/// <summary>
/// Orchestrates comprehensive validation across all validation services.
/// Coordinates pre-deployment validation, Azure state validation, and drift detection.
/// </summary>
public class ValidationOrchestratorService(
    ILogger<ValidationOrchestratorService> logger,
    IPreDeploymentValidator preDeploymentValidator,
    IAzureResourceStateValidator? azureStateValidator = null,
    IDriftDetectionService? driftDetectionService = null,
    INamingConsistencyValidator? namingValidator = null)
    : IValidationOrchestratorService
{
    private readonly ILogger<ValidationOrchestratorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPreDeploymentValidator _preDeploymentValidator = preDeploymentValidator ?? throw new ArgumentNullException(nameof(preDeploymentValidator));
    private readonly IAzureResourceStateValidator? _azureStateValidator = azureStateValidator;
    private readonly IDriftDetectionService? _driftDetectionService = driftDetectionService;
    private readonly INamingConsistencyValidator? _namingValidator = namingValidator;

    public async Task<PreDeploymentValidationResult> RunComprehensiveValidationAsync(
        InfrastructureSettings settings,
        bool includeStateValidation = true,
        bool includeDriftDetection = true)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _logger.LogInformation("🚀 Starting comprehensive validation for environment: {Environment}", settings.Environment);

        var result = new PreDeploymentValidationResult();
        var validationTasks = new List<Task<PreDeploymentValidationResult>>();

        try
        {
            // Always run pre-deployment validation
            validationTasks.Add(RunPreDeploymentValidationAsync(settings));

            // Conditionally run Azure state validation
            if (includeStateValidation && _azureStateValidator != null)
            {
                validationTasks.Add(RunAzureStateValidationAsync(settings));
            }
            else if (includeStateValidation)
            {
                _logger.LogWarning("Azure state validation requested but IAzureResourceStateValidator not available");
                result.Warnings.Add("Azure state validation skipped - service not configured");
            }

            // Conditionally run drift detection
            if (includeDriftDetection && _driftDetectionService != null)
            {
                validationTasks.Add(RunDriftDetectionAsync(settings));
            }
            else if (includeDriftDetection)
            {
                _logger.LogWarning("Drift detection requested but IDriftDetectionService not available");
                result.Warnings.Add("Drift detection skipped - service not configured");
            }

            // Execute all validation tasks concurrently
            var validationResults = await Task.WhenAll(validationTasks);

            // Combine all validation results
            foreach (var validationResult in validationResults)
            {
                result.Errors.AddRange(validationResult.Errors);
                result.Warnings.AddRange(validationResult.Warnings);
                result.ValidatedResources.AddRange(validationResult.ValidatedResources);
            }

            result.IsValid = result.Errors.Count == 0;

            // Log comprehensive summary
            _logger.LogInformation("📊 Comprehensive validation completed:");
            _logger.LogInformation("   ✅ Validated Resources: {ValidatedCount}", result.ValidatedResources.Count);
            _logger.LogInformation("   ⚠️  Warnings: {WarningCount}", result.Warnings.Count);
            _logger.LogInformation("   ❌ Errors: {ErrorCount}", result.Errors.Count);
            _logger.LogInformation("   🎯 Overall Result: {Result}", result.IsValid ? "PASSED" : "FAILED");

            // Add summary to result
            result.ValidatedResources.Add($"📊 Comprehensive Validation Summary: {(result.IsValid ? "PASSED" : "FAILED")} - {result.ValidatedResources.Count} resources validated, {result.Warnings.Count} warnings, {result.Errors.Count} errors");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Comprehensive validation failed for environment: {Environment}", settings.Environment);
            result.Errors.Add($"Comprehensive validation failed: {ex.Message}");
        }

        return result;
    }

    public async Task<PreDeploymentValidationResult> RunPreDeploymentValidationAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _logger.LogInformation("🔍 Running pre-deployment validation for environment: {Environment}", settings.Environment);

        try
        {
            var result = await _preDeploymentValidator.ValidateAllAsync(settings);

            _logger.LogInformation("Pre-deployment validation completed with {ErrorCount} errors and {WarningCount} warnings",
                result.Errors.Count, result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pre-deployment validation failed");
            return new PreDeploymentValidationResult
            {
                Errors = { $"Pre-deployment validation failed: {ex.Message}" }
            };
        }
    }

    public async Task<PreDeploymentValidationResult> RunAzureStateValidationAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (_azureStateValidator == null)
        {
            _logger.LogWarning("Azure state validator not available");
            return new PreDeploymentValidationResult
            {
                IsValid = true,
                Warnings = { "Azure state validator skipped - service not configured" }
            };
        }

        _logger.LogInformation("🔍 Running Azure state validation for environment: {Environment}", settings.Environment);

        try
        {
            var result = await _azureStateValidator.ValidatePreDeploymentStateAsync(settings, CancellationToken.None);

            _logger.LogInformation("Azure state validation completed with {ErrorCount} errors and {WarningCount} warnings",
                result.Errors.Count, result.Warnings.Count);

            return ConvertToPreDeploymentValidationResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure state validation failed");
            return new PreDeploymentValidationResult
            {
                Errors = { $"Azure state validation failed: {ex.Message}" }
            };
        }
    }

    public async Task<PreDeploymentValidationResult> RunDriftDetectionAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (_driftDetectionService == null)
        {
            _logger.LogWarning("Drift detection service not available");
            return new PreDeploymentValidationResult
            {
                IsValid = true,
                Warnings = { "Drift detection service skipped - service not configured" }
            };
        }

        _logger.LogInformation("🔄 Running drift detection for environment: {Environment}", settings.Environment);

        try
        {
            var result = await _driftDetectionService.DetectDriftAsync(settings);

            _logger.LogInformation("Drift detection completed with {ErrorCount} errors and {WarningCount} warnings",
                result.Errors.Count, result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Drift detection failed");
            return new PreDeploymentValidationResult
            {
                Errors = { $"Drift detection failed: {ex.Message}" }
            };
        }
    }

    public async Task<PreDeploymentValidationResult> GenerateValidationReportAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _logger.LogInformation("📋 Generating comprehensive validation report for environment: {Environment}", settings.Environment);

        var result = await RunComprehensiveValidationAsync(settings, includeStateValidation: true, includeDriftDetection: true);

        // Enhance result with additional reporting information
        var reportSections = new List<string>
        {
            "🎯 VALIDATION REPORT",
            $"Environment: {settings.Environment}",
            $"Location: {settings.Location}",
            $"Resource Group: {settings.ResourceGroupName}",
            $"Naming Prefix: {settings.NamingPrefix}",
            "",
            "📊 SUMMARY:",
            $"   Total Resources Validated: {result.ValidatedResources.Count}",
            $"   Warnings: {result.Warnings.Count}",
            $"   Errors: {result.Errors.Count}",
            $"   Overall Status: {(result.IsValid ? "✅ PASSED" : "❌ FAILED")}",
            ""
        };

        if (result.Errors.Any())
        {
            reportSections.Add("❌ ERRORS:");
            reportSections.AddRange(result.Errors.Select(error => $"   • {error}"));
            reportSections.Add("");
        }

        if (result.Warnings.Any())
        {
            reportSections.Add("⚠️ WARNINGS:");
            reportSections.AddRange(result.Warnings.Select(warning => $"   • {warning}"));
            reportSections.Add("");
        }

        if (result.ValidatedResources.Any())
        {
            reportSections.Add("✅ VALIDATED RESOURCES:");
            reportSections.AddRange(result.ValidatedResources.Select(resource => $"   • {resource}"));
            reportSections.Add("");
        }

        // Add recommendations
        reportSections.Add("💡 RECOMMENDATIONS:");
        if (!result.IsValid)
        {
            reportSections.Add("   • Fix all errors before proceeding with deployment");
            if (result.Warnings.Any(w => w.Contains("not found")))
            {
                reportSections.Add("   • Run 'pulumi refresh' to sync state with Azure");
            }
            if (result.Warnings.Any(w => w.Contains("mismatch")))
            {
                reportSections.Add("   • Review infrastructure settings for consistency");
            }
        }
        else
        {
            reportSections.Add("   • All validations passed - ready for deployment");
            if (result.Warnings.Any())
            {
                reportSections.Add("   • Review warnings for potential improvements");
            }
        }

        // Replace the ValidatedResources with the formatted report
        result.ValidatedResources.Clear();
        result.ValidatedResources.AddRange(reportSections);

        return result;
    }

    public async Task<PreDeploymentValidationResult> ValidateSpecificResourcesAsync(
        InfrastructureSettings settings,
        params string[] resourceTypes)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceTypes);

        _logger.LogInformation("🎯 Running validation for specific resources: {ResourceTypes} in environment: {Environment}",
            string.Join(", ", resourceTypes), settings.Environment);

        var result = new PreDeploymentValidationResult();
        var validationTasks = new List<Task<PreDeploymentValidationResult>>();

        try
        {
            foreach (var resourceType in resourceTypes)
            {
                // Run drift detection for specific resource if service is available
                if (_driftDetectionService != null)
                {
                    validationTasks.Add(_driftDetectionService.DetectResourceDriftAsync(resourceType, $"{resourceType}-resource", settings));
                }

                // Run Azure state validation for specific resource if service is available
                if (_azureStateValidator != null)
                {
                    // Note: This would need to be implemented in the Azure state validator
                    // For now, we'll run the full validation and filter results
                    validationTasks.Add(RunAzureStateValidationAsync(settings));
                }
            }

            if (resourceTypes.Length == 0)
            {
                result.Warnings.Add("No resources specified for validation");
            }
            else if (validationTasks.Any())
            {
                var validationResults = await Task.WhenAll(validationTasks);

                // Combine results and filter for specific resource types
                foreach (var validationResult in validationResults)
                {
                    // Filter results to only include the requested resource types
                    var filteredErrors = validationResult.Errors
                        .Where(error => resourceTypes.Any(rt => error.Contains(rt, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    var filteredWarnings = validationResult.Warnings
                        .Where(warning => resourceTypes.Any(rt => warning.Contains(rt, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    var filteredResources = validationResult.ValidatedResources
                        .Where(resource => resourceTypes.Any(rt => resource.Contains(rt, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    result.Errors.AddRange(filteredErrors);
                    result.Warnings.AddRange(filteredWarnings);
                    result.ValidatedResources.AddRange(filteredResources);
                }
            }
            else
            {
                result.Warnings.Add("No validation services available for specific resource validation");
            }

            result.IsValid = result.Errors.Count == 0;

            _logger.LogInformation("Specific resource validation completed for {ResourceTypes} with {ErrorCount} errors and {WarningCount} warnings",
                string.Join(", ", resourceTypes), result.Errors.Count, result.Warnings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Specific resource validation failed for {ResourceTypes}", string.Join(", ", resourceTypes));
            result.Errors.Add($"Specific resource validation failed: {ex.Message}");
        }

        return result;
    }

    public async Task<PreDeploymentValidationResult> RunQuickValidationAsync(InfrastructureSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _logger.LogInformation("⚡ Running quick validation for CI/CD pipeline - environment: {Environment}", settings.Environment);

        try
        {
            // Quick validation focuses on essential checks without Azure connectivity
            var result = new PreDeploymentValidationResult();

            // Run basic pre-deployment validation
            var preDeploymentResult = await _preDeploymentValidator.ValidateAllAsync(settings);
            result.Errors.AddRange(preDeploymentResult.Errors);
            result.Warnings.AddRange(preDeploymentResult.Warnings);
            result.ValidatedResources.AddRange(preDeploymentResult.ValidatedResources);

            // Run naming consistency validation if available
            if (_namingValidator != null)
            {
                var namingResult = _namingValidator.ValidateNamingConsistency(settings);
                result.Errors.AddRange(namingResult.Errors);
                result.Warnings.AddRange(namingResult.Warnings);
                result.ValidatedResources.AddRange(namingResult.ValidatedResources);
            }

            result.IsValid = result.Errors.Count == 0;

            // Add quick validation marker
            result.ValidatedResources.Add("⚡ Quick Validation Mode - Essential checks only");

            _logger.LogInformation("Quick validation completed with {ErrorCount} errors and {WarningCount} warnings",
                result.Errors.Count, result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quick validation failed");
            return new PreDeploymentValidationResult
            {
                Errors = { $"Quick validation failed: {ex.Message}" }
            };
        }
    }

    /// <summary>
    /// Converts a ValidationResult to PreDeploymentValidationResult
    /// </summary>
    private static PreDeploymentValidationResult ConvertToPreDeploymentValidationResult(ValidationResult validationResult)
    {
        var result = new PreDeploymentValidationResult
        {
            IsValid = validationResult.IsValid
        };

        result.Errors.AddRange(validationResult.Errors);
        result.Warnings.AddRange(validationResult.Warnings);

        if (validationResult.ValidatedResources != null)
        {
            result.ValidatedResources.AddRange(validationResult.ValidatedResources);
        }

        return result;
    }
}

