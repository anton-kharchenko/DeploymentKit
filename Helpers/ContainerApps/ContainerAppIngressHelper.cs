using DeploymentKit.Extensions;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Helpers.ContainerApps;

/// <summary>
/// Helper class for creating container app ingress configurations.
/// </summary>
public static class ContainerAppIngressHelper
{
    /// <summary>
    /// Creates the ingress configuration for a container app.
    /// </summary>
    /// <param name="ingressSettings">The ingress settings.</param>
    /// <param name="logger">The logger instance.</param>
    /// <returns>The ingress arguments.</returns>
    public static IngressArgs CreateIngressConfiguration(
        Settings.IngressSettings? ingressSettings,
        ILogger logger)
    {
        // If no ingress settings provided, default to internal-only for backward compatibility
        if (ingressSettings == null)
        {
            return ContainerAppIngressExtensions.CreateInternalIngressArgs(
                Constants.ServiceConstants.ContainerDefaults.DefaultTargetPort,
                Constants.ServiceConstants.ContainerDefaults.DefaultTransport
            );
        }

        // Force AllowInsecure to false as per security requirements ("Secure by Default")
        if (ingressSettings.AllowInsecure)
        {
            logger.LogWarning("Ingress AllowInsecure setting is set to true but will be enforced to false for security hardening.");
        }

        // Use extension method to create ingress with custom settings, enforcing AllowInsecure = false
        var ingress = ingressSettings.External
            ? ContainerAppIngressExtensions.CreateExternalIngressArgs(
                ingressSettings.TargetPort,
                false, // Enforce AllowInsecure = false
                ingressSettings.Transport
            )
            : ContainerAppIngressExtensions.CreateInternalIngressArgs(
                ingressSettings.TargetPort,
                ingressSettings.Transport
            );

        // Add IP security restrictions if provided
        if (ingressSettings.IpSecurityRestrictions is { Count: > 0 })
        {
            ingress.IpSecurityRestrictions = new InputList<IpSecurityRestrictionRuleArgs>();
            foreach (var restriction in ingressSettings.IpSecurityRestrictions)
            {
                ingress.IpSecurityRestrictions.Add(new IpSecurityRestrictionRuleArgs
                {
                    Name = restriction.Name,
                    IpAddressRange = restriction.IpAddressRange,
                    Action = restriction.Action,
                    Description = restriction.Description ?? throw new InvalidOperationException("Description cannot be null.")
                });
            }
            logger.LogInformation("Configured {Count} IP security restrictions for ingress", ingressSettings.IpSecurityRestrictions.Count);
        }

        // Note: Custom domains require post-deployment binding via Azure Container Apps Managed Certificates
        // They cannot be configured during initial container app creation and will be handled separately
        if (ingressSettings.CustomDomains is { Count: > 0 })
        {
            logger.LogWarning("Custom domains detected: {Domains}. Custom domain binding with managed certificates must be configured after initial deployment. " +
                "Consider using Azure CLI or Portal for custom domain and SSL certificate setup.",
                string.Join(", ", ingressSettings.CustomDomains.Select(d => d.Name)));
        }

        return ingress;
    }
}



