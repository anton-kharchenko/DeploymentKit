namespace DeploymentKit.Exceptions;

/// <summary>
/// Exception thrown when Pulumi state drift is detected with Azure resources
/// </summary>
[Serializable]
public class StateDriftException : InfrastructureException
{
    /// <summary>
    /// The Azure error code that indicates the drift
    /// </summary>
    public string? AzureErrorCode { get; }

    /// <summary>
    /// The correlation ID for tracking the deployment attempt
    /// </summary>
    public string? CorrelationId { get; }

    /// <summary>
    /// Indicates whether automatic recovery was attempted
    /// </summary>
    public bool RecoveryAttempted { get; }

    /// <summary>
    /// Indicates whether the recovery was successful
    /// </summary>
    public bool RecoverySuccessful { get; }

    public StateDriftException()
    {
    }

    public StateDriftException(string message) : base(message)
    {
    }

    public StateDriftException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public StateDriftException(
        string message,
        string? azureErrorCode,
        string? correlationId,
        bool recoveryAttempted = false,
        bool recoverySuccessful = false)
        : base(message)
    {
        AzureErrorCode = azureErrorCode;
        CorrelationId = correlationId;
        RecoveryAttempted = recoveryAttempted;
        RecoverySuccessful = recoverySuccessful;
    }

    public StateDriftException(
        string message,
        Exception innerException,
        string? azureErrorCode,
        string? correlationId,
        bool recoveryAttempted = false,
        bool recoverySuccessful = false)
        : base(message, innerException)
    {
        AzureErrorCode = azureErrorCode;
        CorrelationId = correlationId;
        RecoveryAttempted = recoveryAttempted;
        RecoverySuccessful = recoverySuccessful;
    }
}



