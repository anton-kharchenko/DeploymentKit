namespace DeploymentKit.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a resource with a name that already exists
/// </summary>
[Serializable]
public class DuplicateResourceNameException : InfrastructureException
{
    public string? ExistingResourceName { get; }
    public string? ConflictingResourceName { get; }
    public string? CorrelationId { get; }

    public DuplicateResourceNameException()
    {
    }

    public DuplicateResourceNameException(string message) : base(message)
    {
    }

    public DuplicateResourceNameException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DuplicateResourceNameException(string message, string? resourceName, string? resourceType, string? environment, 
        string? existingResourceName = null, string? conflictingResourceName = null, string? correlationId = null)
        : base(message, resourceName, resourceType, environment)
    {
        ExistingResourceName = existingResourceName;
        ConflictingResourceName = conflictingResourceName;
        CorrelationId = correlationId;
    }

    public DuplicateResourceNameException(string message, Exception innerException, string? resourceName, string? resourceType, string? environment,
        string? existingResourceName = null, string? conflictingResourceName = null, string? correlationId = null)
        : base(message, innerException, resourceName, resourceType, environment)
    {
        ExistingResourceName = existingResourceName;
        ConflictingResourceName = conflictingResourceName;
        CorrelationId = correlationId;
    }
}



