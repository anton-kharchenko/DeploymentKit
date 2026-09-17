using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Utilities;

/// <summary>
/// A validation result that can contain multiple child validation results
/// </summary>
public class CompositeValidationResult : ValidationResult
{
    private readonly List<ValidationResult> _results = [];

    /// <summary>
    /// Initializes a new instance of the CompositeValidationResult class
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    public CompositeValidationResult(string errorMessage) : base(errorMessage) { }

    /// <summary>
    /// Initializes a new instance of the CompositeValidationResult class
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <param name="memberNames">The member names</param>
    public CompositeValidationResult(string errorMessage, IEnumerable<string> memberNames) : base(errorMessage, memberNames) { }

    /// <summary>
    /// Adds a validation result to the composite result
    /// </summary>
    /// <param name="validationResult">The validation result to add</param>
    public void AddResult(ValidationResult validationResult) => _results.Add(validationResult);
}

