using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

/// <summary>
/// Service for managing DNS records (A, CNAME, TXT, CAA)
/// </summary>
public interface IDnsRecordService
{
    /// <summary>
    /// Creates DNS records for custom domain (A, CNAME, TXT, CAA based on settings)
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="resourceGroup">Resource group name</param>
    /// <param name="publicIpAddress">Public IP address for A record</param>
    /// <param name="dnsZoneName">DNS zone name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>DNS record outputs</returns>
    Task<DnsRecordOutputs> CreateDnsRecordsAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        Input<string> publicIpAddress,
        string dnsZoneName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates DNS propagation for a given domain
    /// </summary>
    /// <param name="domainName">Domain name to validate</param>
    /// <param name="expectedIpAddress">Expected IP address from A record</param>
    /// <param name="timeoutSeconds">Maximum wait time in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if DNS has propagated successfully</returns>
    Task<bool> ValidateDnsPropagationAsync(
        string domainName,
        string expectedIpAddress,
        int timeoutSeconds,
        CancellationToken cancellationToken = default);
}

