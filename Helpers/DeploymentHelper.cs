using DeploymentKit.Constants;
using DeploymentKit.Models;
using DeploymentKit.Settings;
using Pulumi.AzureNative.Dns;
using Pulumi.AzureNative.Dns.Inputs;

namespace DeploymentKit.Helpers;

/// <summary>
/// Helper methods for Azure deployment operations
/// </summary>
public static class DeploymentHelper
{
    /// <summary>
    /// Parses ALLOWED_IP_RANGES environment variable and creates IP security restriction settings.
    /// Format: "Name1|IP1|Description1;Name2|IP2|Description2"
    /// </summary>
    /// <returns>List of IP security restriction settings</returns>
    public static List<IpSecurityRestrictionSettings> GetAllowedIpRanges()
    {
        var allowedIps =
            Environment.GetEnvironmentVariable(EnvironmentVariableNames.Container.IngressIpRestrictions)
            ?? Environment.GetEnvironmentVariable("ALLOWED_IP_RANGES");

        if (string.IsNullOrWhiteSpace(allowedIps))
        {
            return new List<IpSecurityRestrictionSettings>();
        }

        var ipEntries = allowedIps.Split(';', StringSplitOptions.RemoveEmptyEntries);

        return (from entry in ipEntries
                select entry.Split('|')
                into parts
                where parts.Length >= 2
                select new IpSecurityRestrictionSettings
                {
                    Name = parts[0].Trim(),
                    IpAddressRange = parts[1].Trim(),
                    Action = "Allow",
                    Description = parts.Length >= 3 ? parts[2].Trim() : parts[0].Trim()
                }).ToList();
    }

    /// <summary>
    /// Parses an environment file (.env) and returns key-value pairs
    /// </summary>
    /// <param name="envFilePath">Path to the .env file</param>
    /// <returns>Dictionary of environment variables</returns>
    public static async Task<Dictionary<string, string>> ParseEnvFileAsync(string envFilePath)
    {
        string[] lines;
        try
        {
            lines = await File.ReadAllLinesAsync(envFilePath).ConfigureAwait(false);
        }
        catch (FileNotFoundException)
        {
            return new Dictionary<string, string>();
        }
        catch (DirectoryNotFoundException)
        {
            return new Dictionary<string, string>();
        }

        var envVars = new Dictionary<string, string>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || !line.Contains('='))
                continue;

            var parts = line.Split('=', 2);
            var key = parts[0].Trim();
            var value = parts.Length > 1 ? parts[1].Trim().Trim('"') : "";
            envVars[key] = value;
        }

        return envVars;
    }

    /// <summary>
    /// Creates Azure CLI commands to inject secrets and environment variables into a Container App
    /// </summary>
    /// <param name="envVars">Environment variables to inject</param>
    /// <param name="containerAppName">Container App name</param>
    /// <param name="resourceGroupName">Resource group name</param>
    /// <param name="commandNamePrefix">Prefix for command names</param>
    /// <returns>Command resource or null if no variables to inject</returns>
    public static Command? CreateSecretInjectionCommands(
        Dictionary<string, string> envVars,
        string containerAppName,
        string resourceGroupName,
        string commandNamePrefix)
    {
        if (envVars.Count == 0)
        {
            return null;
        }

        var secretsList = new List<string>();
        var envVarsList = new List<string>();

        foreach (var kv in envVars)
        {
            var secretName = kv.Key.ToLower().Replace("_", "-");
            var secretValue = kv.Value.Replace("\"", "\\\"");
            secretsList.Add($"{secretName}={secretValue}");
            envVarsList.Add($"{kv.Key}=secretref:{secretName}");
        }

        var secretsCommand = $"az containerapp secret set --name {containerAppName} --resource-group {resourceGroupName} --secrets " +
                             string.Join(" ", secretsList.Select(s => $"\"{s}\""));

        var injectSecrets = new Command($"{commandNamePrefix}-secrets", new CommandArgs
        {
            Create = secretsCommand
        });

        var envVarsCommand = $"az containerapp update --name {containerAppName} --resource-group {resourceGroupName} --set-env-vars " +
                             string.Join(" ", envVarsList.Select(e => $"\"{e}\""));

        return new Command($"{commandNamePrefix}-env-vars", new CommandArgs
        {
            Create = envVarsCommand
        }, new CustomResourceOptions { DependsOn = new[] { injectSecrets } });
    }

    /// <summary>
    /// Creates DNS records (CNAME and TXT) for custom domain validation
    /// </summary>
    /// <param name="dnsConfig">DNS configuration</param>
    /// <param name="verificationToken">Azure Container Apps verification token</param>
    /// <returns>Tuple of CNAME and TXT record sets</returns>
    public static (RecordSet cnameRecord, RecordSet txtRecord) CreateDnsRecords(
        DnsConfig dnsConfig,
        string[] verificationToken)
    {
        var cnameRecord = new RecordSet($"cname-{dnsConfig.CustomDomain}", new RecordSetArgs
        {
            ResourceGroupName = dnsConfig.ResourceGroupName,
            ZoneName = dnsConfig.ZoneName,
            RecordType = "CNAME",
            RelativeRecordSetName = dnsConfig.CustomDomain,
            Ttl = 3600,
            CnameRecord = new CnameRecordArgs
            {
                Cname = dnsConfig.ContainerAppFqdn
            }
        });

        var txtRecord = new RecordSet($"txt-asuid-{dnsConfig.CustomDomain}", new RecordSetArgs
        {
            ResourceGroupName = dnsConfig.ResourceGroupName,
            ZoneName = dnsConfig.ZoneName,
            RecordType = "TXT",
            RelativeRecordSetName = $"asuid.{dnsConfig.CustomDomain}",
            Ttl = 3600,
            TxtRecords = new[]
            {
                new TxtRecordArgs { Value = verificationToken }
            }
        });

        return (cnameRecord, txtRecord);
    }

    /// <summary>
    /// Binds a custom domain to a Container App with managed certificate
    /// </summary>
    /// <param name="dnsConfig">DNS configuration</param>
    /// <param name="cnameRecord">CNAME record resource</param>
    /// <param name="txtRecord">TXT record resource</param>
    /// <param name="envVarsCommand">Optional environment variables command to depend on</param>
    public static void BindCustomDomain(
        DnsConfig dnsConfig,
        RecordSet cnameRecord,
        RecordSet txtRecord,
        Command? envVarsCommand)
    {
        var domainBindingDeps = envVarsCommand != null
            ? new Resource[] { cnameRecord, txtRecord, envVarsCommand }
            : new Resource[] { cnameRecord, txtRecord };

        var fullDomainName = dnsConfig.FullDomainName;

        var customDomainBinding = new Command($"bind-custom-domain-{dnsConfig.CustomDomain}", new CommandArgs
        {
            Create = $"az containerapp hostname add --resource-group {dnsConfig.ResourceGroupName} --name {dnsConfig.ContainerAppName} --hostname {fullDomainName}",
            Delete = $"az containerapp hostname delete --resource-group {dnsConfig.ResourceGroupName} --name {dnsConfig.ContainerAppName} --hostname {fullDomainName} --yes"
        }, new CustomResourceOptions { DependsOn = domainBindingDeps });

        _ = new Command($"add-managed-cert-{dnsConfig.CustomDomain}", new CommandArgs
        {
            Create = $"az containerapp hostname bind --resource-group {dnsConfig.ResourceGroupName} --name {dnsConfig.ContainerAppName} --hostname {fullDomainName} --environment {dnsConfig.EnvironmentName} --validation-method CNAME"
        }, new CustomResourceOptions { DependsOn = new[] { customDomainBinding } });
    }
}

