using DeploymentKit.Constants;
using DeploymentKit.Exceptions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.Cdn;
using Pulumi.AzureNative.Cdn.Inputs;
using Pulumi.AzureNative.Dns;
using Pulumi.AzureNative.Dns.Inputs;
using CdnProfile = Pulumi.AzureNative.Cdn.Profile;
using CdnEndpoint = Pulumi.AzureNative.Cdn.Endpoint;
using CdnProfileArgs = Pulumi.AzureNative.Cdn.ProfileArgs;
using CdnEndpointArgs = Pulumi.AzureNative.Cdn.EndpointArgs;
using CdnSkuArgs = Pulumi.AzureNative.Cdn.Inputs.SkuArgs;
using DnsZone = Pulumi.AzureNative.Dns.Zone;
using DnsZoneArgs = Pulumi.AzureNative.Dns.ZoneArgs;

namespace DeploymentKit.Services;

/// <summary>
/// Service for optimizing domain configuration and performance with DNS and SSL automation
/// </summary>
public class DomainOptimizationService(
    ILogger<DomainOptimizationService> logger,
    ICorrelationIdService correlationIdService,
    IDnsRecordService dnsRecordService) : IDomainOptimizationService
{
    private readonly ILogger<DomainOptimizationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
    private readonly IDnsRecordService _dnsRecordService = dnsRecordService ?? throw new ArgumentNullException(nameof(dnsRecordService));

    public async Task<DomainOptimizationOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, ApplicationGatewayOutputs applicationGateway, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(applicationGateway);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
            _logger.LogInformation("Creating domain optimization infrastructure for environment: {Environment} with CorrelationId: {CorrelationId}",
                settings.Environment, correlationId);

            CdnProfile? cdnProfile = null;
            CdnEndpoint? cdnEndpoint = null;

            if (settings.Network?.EnableCdn == true)
            {
                try
                {
                    _logger.LogInformation("Attempting to create CDN Profile for environment: {Environment}", settings.Environment);
                    cdnProfile = await CreateCdnProfileAsync(settings, resourceGroup);
                    cdnEndpoint = await CreateCdnEndpointAsync(settings, resourceGroup, cdnProfile, applicationGateway);
                    _logger.LogInformation("CDN Profile and Endpoint created successfully for environment: {Environment}", settings.Environment);
                }
                catch (Exception cdnEx)
                {
                    _logger.LogWarning(cdnEx, "CDN creation failed (Azure may have retired this SKU). Continuing without CDN for environment: {Environment}. Using DNS and Application Gateway instead.", settings.Environment);
                }
            }
            else
            {
                _logger.LogInformation("CDN provisioning is disabled for environment: {Environment}. Skipping CDN Profile creation.", settings.Environment);
            }

            // Create DNS Zone for custom domain management
            var dnsZone = await CreateDnsZoneAsync(settings, resourceGroup);

            // Create DNS records (A, CNAME, CAA) if custom domain is enabled
            DnsRecordOutputs? dnsRecords = null;
            if (settings.CustomDomain?.Enabled == true)
            {
                _logger.LogInformation("Creating DNS records for custom domain: {Domain}", settings.CustomDomain.Name);
                dnsRecords = await _dnsRecordService.CreateDnsRecordsAsync(
                    settings,
                    resourceGroup,
                    applicationGateway.PublicIpAddress,
                    settings.CustomDomain.Name,
                    cancellationToken);

                // Note: DNS propagation validation is commented out because Pulumi Output<T> cannot be awaited directly
                // This validation would need to be performed post-deployment
                if (settings.CustomDomain.WaitForDnsPropagation && settings.CustomDomain.CreateDnsRecords)
                {
                    _logger.LogInformation("DNS propagation validation will be performed after deployment completes");
                }
            }

            _logger.LogInformation("Successfully created domain optimization infrastructure for environment: {Environment}", settings.Environment);

            return new DomainOptimizationOutputs
            {
                CdnProfileId = cdnProfile?.Id ?? Output.Create(""),
                CdnEndpointId = cdnEndpoint?.Id ?? Output.Create(""),
                CdnEndpointHostname = cdnEndpoint?.HostName ?? Output.Create(""),
                TrafficManagerProfileId = Output.Create(""),
                TrafficManagerDnsName = Output.Create(""),
                DnsZoneId = dnsZone.Id,
                NameServers = dnsZone.NameServers,
                OptimizedDomainUrl = Output.Format($"https://{settings.CustomDomain?.Name ?? settings.Network?.CustomDomain}")
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create domain optimization infrastructure for environment: {Environment}", settings.Environment);
            throw new ResourceCreationException(
                $"Failed to create domain optimization infrastructure for environment '{settings.Environment}'",
                ex,
                ServiceConstants.ResourceTypes.DomainOptimization,
                ServiceConstants.ResourceTypes.CdnDns,
                settings.Environment,
                _correlationIdService.GetOrGenerateCorrelationId(),
                ServiceConstants.ErrorCodes.DomainOptimizationCreationFailed);
        }
    }

    private Task<CdnProfile> CreateCdnProfileAsync(InfrastructureSettings settings, Input<string> resourceGroup)
    {
        var cdnProfileName = $"{settings.NamingPrefix}-cdn-{settings.Environment}";

        _logger.LogInformation("Creating CDN Profile: {CdnProfileName}", cdnProfileName);

        var cdnProfile = new CdnProfile(cdnProfileName, new CdnProfileArgs
        {
            ResourceGroupName = resourceGroup,
            Location = "Global",
            Sku = new CdnSkuArgs
            {
                Name = SkuName.Standard_Verizon
            },
            Tags = ResourceTagHelper.GetStandardTags(settings.Environment, "cdn-profile")
        });

        return Task.FromResult(cdnProfile);
    }

    private Task<CdnEndpoint> CreateCdnEndpointAsync(InfrastructureSettings settings, Input<string> resourceGroup, CdnProfile cdnProfile, ApplicationGatewayOutputs applicationGateway)
    {
        var cdnEndpointName = $"{settings.NamingPrefix}-cdn-endpoint-{settings.Environment}";

        _logger.LogInformation("Creating CDN Endpoint: {CdnEndpointName}", cdnEndpointName);

        var cdnEndpoint = new CdnEndpoint(cdnEndpointName, new CdnEndpointArgs
        {
            ResourceGroupName = resourceGroup,
            ProfileName = cdnProfile.Name,
            Location = "Global",
            Origins = new[]
            {
                new DeepCreatedOriginArgs
                {
                    Name = "applicationGatewayOrigin",
                    HostName = applicationGateway.PublicIpAddress,
                    HttpPort = 80,
                    HttpsPort = 443,
                    Priority = 1,
                    Weight = 1000,
                    Enabled = true
                }
            },
            OriginHostHeader = settings.Network?.CustomDomain ?? throw new InvalidOperationException(),
            IsHttpAllowed = false, // Force HTTPS only
            IsHttpsAllowed = true,
            QueryStringCachingBehavior = QueryStringCachingBehavior.IgnoreQueryString,
            OptimizationType = "GeneralWebDelivery",
            Tags = ResourceTagHelper.GetStandardTags(settings.Environment, "cdn-endpoint")
        });

        return Task.FromResult(cdnEndpoint);
    }

    private Task<DnsZone> CreateDnsZoneAsync(InfrastructureSettings settings, Input<string> resourceGroup)
    {
        if (string.IsNullOrEmpty(settings.Network?.CustomDomain))
        {
            throw new InvalidOperationException("Custom domain must be specified for DNS zone creation");
        }

        var dnsZoneName = settings.Network.CustomDomain;

        _logger.LogInformation("Creating DNS Zone: {DnsZoneName}", dnsZoneName);

        var dnsZone = new DnsZone(dnsZoneName.Replace(".", "-"), new DnsZoneArgs
        {
            ResourceGroupName = resourceGroup,
            Location = "Global",
            ZoneName = dnsZoneName,
            ZoneType = ZoneType.Public,
            Tags = ResourceTagHelper.GetStandardTags(settings.Environment, "dns-zone")
        });

        return Task.FromResult(dnsZone);
    }

    /// <summary>
    /// Creates domain optimization infrastructure with default Application Gateway configuration
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="resourceGroup">Resource group name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Domain optimization outputs as object</returns>
    async Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _logger.LogWarning("Creating DomainOptimization with default Application Gateway configuration. Consider using the full CreateAsync overload for production scenarios.");

        // Create default Application Gateway outputs for domain optimization
        var defaultApplicationGateway = new ApplicationGatewayOutputs
        {
            ApplicationGatewayId = Output.Create("default-appgw-id"),
            ApplicationGatewayName = Output.Create("default-appgw"),
            PublicIpAddress = Output.Create("default-public-ip"),
            FrontendUrl = Output.Create($"https://{settings.NamingPrefix}-appgw-{settings.Environment}.{settings.Location}.cloudapp.azure.com")
        };

        var result = await CreateAsync(settings, resourceGroup, defaultApplicationGateway, cancellationToken);
        return result;
    }

    /// <summary>
    /// Creates domain optimization infrastructure with default Application Gateway configuration (synchronous version)
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="resourceGroup">Resource group name</param>
    /// <returns>Domain optimization outputs as object</returns>
    Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup)
    {
        return ((IInfrastructureService)this).CreateAsync(settings, resourceGroup, CancellationToken.None);
    }
}

