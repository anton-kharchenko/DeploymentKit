using DeploymentKit.Exceptions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using Pulumi.AzureNative.Dns;
using Pulumi.AzureNative.Dns.Inputs;
using System.Collections.Immutable;
using System.Net;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing DNS records (A, CNAME, TXT, CAA)
/// </summary>
public class DnsRecordService(
    ILogger<DnsRecordService> logger,
    ICorrelationIdService correlationIdService) : IDnsRecordService
{
    private readonly ILogger<DnsRecordService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));

    public async Task<DnsRecordOutputs> CreateDnsRecordsAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        Input<string> publicIpAddress,
        string dnsZoneName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(settings.CustomDomain);

        if (!settings.CustomDomain.Enabled || !settings.CustomDomain.CreateDnsRecords)
        {
            _logger.LogInformation("DNS record creation is disabled. Skipping.");
            return CreateEmptyOutputs(dnsZoneName, settings.CustomDomain.Name);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
            _logger.LogInformation("Creating DNS records for domain: {Domain} with CorrelationId: {CorrelationId}",
                settings.CustomDomain.Name, correlationId);

            var outputs = new DnsRecordOutputs
            {
                DnsZoneId = Output.Create(""),
                DnsZoneName = Output.Create(dnsZoneName),
                DomainName = Output.Create(settings.CustomDomain.Name),
                NameServers = Output.Create(ImmutableArray<string>.Empty)
            };

            // Create A record (domain -> IP)
            if (settings.CustomDomain.CreateARecord)
            {
                var aRecord = await CreateARecordAsync(settings, resourceGroup, publicIpAddress, dnsZoneName);
                outputs.ARecordId = aRecord.Id;
                _logger.LogInformation("A record created successfully for domain: {Domain}", settings.CustomDomain.Name);
            }

            // Create CNAME record for www subdomain
            if (settings.CustomDomain.CreateWwwCname)
            {
                var cnameRecord = await CreateCnameRecordAsync(settings, resourceGroup, dnsZoneName);
                outputs.CnameRecordId = cnameRecord.Id;
                _logger.LogInformation("CNAME record created successfully for www subdomain");
            }

            // Create CAA records (certificate authority authorization)
            if (settings.CustomDomain.CreateCaaRecords)
            {
                var caaRecord = await CreateCaaRecordAsync(settings, resourceGroup, dnsZoneName);
                outputs.CaaRecordId = caaRecord.Id;
                _logger.LogInformation("CAA records created successfully");
            }

            _logger.LogInformation("DNS records creation completed for domain: {Domain}", settings.CustomDomain.Name);

            return outputs;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DNS records for domain: {Domain}", settings.CustomDomain?.Name);
            throw new ResourceCreationException(
                $"Failed to create DNS records for domain '{settings.CustomDomain?.Name}'",
                ex,
                "DnsRecords",
                settings.Environment,
                _correlationIdService.GetOrGenerateCorrelationId(),
                "DNS_RECORD_CREATION_FAILED");
        }
    }

    private Task<RecordSet> CreateARecordAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        Input<string> publicIpAddress,
        string dnsZoneName)
    {
        var recordName = "@"; // @ represents the apex domain (e.g., example.com, not www.example.com)
        var recordSetName = $"{dnsZoneName}-a-record";

        _logger.LogInformation("Creating A record: {RecordName} in zone: {ZoneName}", recordName, dnsZoneName);

        var aRecord = new RecordSet(recordSetName, new RecordSetArgs
        {
            ResourceGroupName = resourceGroup,
            ZoneName = dnsZoneName,
            RelativeRecordSetName = recordName,
            RecordType = "A",
            Ttl = settings.CustomDomain!.DnsRecordTtl,
            ARecords = new[]
            {
                new ARecordArgs
                {
                    Ipv4Address = publicIpAddress
                }
            },
            Metadata = new InputMap<string>
            {
                ["ManagedBy"] = "DeploymentKit",
                ["Environment"] = settings.Environment,
                ["Domain"] = settings.CustomDomain.Name
            }
        });

        return Task.FromResult(aRecord);
    }

    private Task<RecordSet> CreateCnameRecordAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        string dnsZoneName)
    {
        var recordName = "www";
        var recordSetName = $"{dnsZoneName}-cname-www";

        _logger.LogInformation("Creating CNAME record: {RecordName} in zone: {ZoneName}", recordName, dnsZoneName);

        var cnameRecord = new RecordSet(recordSetName, new RecordSetArgs
        {
            ResourceGroupName = resourceGroup,
            ZoneName = dnsZoneName,
            RelativeRecordSetName = recordName,
            RecordType = "CNAME",
            Ttl = settings.CustomDomain!.DnsRecordTtl,
            CnameRecord = new CnameRecordArgs
            {
                Cname = settings.CustomDomain.Name // www.example.com -> example.com
            },
            Metadata = new InputMap<string>
            {
                ["ManagedBy"] = "DeploymentKit",
                ["Environment"] = settings.Environment,
                ["Domain"] = settings.CustomDomain.Name
            }
        });

        return Task.FromResult(cnameRecord);
    }

    private Task<RecordSet> CreateCaaRecordAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        string dnsZoneName)
    {
        var recordName = "@"; // CAA records apply to apex domain
        var recordSetName = $"{dnsZoneName}-caa-record";

        _logger.LogInformation("Creating CAA records in zone: {ZoneName}", dnsZoneName);

        // Create CAA records for each allowed certificate authority
        var caaRecords = settings.CustomDomain!.AllowedCertificateAuthorities.Select(ca => new CaaRecordArgs
        {
            Flags = 0, // 0 = non-critical, 128 = critical
            Tag = "issue", // "issue" allows CA to issue certs, "issuewild" for wildcards
            Value = ca
        }).ToArray();

        var caaRecordSet = new RecordSet(recordSetName, new RecordSetArgs
        {
            ResourceGroupName = resourceGroup,
            ZoneName = dnsZoneName,
            RelativeRecordSetName = recordName,
            RecordType = "CAA",
            Ttl = settings.CustomDomain.DnsRecordTtl,
            CaaRecords = caaRecords,
            Metadata = new InputMap<string>
            {
                ["ManagedBy"] = "DeploymentKit",
                ["Environment"] = settings.Environment,
                ["Domain"] = settings.CustomDomain.Name
            }
        });

        return Task.FromResult(caaRecordSet);
    }

    public async Task<bool> ValidateDnsPropagationAsync(
        string domainName,
        string expectedIpAddress,
        int timeoutSeconds,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating DNS propagation for domain: {Domain}, expected IP: {IpAddress}",
            domainName, expectedIpAddress);

        var startTime = DateTime.UtcNow;
        var retryCount = 0;
        var maxRetries = timeoutSeconds / 10; // Check every 10 seconds

        while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(timeoutSeconds))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(domainName, cancellationToken);
                var resolvedIp = hostEntry.AddressList.FirstOrDefault()?.ToString();

                if (resolvedIp == expectedIpAddress)
                {
                    _logger.LogInformation("DNS propagation validated successfully for domain: {Domain} -> {IpAddress}",
                        domainName, resolvedIp);
                    return true;
                }

                _logger.LogDebug("DNS not propagated yet. Expected: {Expected}, Got: {Actual}",
                    expectedIpAddress, resolvedIp);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("DNS lookup failed: {Message}", ex.Message);
            }

            retryCount++;
            if (retryCount < maxRetries)
            {
                await Task.Delay(10000, cancellationToken); // Wait 10 seconds before retry
            }
            else
            {
                break;
            }
        }

        _logger.LogWarning("DNS propagation validation timed out after {Seconds} seconds for domain: {Domain}",
            timeoutSeconds, domainName);
        return false;
    }

    private static DnsRecordOutputs CreateEmptyOutputs(string dnsZoneName, string domainName)
    {
        return new DnsRecordOutputs
        {
            DnsZoneId = Output.Create(""),
            DnsZoneName = Output.Create(dnsZoneName),
            DomainName = Output.Create(domainName),
            NameServers = Output.Create(ImmutableArray<string>.Empty)
        };
    }
}

