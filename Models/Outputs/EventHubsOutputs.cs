namespace DeploymentKit.Models.Outputs;

/// <summary>
/// Outputs from Event Hubs service
/// </summary>
public class EventHubsOutputs
{
    public Output<string> EventHubsNamespaceName { get; set; } = null!;
    public Output<string> EventHubsConnectionString { get; set; } = null!;
    public Output<string> EventHubsEndpoint { get; set; } = null!;
    public Output<string> EventHubsResourceId { get; set; } = null!;
    public string EventHubName { get; set; } = string.Empty;
    public string ConsumerGroupName { get; set; } = string.Empty;
}

