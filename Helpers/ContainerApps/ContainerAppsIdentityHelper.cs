using DeploymentKit.Components;
using DeploymentKit.Constants;
using DeploymentKit.Interfaces;
using DeploymentKit.Models.Outputs;
using DeploymentKit.Settings;
using Pulumi.AzureNative.Authorization;
using System.Security.Cryptography;
using System.Text;
using ContainerAppManagedServiceIdentityArgs = Pulumi.AzureNative.App.Inputs.ManagedServiceIdentityArgs;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;

namespace DeploymentKit.Helpers.ContainerApps;

/// <summary>
/// Helper class for Container Apps identity and access management.
/// </summary>
public static class ContainerAppsIdentityHelper
{
    private const string KeyVaultSecretsUserRoleId = ServiceConstants.KeyVault.SecretsUserRoleId;

    /// <summary>
    /// Gets the identity configuration for Container App
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="keyVault">Key Vault outputs</param>
    /// <returns>Managed Service Identity arguments</returns>
    public static ContainerAppManagedServiceIdentityArgs? GetContainerAppIdentity(InfrastructureSettings settings, KeyVaultOutputs? keyVault)
    {
        if (!ContainerAppsSecretsHelper.ShouldUseKeyVaultSecrets(settings, keyVault))
        {
            return null;
        }

        return new ContainerAppManagedServiceIdentityArgs
        {
            Type = ManagedServiceIdentityType.SystemAssigned
        };
    }

    /// <summary>
    /// Configures Key Vault access for Container Apps
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="keyVault">Key Vault outputs</param>
    /// <param name="apiApp">API Container App</param>
    /// <param name="jobsApp">Jobs Container App</param>
    /// <param name="namingService">Resource naming service</param>
    public static void ConfigureKeyVaultAccessForContainerApps(
        InfrastructureSettings settings,
        KeyVaultOutputs? keyVault,
        ContainerApp apiApp,
        ContainerApp jobsApp,
        ContainerApp? botApp,
        IResourceNamingService namingService)
    {
        if (!ContainerAppsSecretsHelper.ShouldUseKeyVaultSecrets(settings, keyVault) || settings.KeyVault?.EnableRbacAuthorization != true)
        {
            return;
        }

        var keyVaultName = namingService.GenerateKeyVaultName(settings.NamingPrefix, settings.Environment);
        var roleDefinitionId = $"/subscriptions/{settings.SubscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{KeyVaultSecretsUserRoleId}";

        CreateKeyVaultRoleAssignment(
            CreateDeterministicGuid($"{keyVaultName}-{settings.Environment}-api-{KeyVaultSecretsUserRoleId}"),
            apiApp,
            keyVault!,
            roleDefinitionId);

        CreateKeyVaultRoleAssignment(
            CreateDeterministicGuid($"{keyVaultName}-{settings.Environment}-jobs-{KeyVaultSecretsUserRoleId}"),
            jobsApp,
            keyVault!,
            roleDefinitionId);

        if (botApp != null)
        {
            CreateKeyVaultRoleAssignment(
                CreateDeterministicGuid($"{keyVaultName}-{settings.Environment}-bot-{KeyVaultSecretsUserRoleId}"),
                botApp,
                keyVault!,
                roleDefinitionId);
        }
    }

    private static void CreateKeyVaultRoleAssignment(
        string roleAssignmentName,
        ContainerApp app,
        KeyVaultOutputs keyVault,
        string roleDefinitionId)
    {
        var principalId = app.Identity.Apply(identity => identity?.PrincipalId ?? string.Empty);

        _ = new RoleAssignment(roleAssignmentName, new RoleAssignmentArgs
        {
            PrincipalId = principalId,
            RoleDefinitionId = roleDefinitionId,
            Scope = keyVault.ResourceId
        }, ComponentResourceScope.CreateChildOptions(roleAssignmentName, options => options.DependsOn = new[] { app }));
    }

    private static string CreateDeterministicGuid(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Guid.NewGuid().ToString();
        }
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash).ToString();
    }
}
