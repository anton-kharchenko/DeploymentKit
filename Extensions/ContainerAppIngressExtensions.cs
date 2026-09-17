using AppConfigurationArgs = Pulumi.AzureNative.App.Inputs.ConfigurationArgs;
using AppIngressArgs = Pulumi.AzureNative.App.Inputs.IngressArgs;
using AppTrafficWeightArgs = Pulumi.AzureNative.App.Inputs.TrafficWeightArgs;

namespace DeploymentKit.Extensions;

/// <summary>
/// Extensions for configuring Container App ingress settings
/// This enables external access to Container Apps deployed via Application SDK
/// </summary>
public static class ContainerAppIngressExtensions
{
    /// <summary>
    /// Adds external HTTPS ingress configuration to a Container App configuration
    /// </summary>
    /// <param name="configuration">The configuration to extend</param>
    /// <param name="targetPort">Target port the container listens on (default: 8080)</param>
    /// <param name="allowInsecure">Allow HTTP traffic (default: false)</param>
    /// <param name="transport">Transport protocol (default: "auto" for HTTP/1.1 and HTTP/2)</param>
    /// <returns>The configuration with ingress configured</returns>
    public static AppConfigurationArgs WithExternalIngress(
        this AppConfigurationArgs configuration,
        int targetPort = 8080,
        bool allowInsecure = false,
        string? transport = null)
    {
        configuration.Ingress = CreateExternalIngressArgs(targetPort, allowInsecure, transport);
        return configuration;
    }

    /// <summary>
    /// Adds internal ingress configuration to a Container App configuration
    /// </summary>
    /// <param name="configuration">The configuration to extend</param>
    /// <param name="targetPort">Target port the container listens on (default: 8080)</param>
    /// <param name="transport">Transport protocol (default: "auto" for HTTP/1.1 and HTTP/2)</param>
    /// <returns>The configuration with ingress configured</returns>
    public static AppConfigurationArgs WithInternalIngress(
        this AppConfigurationArgs configuration,
        int targetPort = 8080,
        string? transport = null)
    {
        configuration.Ingress = CreateInternalIngressArgs(targetPort, transport);
        return configuration;
    }

    /// <summary>
    /// Creates an external ingress configuration
    /// </summary>
    /// <param name="targetPort">Target port the container listens on</param>
    /// <param name="allowInsecure">Allow HTTP traffic (default: false)</param>
    /// <param name="transport">Transport protocol (default: "auto" for HTTP/1.1 and HTTP/2). Valid values: "auto", "http", "http2", "tcp"</param>
    /// <param name="trafficWeight">Traffic weight percentage (default: 100)</param>
    /// <returns>Configured IngressArgs for external access</returns>
    public static AppIngressArgs CreateExternalIngressArgs(
        int targetPort,
        bool allowInsecure = false,
        string? transport = null,
        int trafficWeight = 100)
    {
        return new AppIngressArgs
        {
            External = true,
            TargetPort = targetPort,
            Transport = transport ?? "auto",
            AllowInsecure = allowInsecure,
            Traffic = new InputList<AppTrafficWeightArgs>
            {
                new AppTrafficWeightArgs
                {
                    LatestRevision = true,
                    Weight = trafficWeight
                }
            }
        };
    }

    /// <summary>
    /// Creates an internal ingress configuration (only accessible within VNet)
    /// </summary>
    /// <param name="targetPort">Target port the container listens on</param>
    /// <param name="transport">Transport protocol (default: "auto" for HTTP/1.1 and HTTP/2). Valid values: "auto", "http", "http2", "tcp"</param>
    /// <param name="trafficWeight">Traffic weight percentage (default: 100)</param>
    /// <returns>Configured IngressArgs for internal access only</returns>
    public static AppIngressArgs CreateInternalIngressArgs(
        int targetPort,
        string? transport = null,
        int trafficWeight = 100)
    {
        return new AppIngressArgs
        {
            External = false,
            TargetPort = targetPort,
            Transport = transport ?? "auto",
            AllowInsecure = false, // Always enforce HTTPS for internal traffic
            Traffic = new InputList<AppTrafficWeightArgs>
            {
                new AppTrafficWeightArgs
                {
                    LatestRevision = true,
                    Weight = trafficWeight
                }
            }
        };
    }

    /// <summary>
    /// Creates ingress configuration with custom traffic splitting
    /// </summary>
    /// <param name="targetPort">Target port the container listens on</param>
    /// <param name="external">Enable external access (true) or internal only (false)</param>
    /// <param name="trafficWeights">Custom traffic weight configuration</param>
    /// <param name="allowInsecure">Allow HTTP traffic (default: false)</param>
    /// <param name="transport">Transport protocol (default: "auto"). Valid values: "auto", "http", "http2", "tcp"</param>
    /// <returns>Configured IngressArgs with custom traffic distribution</returns>
    public static AppIngressArgs CreateCustomTrafficIngressArgs(
        int targetPort,
        bool external,
        InputList<AppTrafficWeightArgs> trafficWeights,
        bool allowInsecure = false,
        string? transport = null)
    {
        return new AppIngressArgs
        {
            External = external,
            TargetPort = targetPort,
            Transport = transport ?? "auto",
            AllowInsecure = allowInsecure,
            Traffic = trafficWeights
        };
    }
}


