using DeploymentKit.Constants;
using DeploymentKit.Exceptions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Network.Inputs;
using PublicIPAddressArgs = Pulumi.AzureNative.Network.PublicIPAddressArgs;
using SubnetArgs = Pulumi.AzureNative.Network.SubnetArgs;
using VirtualNetworkGatewayArgs = Pulumi.AzureNative.Network.VirtualNetworkGatewayArgs;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing VPN Gateway infrastructure with secure Point-to-Site connectivity
/// </summary>
public class VpnService(ILogger<VpnService> logger, IResourceNamingService namingService, ICorrelationIdService correlationIdService) : IVpnService
{
    private readonly ILogger<VpnService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IResourceNamingService _namingService =
        namingService ?? throw new ArgumentNullException(nameof(namingService));

    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));

    public async Task<VpnOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup,
        NetworkOutputs networkOutputs, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation(ServiceConstants.VpnGateway.CreationStartMessage,
                settings.Environment);

            if (settings.Network is { EnableVpnGateway: false })
            {
                _logger.LogInformation(ServiceConstants.VpnGateway.DisabledMessage, settings.Environment);
                return new VpnOutputs();
            }

            // Create VPN Gateway subnet
            var vpnGatewaySubnet = await CreateVpnGatewaySubnetAsync(settings, resourceGroup, networkOutputs.VirtualNetworkName);

            // Create Public IP for VPN Gateway
            var vpnGatewayPublicIp = await CreateVpnGatewayPublicIpAsync(settings, resourceGroup);

            // Create VPN Gateway
            var vpnGateway = await CreateVpnGatewayAsync(settings, resourceGroup, vpnGatewaySubnet, vpnGatewayPublicIp);

            // Configure Point-to-Site VPN
            var p2SVpnConfiguration = await ConfigurePointToSiteVpnAsync(settings);

            _logger.LogInformation(ServiceConstants.VpnGateway.CreationSuccessMessage, settings.Environment);

            return new VpnOutputs
            {
                VpnGatewayId = vpnGateway.Id,
                VpnGatewayName = vpnGateway.Name,
                VpnGatewayPublicIp = vpnGatewayPublicIp.IpAddress.Apply(ip => ip ?? ""),
                VpnGatewaySubnetId = vpnGatewaySubnet.Id,
                P2SVpnConfigurationName = Output.Create(p2SVpnConfiguration),
                VpnClientAddressPool = settings.Network?.VpnClientAddressPool ?? throw new InvalidOperationException(),
                VpnAuthenticationType = settings.Network.VpnAuthenticationType,
                VpnTunnelType = settings.Network.VpnTunnelType,
                RootCertificateName = ServiceConstants.VpnGateway.RootCertificateName,
                RootCertificateData = ServiceConstants.VpnGateway.PlaceholderCertificateData
            };
        }
        catch (Exception ex)
        {
            var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
            _logger.LogError(ex, ServiceConstants.VpnGateway.CreationFailedMessage,
                settings.Environment);
            throw new ResourceCreationException(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.VpnGateway.CreationFailedMessage, settings.Environment),
                ex,
                ServiceConstants.ResourceTypes.VPN,
                ServiceConstants.ResourceTypes.VpnGateway,
                settings.Environment,
                correlationId,
                ServiceConstants.ErrorCodes.VpnGatewayCreationFailed);
        }
    }

    public async Task<string> GenerateClientConfigurationAsync(InfrastructureSettings settings, VpnOutputs vpnOutputs, CancellationToken cancellationToken = default)
    {
        return await GenerateClientConfigurationInternalAsync(settings, vpnOutputs);
    }

    private Task<string> GenerateClientConfigurationInternalAsync(InfrastructureSettings settings, VpnOutputs vpnOutputs)
    {
        try
        {
            _logger.LogInformation(ServiceConstants.VpnGateway.ClientConfigGenerationMessage,
                settings.Environment);

            // Since we can't directly await Output<string> in string interpolation,
            // we'll create a placeholder configuration
            var clientConfig = $"""

                                {string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.VpnGateway.ClientConfigurationTemplate.Header, settings.Environment.ToUpper())}
                                {ServiceConstants.VpnGateway.ClientConfigurationTemplate.Separator}

                                {ServiceConstants.VpnGateway.ClientConfigurationTemplate.GatewayLabel}
                                {string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.VpnGateway.ClientConfigurationTemplate.TunnelTypeLabel, vpnOutputs.VpnTunnelType)}
                                {string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.VpnGateway.ClientConfigurationTemplate.AuthenticationLabel, vpnOutputs.VpnAuthenticationType)}
                                {string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.VpnGateway.ClientConfigurationTemplate.ClientAddressPoolLabel, vpnOutputs.VpnClientAddressPool)}

                                {ServiceConstants.VpnGateway.ClientConfigurationTemplate.ConnectionInstructions}

                                {ServiceConstants.VpnGateway.ClientConfigurationTemplate.SecurityFeatures}

                                """;

            return Task.FromResult(clientConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ServiceConstants.VpnGateway.ClientConfigGenerationFailedMessage,
                settings.Environment);
            throw;
        }
    }

    private Task<Subnet> CreateVpnGatewaySubnetAsync(InfrastructureSettings settings, Input<string> resourceGroup, Output<string> vnetName)
    {
        const string subnetName = ServiceConstants.VpnGateway.GatewaySubnetName; // Required name for VPN Gateway subnet

        _logger.LogInformation(ServiceConstants.VpnGateway.SubnetCreationMessage, subnetName);

        var subnet = new Subnet(subnetName,
            new SubnetArgs
            {
                ResourceGroupName = resourceGroup,
                VirtualNetworkName = vnetName,
                AddressPrefix = settings.Network.VpnGatewaySubnet,
                SubnetName = subnetName
            });

        return Task.FromResult(subnet);
    }

    private Task<PublicIPAddress> CreateVpnGatewayPublicIpAsync(InfrastructureSettings settings,
        Input<string> resourceGroup)
    {
        var publicIpName = _namingService.GeneratePublicIpName(settings.NamingPrefix, "vpngw", settings.Environment);

        _logger.LogInformation(ServiceConstants.VpnGateway.PublicIpCreationMessage, publicIpName);

        var publicIp = new PublicIPAddress(publicIpName,
            new PublicIPAddressArgs
            {
                ResourceGroupName = resourceGroup,
                Location = settings.Location,
                PublicIPAllocationMethod = IPAllocationMethod.Static,
                Sku = new PublicIPAddressSkuArgs
                {
                    Name = PublicIPAddressSkuName.Standard, Tier = PublicIPAddressSkuTier.Regional
                },
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, ServiceConstants.VpnGateway.VpnGatewayPublicIpResourceType)
            });

        return Task.FromResult(publicIp);
    }

    private Task<VirtualNetworkGateway> CreateVpnGatewayAsync(InfrastructureSettings settings,
        Input<string> resourceGroup, Subnet vpnGatewaySubnet, PublicIPAddress publicIp)
    {
        var vpnGatewayName = _namingService.GenerateVpnGatewayName(settings.NamingPrefix, settings.Environment);

        _logger.LogInformation(ServiceConstants.VpnGateway.GatewayCreationMessage, vpnGatewayName);

        var vpnGateway = new VirtualNetworkGateway(vpnGatewayName, new VirtualNetworkGatewayArgs
        {
            ResourceGroupName = resourceGroup,
            Location = settings.Location,
            GatewayType = ServiceConstants.VpnGateway.GatewayType,
            VpnType = ServiceConstants.VpnGateway.VpnType,
            Sku = new VirtualNetworkGatewaySkuArgs
            {
                Name = ServiceConstants.VpnGateway.SkuName,
                Tier = ServiceConstants.VpnGateway.SkuTier
            },
            IpConfigurations = new[]
            {
                new VirtualNetworkGatewayIPConfigurationArgs
                {
                    Name = ServiceConstants.VpnGateway.DefaultIpConfigurationName,
                    PrivateIPAllocationMethod = ServiceConstants.VpnGateway.PrivateIpAllocationMethod,
                    PublicIPAddress = new SubResourceArgs { Id = publicIp.Id },
                    Subnet = new SubResourceArgs { Id = vpnGatewaySubnet.Id }
                }
            },
            VpnClientConfiguration = new VpnClientConfigurationArgs
            {
                VpnClientAddressPool = new AddressSpaceArgs
                {
                    AddressPrefixes = new[] { settings.Network.VpnClientAddressPool }
                },
                VpnClientProtocols = new InputList<Union<string, VpnClientProtocol>> { VpnClientProtocol.OpenVPN, VpnClientProtocol.IkeV2 },
                VpnAuthenticationTypes = new InputList<Union<string, VpnAuthenticationType>> { VpnAuthenticationType.Certificate, VpnAuthenticationType.AAD },
                VpnClientRootCertificates = new[]
                {
                    new VpnClientRootCertificateArgs
                    {
                        Name = ServiceConstants.VpnGateway.RootCertificateName,
                        PublicCertData = ServiceConstants.VpnGateway.PlaceholderCertificateData
                    }
                },
                AadTenant = ServiceConstants.VpnGateway.AadTenantUrl,
                AadAudience = ServiceConstants.VpnGateway.AzureVpnClientAppId,
                AadIssuer = ServiceConstants.VpnGateway.AadIssuerUrl
            },
            Tags = ResourceTagHelper.GetStandardTags(settings.Environment, ServiceConstants.VpnGateway.VpnGatewayResourceType)
        }, new CustomResourceOptions { DependsOn = { publicIp, vpnGatewaySubnet } });

        return Task.FromResult(vpnGateway);
    }

    private Task<string> ConfigurePointToSiteVpnAsync(InfrastructureSettings settings)
    {
        _logger.LogInformation(ServiceConstants.VpnGateway.P2SConfigurationMessage, settings.Environment);

        // The P2S configuration is already included in the VPN Gateway creation
        // This method can be extended for additional P2S-specific configurations
        var configurationName = string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.VpnGateway.P2SEnvironmentFormat, settings.Environment);

        return Task.FromResult(configurationName);
    }
}


