using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available consistency levels for Azure Cosmos DB
/// </summary>
public enum CosmosDbConsistencyLevelType
{
    /// <summary>
    /// Strong consistency - Linearizability guarantee
    /// </summary>
    [Description("Strong")]
    Strong,

    /// <summary>
    /// Bounded staleness - Consistent prefix with lag bounds
    /// </summary>
    [Description("BoundedStaleness")]
    BoundedStaleness,

    /// <summary>
    /// Session consistency - Consistent prefix with monotonic reads and writes
    /// </summary>
    [Description("Session")]
    Session,

    /// <summary>
    /// Consistent prefix - Updates appear in order
    /// </summary>
    [Description("ConsistentPrefix")]
    ConsistentPrefix,

    /// <summary>
    /// Eventual consistency - No ordering guarantee
    /// </summary>
    [Description("Eventual")]
    Eventual
}
