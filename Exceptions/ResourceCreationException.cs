namespace DeploymentKit.Exceptions;

/// <summary>
/// Exception thrown when Azure resource creation fails
/// </summary>
[Serializable]
public class ResourceCreationException : InfrastructureException
{
    public string? CorrelationId { get; }
    public string? ErrorCode { get; }

    public ResourceCreationException()
    {
    }

    public ResourceCreationException(string message) : base(message)
    {
    }

    public ResourceCreationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ResourceCreationException(string message, string? resourceName, string? resourceType, string? environment, string? correlationId = null, string? errorCode = null)
        : base(message, resourceName, resourceType, environment)
    {
        CorrelationId = correlationId;
        ErrorCode = errorCode;
    }

    public ResourceCreationException(string message, Exception innerException, string? resourceName, string? resourceType, string? environment, string? correlationId = null, string? errorCode = null)
        : base(message, innerException, resourceName, resourceType, environment)
    {
        CorrelationId = correlationId;
        ErrorCode = errorCode;
    }
}




