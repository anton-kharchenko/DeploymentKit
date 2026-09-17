namespace DeploymentKit.Interfaces;

/// <summary>
/// Service for managing correlation IDs across infrastructure operations for distributed tracing
/// </summary>
public interface ICorrelationIdService
{
    /// <summary>
    /// Gets the current correlation ID from context, or generates a new one if none exists
    /// </summary>
    /// <returns>The current correlation ID</returns>
    string GetOrGenerateCorrelationId();

    /// <summary>
    /// Sets the correlation ID for the current context
    /// </summary>
    /// <param name="correlationId">The correlation ID to set</param>
    void SetCorrelationId(string correlationId);

    /// <summary>
    /// Generates a new correlation ID
    /// </summary>
    /// <returns>A new correlation ID</returns>
    string GenerateCorrelationId();

    /// <summary>
    /// Gets the current correlation ID without generating a new one
    /// </summary>
    /// <returns>The current correlation ID, or null if none exists</returns>
    string? GetCurrentCorrelationId();
}
