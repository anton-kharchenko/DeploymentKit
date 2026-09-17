using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Represents the types of resource groups for organizing Azure resources.
/// </summary>
public enum ResourceGroupType
{
    /// <summary>
    /// Application-specific resource group containing application resources like Container Apps, databases, and caches.
    /// </summary>
    [Description("application")]
    Application,

    /// <summary>
    /// Shared resource group containing shared infrastructure resources like monitoring, Key Vault, and storage.
    /// </summary>
    [Description("shared")]
    Shared,

    /// <summary>
    /// Network resource group containing networking resources like VNets, subnets, and gateways.
    /// </summary>
    [Description("network")]
    Network
}
