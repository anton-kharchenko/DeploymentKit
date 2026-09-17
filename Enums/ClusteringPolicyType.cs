using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the clustering policy types for Azure Managed Redis
/// </summary>
public enum ClusteringPolicyType
{
    /// <summary>
    /// OSS Cluster - Open Source Redis clustering policy (default)
    /// </summary>
    [Description("OSSCluster")]
    OSSCluster,

    /// <summary>
    /// Enterprise Cluster - Redis Enterprise clustering policy
    /// </summary>
    [Description("EnterpriseCluster")]
    EnterpriseCluster
}

