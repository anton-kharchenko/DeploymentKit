namespace DeploymentKit.Models.Results;

/// <summary>
/// Result of a validation operation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    
    /// <summary>
    /// List of validated resources
    /// </summary>
    public List<string> ValidatedResources { get; set; } = new();

    /// <summary>
    /// Adds an error message to the validation result
    /// </summary>
    /// <param name="error">The error message to add</param>
    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    /// <summary>
    /// Adds a warning message to the validation result
    /// </summary>
    /// <param name="warning">The warning message to add</param>
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    /// <summary>
    /// Adds a recommendation message to the validation result
    /// </summary>
    /// <param name="recommendation">The recommendation message to add</param>
    public void AddRecommendation(string recommendation)
    {
        Recommendations.Add(recommendation);
    }
}

