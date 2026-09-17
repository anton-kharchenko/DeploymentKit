using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Exceptions;
using DeploymentKit.Extensions;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Network.Inputs;
using PublicIPAddressArgs = Pulumi.AzureNative.Network.PublicIPAddressArgs;

namespace DeploymentKit.Services;

/// <summary>
/// Service for managing Application Gateway for secure external access to private Container Apps
/// </summary>
public class ApplicationGatewayService(ILogger<ApplicationGatewayService> logger, IResourceNamingService namingService, ICorrelationIdService correlationIdService) : IApplicationGatewayService
{
    private readonly ILogger<ApplicationGatewayService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IResourceNamingService _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));

    public async Task<ApplicationGatewayOutputs> CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, NetworkOutputs network, ContainerAppsOutputs containerApps, CertificateOutputs? certificate = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        if (string.IsNullOrEmpty(settings.NamingPrefix))
            throw new ArgumentException("NamingPrefix cannot be null or empty");

        if (string.IsNullOrEmpty(settings.Environment))
            throw new ArgumentException("Environment cannot be null or empty");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation(ServiceConstants.ApplicationGateway.CreationStartMessage, settings.Environment);

            // Create Public IP for Application Gateway
            var publicIp = await CreatePublicIpAsync(settings, resourceGroup);

            // Create Application Gateway
            var appGateway = await CreateApplicationGatewayAsync(settings, resourceGroup, network, containerApps, publicIp, certificate);

            _logger.LogInformation(ServiceConstants.ApplicationGateway.CreationSuccessMessage, settings.Environment);

            return new ApplicationGatewayOutputs
            {
                ApplicationGatewayId = appGateway.Id,
                ApplicationGatewayName = appGateway.Name,
                PublicIpAddress = publicIp.IpAddress!,
                FrontendUrl = Output.Format($"{DeploymentConstants.Urls.HttpsScheme}{settings.Network?.CustomDomain}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ServiceConstants.ApplicationGateway.CreationFailedMessage, settings.Environment);
            throw new ResourceCreationException(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, ServiceConstants.ApplicationGateway.CreationFailedMessage, settings.Environment),
                ex,
                ResourceType.ApplicationGateway.ToStringValue(),
                ResourceType.ApplicationGateway.ToStringValue(),
                settings.Environment,
                _correlationIdService.GetOrGenerateCorrelationId(),
                ServiceConstants.ErrorCodes.AppGatewayCreationFailed);
        }
    }

    private Task<PublicIPAddress> CreatePublicIpAsync(InfrastructureSettings settings, Input<string> resourceGroup)
    {
        try
        {
            var publicIpName = _namingService.GeneratePublicIpName(settings.NamingPrefix, settings.Environment);

            _logger.LogInformation(ServiceConstants.ApplicationGateway.PublicIpCreationMessage, publicIpName);

            var publicIp = new PublicIPAddress(publicIpName, new PublicIPAddressArgs
            {
                PublicIpAddressName = publicIpName,
                ResourceGroupName = resourceGroup,
                Location = settings.Location,
                PublicIPAllocationMethod = DeploymentConstants.Network.StaticAllocation,
                Sku = new PublicIPAddressSkuArgs
                {
                    Name = DeploymentConstants.Network.StandardSku,
                    Tier = DeploymentConstants.Network.RegionalTier
                },
                DnsSettings = new PublicIPAddressDnsSettingsArgs
                {
                    DomainNameLabel = $"{settings.NamingPrefix}-{settings.Environment}"
                },
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, DeploymentConstants.ResourceTags.PublicIpType)
            });

            return Task.FromResult(publicIp);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Pulumi") || ex.Message.Contains("Deployment.Instance"))
        {
            _logger.LogWarning("Pulumi context exception in CreatePublicIpAsync: {Message}", ex.Message);
            throw new ResourceCreationException(
                "Pulumi context error creating Public IP",
                ex,
                ServiceConstants.ResourceTypes.PublicIP,
                ServiceConstants.ResourceTypes.Network,
                _correlationIdService.GetOrGenerateCorrelationId(),
                _correlationIdService.GetOrGenerateCorrelationId(),
                ServiceConstants.ErrorCodes.AppGatewayCreationFailed);
        }
        catch (Exception ex) when (ex.Message.Contains("Pulumi") || ex.Message.Contains("Deployment.Instance"))
        {
            _logger.LogWarning("Pulumi context exception in CreatePublicIpAsync: {Message}", ex.Message);
            throw new ResourceCreationException(
                "Pulumi context error creating Public IP",
                ex,
                ServiceConstants.ResourceTypes.PublicIP,
                ServiceConstants.ResourceTypes.Network,
                _correlationIdService.GetOrGenerateCorrelationId(),
                _correlationIdService.GetOrGenerateCorrelationId(),
                ServiceConstants.ErrorCodes.AppGatewayCreationFailed);
        }
    }

    private Task<ApplicationGateway> CreateApplicationGatewayAsync(InfrastructureSettings settings, Input<string> resourceGroup, NetworkOutputs network, ContainerAppsOutputs containerApps, PublicIPAddress publicIp, CertificateOutputs? certificate)
    {
        var appGatewayName = _namingService.GenerateApplicationGatewayName(settings.NamingPrefix, settings.Environment);

        _logger.LogInformation(ServiceConstants.ApplicationGateway.AppGatewayCreationMessage, appGatewayName);

        // Build SSL Certificates list if certificate is provided
        InputList<ApplicationGatewaySslCertificateArgs>? sslCertificates = null;
        if (certificate?.IsReady == true && certificate.KeyVaultSecretId != null)
        {
            var certName = settings.Network?.SslCertificateName ?? "default-ssl-cert";
            sslCertificates = new InputList<ApplicationGatewaySslCertificateArgs>
            {
                new ApplicationGatewaySslCertificateArgs
                {
                    Name = certName,
                    KeyVaultSecretId = certificate.KeyVaultSecretId
                }
            };
            _logger.LogInformation("SSL certificate configured for Application Gateway: {CertName}, SecretId: {SecretId}",
                certName, certificate.KeyVaultSecretId);
        }

        var appGateway = new ApplicationGateway(appGatewayName, new ApplicationGatewayArgs
        {
            ApplicationGatewayName = appGatewayName,
            ResourceGroupName = resourceGroup,
            Location = settings.Location,
            Identity = (certificate != null ? new ManagedServiceIdentityArgs
            {
                Type = ResourceIdentityType.SystemAssigned
            } : null) ?? throw new InvalidOperationException("Certificate cannot be null"),
            Sku = new ApplicationGatewaySkuArgs
            {
                Name = DeploymentConstants.Network.StandardV2Sku,
                Tier = DeploymentConstants.Network.StandardV2Tier
                // Note: Capacity is omitted when using AutoscaleConfiguration
                // Azure requires EITHER Capacity OR AutoscaleConfiguration, not both
            },
            AutoscaleConfiguration = new ApplicationGatewayAutoscaleConfigurationArgs
            {
                MinCapacity = DeploymentConstants.Network.MinCapacity,
                MaxCapacity = DeploymentConstants.Network.MaxCapacity
            },
            SslCertificates = sslCertificates ?? throw new InvalidOperationException("Ssl certificate cannot be null"),
            GatewayIPConfigurations = new[]
            {
                new ApplicationGatewayIPConfigurationArgs
                {
                    Name = ServiceConstants.ApplicationGateway.IpConfigurationName,
                    Subnet = new SubResourceArgs
                    {
                        Id = network.ApplicationGatewaySubnetId
                    }
                }
            },
            FrontendIPConfigurations = new[]
            {
                new ApplicationGatewayFrontendIPConfigurationArgs
                {
                    Name = ServiceConstants.ApplicationGateway.FrontendIpConfigurationName,
                    PublicIPAddress = new SubResourceArgs
                    {
                        Id = publicIp.Id
                    }
                }
            },
            FrontendPorts = new[]
            {
                new ApplicationGatewayFrontendPortArgs
                {
                    Name = DeploymentConstants.Network.Port80Name,
                    Port = DeploymentConstants.Network.HttpPort
                },
                new ApplicationGatewayFrontendPortArgs
                {
                    Name = DeploymentConstants.Network.Port443Name,
                    Port = DeploymentConstants.Network.HttpsPort
                }
            },
            BackendAddressPools = new[]
            {
                new ApplicationGatewayBackendAddressPoolArgs
                {
                    Name = ServiceConstants.ApplicationGateway.BackendPoolName,
                    BackendAddresses = new[]
                    {
                        new ApplicationGatewayBackendAddressArgs
                        {
                            Fqdn = containerApps.ApiAppUrl.Apply(url => url.Replace(ServiceConstants.ApplicationGateway.HttpsScheme, "").Replace(ServiceConstants.ApplicationGateway.HttpScheme, ""))
                        }
                    }
                }
            },
            BackendHttpSettingsCollection = new[]
            {
                new ApplicationGatewayBackendHttpSettingsArgs
                {
                    Name = DeploymentConstants.Network.BackendHttpSettingsName,
                    Port = DeploymentConstants.Network.HttpPort, // Container Apps internal port
                    Protocol = NetworkProtocolType.Http.ToStringValue(), // Internal communication uses HTTP
                    CookieBasedAffinity = DeploymentConstants.Network.DisabledAffinity,
                    RequestTimeout = DeploymentConstants.Network.DefaultRequestTimeout,
                    PickHostNameFromBackendAddress = true,
                    ProbeEnabled = true,
                    Probe = new SubResourceArgs
                    {
                        Id = Output.Format($"/subscriptions/{settings.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Network/applicationGateways/{appGatewayName}/probes/{DeploymentConstants.Network.HealthProbeName}")
                    }
                }
            },
            HttpListeners = BuildHttpListeners(settings, appGatewayName, resourceGroup),
            RequestRoutingRules = new[]
            {
                new ApplicationGatewayRequestRoutingRuleArgs
                {
                    Name = DeploymentConstants.Network.RoutingRuleName,
                    RuleType = DeploymentConstants.Network.BasicRuleType,
                    Priority = DeploymentConstants.Network.DefaultPriority,
                    HttpListener = new SubResourceArgs
                    {
                        Id = Output.Format($"/subscriptions/{settings.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Network/applicationGateways/{appGatewayName}/httpListeners/{DeploymentConstants.Network.HttpListenerName}")
                    },
                    BackendAddressPool = new SubResourceArgs
                    {
                        Id = Output.Format($"/subscriptions/{settings.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Network/applicationGateways/{appGatewayName}/backendAddressPools/{DeploymentConstants.Network.BackendPoolName}")
                    },
                    BackendHttpSettings = new SubResourceArgs
                    {
                        Id = Output.Format($"/subscriptions/{settings.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Network/applicationGateways/{appGatewayName}/backendHttpSettingsCollection/{DeploymentConstants.Network.BackendHttpSettingsName}")
                    }
                }
            },
            Probes = new[]
            {
                new ApplicationGatewayProbeArgs
                {
                    Name = DeploymentConstants.Network.HealthProbeName,
                    Protocol = NetworkProtocolType.Http.ToStringValue(), // Internal health check uses HTTP
                    Path = DeploymentConstants.Network.HealthCheckPath,
                    Interval = DeploymentConstants.Network.DefaultProbeInterval,
                    Timeout = DeploymentConstants.Network.DefaultProbeTimeout,
                    UnhealthyThreshold = DeploymentConstants.Network.DefaultUnhealthyThreshold,
                    PickHostNameFromBackendHttpSettings = true,
                    MinServers = DeploymentConstants.Network.MinServers,
                    Match = new ApplicationGatewayProbeHealthResponseMatchArgs
                    {
                        StatusCodes = new[] { DeploymentConstants.Network.HealthyStatusCodes }
                    }
                }
            },
            WebApplicationFirewallConfiguration = new ApplicationGatewayWebApplicationFirewallConfigurationArgs
            {
                Enabled = true,
                FirewallMode = DeploymentConstants.Network.PreventionMode,
                RuleSetType = DeploymentConstants.Network.OwaspRuleSet,
                RuleSetVersion = DeploymentConstants.Network.OwaspVersion
            },
            Tags = ResourceTagHelper.GetStandardTags(settings.Environment, DeploymentConstants.ResourceTags.ApplicationGatewayType)
        });

        return Task.FromResult(appGateway);
    }

    /// <summary>
    /// Explicit implementation of IInfrastructureService.CreateAsync with CancellationToken
    /// </summary>
    Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();

        throw new ResourceCreationException(
            $"Failed to create Application Gateway for environment: {settings.Environment}. NetworkOutputs and ContainerAppsOutputs are required.",
            null,
            "ApplicationGateway",
            settings.Environment,
            correlationId,
            ServiceConstants.ErrorCodes.AppGatewayCreationFailed);
    }

    /// <summary>
    /// Explicit implementation of IInfrastructureService.CreateAsync without CancellationToken
    /// </summary>
    Task<object> IInfrastructureService.CreateAsync(InfrastructureSettings settings, Input<string> resourceGroup)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();

        throw new ResourceCreationException(
            $"Failed to create Application Gateway for environment: {settings.Environment}. NetworkOutputs and ContainerAppsOutputs are required.",
            null,
            "ApplicationGateway",
            settings.Environment,
            correlationId,
            ServiceConstants.ErrorCodes.AppGatewayCreationFailed);
    }

    private InputList<ApplicationGatewayHttpListenerArgs> BuildHttpListeners(
        InfrastructureSettings settings,
        string appGatewayName,
        Input<string> resourceGroup)
    {
        var listeners = new InputList<ApplicationGatewayHttpListenerArgs>();

        // Determine if SSL certificate is configured
        var hasSslCert = settings.CustomDomain?.Enabled == true &&
                        settings.CustomDomain.BindToApplicationGateway &&
                        !string.IsNullOrEmpty(settings.Network?.SslCertificateName);

        // HTTPS listener (with SSL certificate if available)
        var httpsListener = new ApplicationGatewayHttpListenerArgs
        {
            Name = DeploymentConstants.Network.HttpListenerName,
            FrontendIPConfiguration = new SubResourceArgs
            {
                Id = Output.Format($"/subscriptions/{settings.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Network/applicationGateways/{appGatewayName}/frontendIPConfigurations/{DeploymentConstants.Network.FrontendIpConfigName}")
            },
            FrontendPort = new SubResourceArgs
            {
                Id = Output.Format($"/subscriptions/{settings.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Network/applicationGateways/{appGatewayName}/frontendPorts/{DeploymentConstants.Network.Port443Name}")
            },
            Protocol = NetworkProtocolType.Https.ToStringValue(),
            HostName = settings.CustomDomain?.Name ?? settings.Network?.CustomDomain ?? throw new InvalidOperationException("Custom domain must be specified"),
            RequireServerNameIndication = true
        };

        // Add SSL certificate reference if configured
        if (hasSslCert)
        {
            httpsListener.SslCertificate = new SubResourceArgs
            {
                Id = Output.Format($"/subscriptions/{settings.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Network/applicationGateways/{appGatewayName}/sslCertificates/{settings.Network!.SslCertificateName}")
            };
            _logger.LogInformation("SSL certificate configured for HTTPS listener: {CertName}", settings.Network.SslCertificateName);
        }
        else
        {
            _logger.LogWarning("No SSL certificate configured for Application Gateway. HTTPS listener will be created without SSL binding.");
        }

        listeners.Add(httpsListener);

        // Optionally add HTTP listener for redirect
        if (settings.CustomDomain?.EnableHttpsRedirect == true)
        {
            var httpListener = new ApplicationGatewayHttpListenerArgs
            {
                Name = "http-listener",
                FrontendIPConfiguration = new SubResourceArgs
                {
                    Id = Output.Format($"/subscriptions/{settings.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Network/applicationGateways/{appGatewayName}/frontendIPConfigurations/{DeploymentConstants.Network.FrontendIpConfigName}")
                },
                FrontendPort = new SubResourceArgs
                {
                    Id = Output.Format($"/subscriptions/{settings.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Network/applicationGateways/{appGatewayName}/frontendPorts/{DeploymentConstants.Network.Port80Name}")
                },
                Protocol = NetworkProtocolType.Http.ToStringValue(),
                HostName = (settings.CustomDomain?.Name ?? settings.Network?.CustomDomain) ?? string.Empty
            };
            listeners.Add(httpListener);
            _logger.LogInformation("HTTP listener configured for HTTPS redirect");
        }

        return listeners;
    }
}


