using DeploymentKit.Constants;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using DeploymentKit.Utilities;
using Pulumi.AzureNative.Cdn;
using Pulumi.AzureNative.Cdn.Inputs;
using AzureNetwork = global::Pulumi.AzureNative.Network;
using AzureNetworkInputs = global::Pulumi.AzureNative.Network.Inputs;

namespace DeploymentKit.Services;

public class FrontDoorDeployer(
    ILogger<FrontDoorDeployer> logger,
    ICorrelationIdService correlationIdService)
    : IFrontDoorDeployer
{
    private readonly ILogger<FrontDoorDeployer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICorrelationIdService _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));

    public Task<FrontDoorOutputs?> CreateFoundationAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.FrontDoor?.Enabled != true)
        {
            return Task.FromResult<FrontDoorOutputs?>(null);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        _logger.LogInformation("Creating Front Door foundation (profile/WAF) for environment: {Environment} CorrelationId: {CorrelationId}",
            settings.Environment, correlationId);

        var profileName = $"{settings.NamingPrefix}-afd-profile-{settings.Environment}";
        var skuName = ParseSku(settings.FrontDoor.SkuName);

        var profile = new Profile(profileName, new ProfileArgs
        {
            ResourceGroupName = resourceGroup,
            ProfileName = profileName,
            Location = DeploymentConstants.Network.GlobalLocation,
            Sku = new SkuArgs { Name = skuName },
            Tags = ResourceTagHelper.GetStandardTags(settings.Environment, "frontdoor-profile")
        });

        var outputs = new FrontDoorOutputs
        {
            ProfileName = profile.Name,
            ProfileResourceId = profile.Id,
            FrontDoorId = profile.Id
        };

        if (settings.FrontDoor.EnableWaf)
        {
            var allowedIpRanges = ResolveAllowedIpRanges(settings);
            if (allowedIpRanges.Count == 0)
            {
                throw new ArgumentException(
                    $"Front Door WAF is enabled but no allowed IP ranges were provided. " +
                    $"Set {EnvironmentVariableNames.FrontDoor.AllowedIpRanges} or {EnvironmentVariableNames.Container.IngressIpRestrictions}.");
            }

            var wafPolicyName = $"{settings.NamingPrefix}-afd-waf-{settings.Environment}";
            var waf = new AzureNetwork.WebApplicationFirewallPolicy(wafPolicyName, new AzureNetwork.WebApplicationFirewallPolicyArgs
            {
                ResourceGroupName = resourceGroup,
                PolicyName = wafPolicyName,
                Location = settings.Location,
                PolicySettings = new AzureNetworkInputs.PolicySettingsArgs
                {
                    State = AzureNetwork.WebApplicationFirewallEnabledState.Enabled,
                    Mode = AzureNetwork.WebApplicationFirewallMode.Prevention
                },
                CustomRules = BuildWafCustomRules(settings, allowedIpRanges),
                Tags = ResourceTagHelper.GetStandardTags(settings.Environment, "frontdoor-waf")
            });

            outputs.WafPolicyResourceId = waf.Id;
        }

        return Task.FromResult<FrontDoorOutputs?>(outputs);
    }

    public Task<FrontDoorOutputs?> ConfigureRoutingAsync(
        InfrastructureSettings settings,
        Input<string> resourceGroup,
        FrontDoorOutputs foundation,
        StorageOutputs storage,
        ContainerAppsOutputs containerApps,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(foundation);
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(containerApps);

        if (settings.FrontDoor?.Enabled != true)
        {
            return Task.FromResult<FrontDoorOutputs?>(null);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var correlationId = _correlationIdService.GetOrGenerateCorrelationId();
        _logger.LogInformation("Configuring Front Door multi-surface routing for environment: {Environment} CorrelationId: {CorrelationId}",
            settings.Environment, correlationId);

        var endpointName = settings.FrontDoor.EndpointNameOverride ?? $"{settings.NamingPrefix}-afd-{settings.Environment}";
        var endpoint = new AFDEndpoint(endpointName, new AFDEndpointArgs
        {
            ResourceGroupName = resourceGroup,
            ProfileName = foundation.ProfileName,
            EndpointName = endpointName,
            Location = DeploymentConstants.Network.GlobalLocation,
            EnabledState = EnabledState.Enabled,
            Tags = ResourceTagHelper.GetStandardTags(settings.Environment, "frontdoor-endpoint")
        });

        var websiteDomain = CreateCustomDomain(resourceGroup, foundation, settings.FrontDoor.WebsiteHostName, "website");
        var miniAppDomain = CreateCustomDomain(resourceGroup, foundation, settings.FrontDoor.MiniAppHostName, "miniapp");
        var apiDomain = CreateCustomDomain(resourceGroup, foundation, settings.FrontDoor.ApiHostName, "api");

        var websiteOriginHost = ToHostName(storage.WebsitePrimaryEndpoint);
        var miniAppOriginHost = ToHostName(storage.MiniAppPrimaryEndpoint);
        var apiOriginHost = ToHostName(containerApps.ApiAppUrl);

        var websiteOriginGroup = CreateOriginGroup(resourceGroup, foundation, settings.FrontDoor, "website");
        var miniAppOriginGroup = CreateOriginGroup(resourceGroup, foundation, settings.FrontDoor, "miniapp");
        var apiOriginGroup = CreateOriginGroup(resourceGroup, foundation, settings.FrontDoor, "api");

        CreateOrigin(resourceGroup, foundation, websiteOriginGroup, websiteOriginHost, "website");
        CreateOrigin(resourceGroup, foundation, miniAppOriginGroup, miniAppOriginHost, "miniapp");
        CreateOrigin(resourceGroup, foundation, apiOriginGroup, apiOriginHost, "api");

        CreateRoute(resourceGroup, foundation, endpoint, websiteOriginGroup, "website", ["/*"], websiteDomain?.Id);
        CreateRoute(resourceGroup, foundation, endpoint, miniAppOriginGroup, "miniapp-static", ["/*"], miniAppDomain?.Id);
        CreateRoute(resourceGroup, foundation, endpoint, apiOriginGroup, "miniapp-api", ["/api/*", "/graphql", "/health*"], miniAppDomain?.Id);
        CreateRoute(resourceGroup, foundation, endpoint, apiOriginGroup, "api", ["/*"], apiDomain?.Id);

        if (settings.FrontDoor.EnableWaf && foundation.WafPolicyResourceId != null)
        {
            var domains = new List<Input<string>> { endpoint.Id };
            AddDomainIfPresent(domains, websiteDomain);
            AddDomainIfPresent(domains, miniAppDomain);
            AddDomainIfPresent(domains, apiDomain);

            new SecurityPolicy($"{endpointName}-security-policy", new SecurityPolicyArgs
            {
                ResourceGroupName = resourceGroup,
                ProfileName = foundation.ProfileName,
                SecurityPolicyName = $"{endpointName}-waf",
                Parameters = new SecurityPolicyWebApplicationFirewallParametersArgs
                {
                    Type = "WebApplicationFirewall",
                    WafPolicy = new ResourceReferenceArgs { Id = foundation.WafPolicyResourceId },
                    Associations =
                    [
                        new SecurityPolicyWebApplicationFirewallAssociationArgs
                        {
                            Domains = domains.Select(id => new ActivatedResourceReferenceArgs { Id = id }).ToArray(),
                            PatternsToMatch = ["/*"]
                        }
                    ]
                }
            });
        }

        return Task.FromResult<FrontDoorOutputs?>(new FrontDoorOutputs
        {
            ProfileName = foundation.ProfileName,
            ProfileResourceId = foundation.ProfileResourceId,
            FrontDoorId = foundation.FrontDoorId,
            WafPolicyResourceId = foundation.WafPolicyResourceId,
            EndpointName = endpoint.Name,
            EndpointHostName = endpoint.HostName,
            CustomDomainHostName = apiDomain?.HostName,
            CustomDomainResourceId = apiDomain?.Id,
            WebsiteCustomDomainHostName = websiteDomain?.HostName,
            MiniAppCustomDomainHostName = miniAppDomain?.HostName,
            ApiCustomDomainHostName = apiDomain?.HostName
        });
    }

    private AFDOriginGroup CreateOriginGroup(Input<string> resourceGroup, FrontDoorOutputs foundation, FrontDoorSettings settings, string suffix)
    {
        var name = $"{suffix}-og";
        return new AFDOriginGroup(name, new AFDOriginGroupArgs
        {
            ResourceGroupName = resourceGroup,
            ProfileName = foundation.ProfileName,
            OriginGroupName = name,
            HealthProbeSettings = new HealthProbeParametersArgs
            {
                ProbePath = settings.HealthProbePath,
                ProbeProtocol = ProbeProtocol.Https,
                ProbeRequestType = HealthProbeRequestType.GET,
                ProbeIntervalInSeconds = 30
            },
            LoadBalancingSettings = new LoadBalancingSettingsParametersArgs
            {
                SampleSize = 4,
                SuccessfulSamplesRequired = 3,
                AdditionalLatencyInMilliseconds = 0
            }
        });
    }

    private AFDOrigin CreateOrigin(Input<string> resourceGroup, FrontDoorOutputs foundation, AFDOriginGroup originGroup, Output<string> hostName, string suffix)
    {
        var name = $"{suffix}-origin";
        return new AFDOrigin(name, new AFDOriginArgs
        {
            ResourceGroupName = resourceGroup,
            ProfileName = foundation.ProfileName,
            OriginGroupName = originGroup.Name,
            OriginName = name,
            HostName = hostName,
            HttpPort = 80,
            HttpsPort = 443,
            EnabledState = EnabledState.Enabled,
            OriginHostHeader = hostName,
            Priority = 1,
            Weight = 1000
        });
    }

    private Route CreateRoute(
        Input<string> resourceGroup,
        FrontDoorOutputs foundation,
        AFDEndpoint endpoint,
        AFDOriginGroup originGroup,
        string routeName,
        IEnumerable<string> patternsToMatch,
        Input<string>? customDomainId)
    {
        InputList<ActivatedResourceReferenceArgs>? routeCustomDomains = null;
        if (customDomainId != null)
        {
            routeCustomDomains =
            [
                new ActivatedResourceReferenceArgs { Id = customDomainId }
            ];
        }

        return new Route(routeName, new RouteArgs
        {
            ResourceGroupName = resourceGroup,
            ProfileName = foundation.ProfileName,
            EndpointName = endpoint.Name,
            RouteName = routeName,
            OriginGroup = new ResourceReferenceArgs { Id = originGroup.Id },
            SupportedProtocols =
            [
                AFDEndpointProtocols.Http,
                AFDEndpointProtocols.Https
            ],
            PatternsToMatch = patternsToMatch.ToArray(),
            ForwardingProtocol = ForwardingProtocol.HttpsOnly,
            LinkToDefaultDomain = customDomainId == null ? LinkToDefaultDomain.Enabled : LinkToDefaultDomain.Disabled,
            HttpsRedirect = HttpsRedirect.Enabled,
            EnabledState = EnabledState.Enabled,
            CustomDomains = routeCustomDomains
        });
    }

    private AFDCustomDomain? CreateCustomDomain(Input<string> resourceGroup, FrontDoorOutputs foundation, Input<string>? hostName, string suffix)
    {
        if (hostName == null)
        {
            return null;
        }

        var customDomainName = $"{suffix}-domain";
        return new AFDCustomDomain(customDomainName, new AFDCustomDomainArgs
        {
            ResourceGroupName = resourceGroup,
            ProfileName = foundation.ProfileName,
            CustomDomainName = customDomainName,
            HostName = hostName,
            TlsSettings = new AFDDomainHttpsParametersArgs
            {
                CertificateType = AfdCertificateType.ManagedCertificate,
                MinimumTlsVersion = AfdMinimumTlsVersion.TLS12
            }
        });
    }

    private static void AddDomainIfPresent(List<Input<string>> domains, AFDCustomDomain? domain)
    {
        if (domain != null)
        {
            domains.Add(domain.Id);
        }
    }

    private static Output<string> ToHostName(Output<string> url) => url.Apply(value =>
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return uri.Host;
        }

        return value.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .TrimEnd('/');
    });

    private static global::Pulumi.AzureNative.Cdn.SkuName ParseSku(string skuName) =>
        skuName.Equals("Premium_AzureFrontDoor", StringComparison.OrdinalIgnoreCase)
            ? global::Pulumi.AzureNative.Cdn.SkuName.Premium_AzureFrontDoor
            : global::Pulumi.AzureNative.Cdn.SkuName.Standard_AzureFrontDoor;

    private static List<string> ResolveAllowedIpRanges(InfrastructureSettings settings)
    {
        if (settings.FrontDoor?.AllowedIpRanges is { Count: > 0 })
        {
            return settings.FrontDoor.AllowedIpRanges;
        }

        var ingress = settings.Container?.IngressSettings?.IpSecurityRestrictions;
        if (ingress is not { Count: > 0 })
        {
            return [];
        }

        return ingress
            .Where(r => r.Action.Equals("Allow", StringComparison.OrdinalIgnoreCase))
            .Select(r => r.IpAddressRange)
            .Where(ip => !string.IsNullOrWhiteSpace(ip))
            .ToList();
    }

    private static InputList<AzureNetworkInputs.WebApplicationFirewallCustomRuleArgs> BuildWafCustomRules(InfrastructureSettings settings, List<string> allowedIpRanges)
    {
        var bypassPrefixes = settings.FrontDoor?.WafBypassPathPrefixes ?? [];
        var matchConditions = new List<AzureNetworkInputs.MatchConditionArgs>
        {
            new()
            {
                MatchVariables =
                [
                    new AzureNetworkInputs.MatchVariableArgs { VariableName = AzureNetwork.WebApplicationFirewallMatchVariable.RemoteAddr }
                ],
                Operator = AzureNetwork.WebApplicationFirewallOperator.IPMatch,
                NegationConditon = true,
                MatchValues = allowedIpRanges
            }
        };

        if (bypassPrefixes.Count > 0)
        {
            matchConditions.Add(new AzureNetworkInputs.MatchConditionArgs
            {
                MatchVariables =
                [
                    new AzureNetworkInputs.MatchVariableArgs { VariableName = AzureNetwork.WebApplicationFirewallMatchVariable.RequestUri }
                ],
                Operator = AzureNetwork.WebApplicationFirewallOperator.BeginsWith,
                NegationConditon = true,
                MatchValues = bypassPrefixes
            });
        }

        return new AzureNetworkInputs.WebApplicationFirewallCustomRuleArgs[]
        {
            new()
            {
                Name = "BlockNonAllowedIps",
                Priority = 1,
                State = AzureNetwork.WebApplicationFirewallState.Enabled,
                RuleType = AzureNetwork.WebApplicationFirewallRuleType.MatchRule,
                Action = AzureNetwork.WebApplicationFirewallAction.Block,
                MatchConditions = matchConditions
            }
        };
    }
}

