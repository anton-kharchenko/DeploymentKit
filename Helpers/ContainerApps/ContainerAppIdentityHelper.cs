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
/// Helper class for managing identity and RBAC for container apps.
/// </summary>
public static class ContainerAppIdentityHelper
{
    private const string KeyVaultSecretsUserRoleId = ServiceConstants.KeyVault.SecretsUserRoleId;

    /// <summary>
    /// Determines whether Key Vault secrets should be used for the container app.
    /// </summary>
    /// <param name="settings">The infrastructure settings.</param>
    /// <param name="keyVault">The Key Vault outputs.</param>
    /// <returns>True if Key Vault secrets should be used; otherwise, false.</returns>
    public static bool ShouldUseKeyVaultSecrets(InfrastructureSettings settings, KeyVaultOutputs? keyVault) =>
        settings.KeyVault?.ApplyToContainerApps == true &&
        keyVault != null &&
        settings.KeyVault.Secrets?.Count > 0;

    /// <summary>
    /// Gets the managed service identity configuration for the container app.
    /// </summary>
    /// <param name="settings">The infrastructure settings.</param>
    /// <param name="keyVault">The Key Vault outputs.</param>
    /// <returns>The managed service identity arguments.</returns>
    public static ContainerAppManagedServiceIdentityArgs? GetContainerAppIdentity(InfrastructureSettings settings, KeyVaultOutputs? keyVault)
    {
        if (!ShouldUseKeyVaultSecrets(settings, keyVault))
        {
            return null;
        }

        return new ContainerAppManagedServiceIdentityArgs
        {
            Type = ManagedServiceIdentityType.SystemAssigned
        };
    }

    /// <summary>
    /// Configures Key Vault access for the API and Jobs container apps.
    /// </summary>
    /// <param name="settings">The infrastructure settings.</param>
    /// <param name="keyVault">The Key Vault outputs.</param>
    /// <param name="apiApp">The API container app.</param>
    /// <param name="jobsApp">The Jobs container app.</param>
    /// <param name="namingService">The resource naming service.</param>
    public static void ConfigureKeyVaultAccessForContainerApps(
        InfrastructureSettings settings,
        KeyVaultOutputs? keyVault,
        ContainerApp apiApp,
        ContainerApp jobsApp,
        IResourceNamingService namingService)
    {
        if (!ShouldUseKeyVaultSecrets(settings, keyVault) || settings.KeyVault?.EnableRbacAuthorization != true)
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
        }, new CustomResourceOptions
        {
            DependsOn = new[] { app }
        });
    }

    private static string CreateDeterministicGuid(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Guid.NewGuid().ToString();
        }

        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash).ToString();
    }
}



