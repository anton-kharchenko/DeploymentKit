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
    private const string AcrPullRoleId = ServiceConstants.ContainerRegistry.AcrPullRoleId;

    /// <summary>
    /// Gets the identity configuration for Container App
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="keyVault">Key Vault outputs</param>
    /// <returns>Managed Service Identity arguments</returns>
    public static ContainerAppManagedServiceIdentityArgs? GetContainerAppIdentity(InfrastructureSettings settings, KeyVaultOutputs? keyVault)
    {
        var needsAcrAccess = settings.Container is { UsePlaceholderImages: false };

        if (!needsAcrAccess && !ContainerAppsSecretsHelper.ShouldUseKeyVaultSecrets(settings, keyVault))
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

    /// <summary>
    /// Configures AcrPull role assignments for Container Apps pulling images from the Azure Container Registry.
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="containerRegistry">Container Registry outputs</param>
    /// <param name="apiApp">API Container App</param>
    /// <param name="jobsApp">Jobs Container App</param>
    /// <param name="botApp">Bot Container App</param>
    public static void ConfigureAcrAccessForContainerApps(
        InfrastructureSettings settings,
        ContainerRegistryOutputs containerRegistry,
        ContainerApp apiApp,
        ContainerApp jobsApp,
        ContainerApp? botApp)
    {
        if (settings.Container is not { UsePlaceholderImages: false })
        {
            return;
        }

        CreateAcrRoleAssignment(
            CreateDeterministicGuid($"{containerRegistry.Name}-api-{AcrPullRoleId}"),
            apiApp,
            settings,
            containerRegistry);

        CreateAcrRoleAssignment(
            CreateDeterministicGuid($"{containerRegistry.Name}-jobs-{AcrPullRoleId}"),
            jobsApp,
            settings,
            containerRegistry);

        if (botApp != null)
        {
            CreateAcrRoleAssignment(
                CreateDeterministicGuid($"{containerRegistry.Name}-bot-{AcrPullRoleId}"),
                botApp,
                settings,
                containerRegistry);
        }
    }

    /// <summary>
    /// Configures an AcrPull role assignment for a single Container App.
    /// </summary>
    /// <param name="settings">Infrastructure settings</param>
    /// <param name="containerRegistry">Container Registry outputs</param>
    /// <param name="app">Container App</param>
    /// <param name="suffix">Suffix identifying the app in the deterministic role assignment name</param>
    public static void ConfigureAcrAccess(
        InfrastructureSettings settings,
        ContainerRegistryOutputs containerRegistry,
        ContainerApp app,
        string suffix)
    {
        if (settings.Container is not { UsePlaceholderImages: false })
        {
            return;
        }

        CreateAcrRoleAssignment(
            CreateDeterministicGuid($"{containerRegistry.Name}-{suffix}-{AcrPullRoleId}"),
            app,
            settings,
            containerRegistry);
    }

    private static void CreateAcrRoleAssignment(
        string roleAssignmentName,
        ContainerApp app,
        InfrastructureSettings settings,
        ContainerRegistryOutputs containerRegistry)
    {
        var principalId = app.Identity.Apply(identity => identity?.PrincipalId ?? string.Empty);
        var roleDefinitionId = $"/subscriptions/{settings.SubscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{AcrPullRoleId}";

        _ = new RoleAssignment(roleAssignmentName, new RoleAssignmentArgs
        {
            PrincipalId = principalId,
            RoleDefinitionId = roleDefinitionId,
            Scope = containerRegistry.ResourceId
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
