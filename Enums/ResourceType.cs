using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Represents the types of Azure resources in the infrastructure.
/// </summary>
public enum ResourceType
{
    /// <summary>
    /// Container App resource.
    /// </summary>
    [Description("ContainerApp")]
    ContainerApp,

    /// <summary>
    /// Database resource.
    /// </summary>
    [Description("Database")]
    Database,

    /// <summary>
    /// Cache resource.
    /// </summary>
    [Description("Cache")]
    Cache,

    /// <summary>
    /// Network resource.
    /// </summary>
    [Description("Network")]
    Network,

    /// <summary>
    /// Storage resource.
    /// </summary>
    [Description("Storage")]
    Storage,

    /// <summary>
    /// Container Registry resource.
    /// </summary>
    [Description("ContainerRegistry")]
    ContainerRegistry,

    /// <summary>
    /// Key Vault resource.
    /// </summary>
    [Description("KeyVault")]
    KeyVault,

    /// <summary>
    /// Monitoring resource.
    /// </summary>
    [Description("Monitoring")]
    Monitoring,

    /// <summary>
    /// Event Hubs resource.
    /// </summary>
    [Description("EventHubs")]
    EventHubs,

    /// <summary>
    /// Application Gateway resource.
    /// </summary>
    [Description("ApplicationGateway")]
    ApplicationGateway
}

