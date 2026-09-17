namespace DeploymentKit.Exceptions;

/// <summary>
/// Base exception for all infrastructure-related errors
/// </summary>
[Serializable]
public class InfrastructureException : Exception
{
    public string? ResourceName { get; }
    public string? ResourceType { get; }
    public string? Environment { get; }

    public InfrastructureException()
    {
    }

    public InfrastructureException(string message) : base(message)
    {
    }

    public InfrastructureException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public InfrastructureException(string message, string? resourceName, string? resourceType, string? environment) 
        : base(message)
    {
        ResourceName = resourceName;
        ResourceType = resourceType;
        Environment = environment;
    }

    public InfrastructureException(string message, Exception innerException, string? resourceName, string? resourceType, string? environment) 
        : base(message, innerException)
    {
        ResourceName = resourceName;
        ResourceType = resourceType;
        Environment = environment;
    }
}



