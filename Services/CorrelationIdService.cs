using DeploymentKit.Interfaces;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing correlation IDs across infrastructure operations using AsyncLocal for thread-safe context
/// </summary>
public class CorrelationIdService : ICorrelationIdService
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    /// <summary>
    /// Gets the current correlation ID from context, or generates a new one if none exists
    /// </summary>
    /// <returns>The current correlation ID</returns>
    public string GetOrGenerateCorrelationId()
    {
        var correlationId = _correlationId.Value;
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = GenerateCorrelationId();
            _correlationId.Value = correlationId;
        }
        return correlationId;
    }

    /// <summary>
    /// Sets the correlation ID for the current context
    /// </summary>
    /// <param name="correlationId">The correlation ID to set</param>
    public void SetCorrelationId(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            throw new ArgumentException("Correlation ID cannot be null or empty", nameof(correlationId));

        _correlationId.Value = correlationId;
    }

    /// <summary>
    /// Generates a new correlation ID using a GUID with a timestamp prefix for better traceability
    /// </summary>
    /// <returns>A new correlation ID</returns>
    public string GenerateCorrelationId()
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
        var guid = Guid.NewGuid().ToString("N")[..8]; // Take first 8 characters for brevity
        return $"{timestamp}-{guid}";
    }

    /// <summary>
    /// Gets the current correlation ID without generating a new one
    /// </summary>
    /// <returns>The current correlation ID, or null if none exists</returns>
    public string? GetCurrentCorrelationId() => _correlationId.Value;
}

