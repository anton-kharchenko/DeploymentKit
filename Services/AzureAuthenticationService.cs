using DeploymentKit.Interfaces;

namespace DeploymentKit.Services;

public class AzureAuthenticationService(ILogger<AzureAuthenticationService> logger) : IAzureAuthenticationService
{
    private readonly ILogger<AzureAuthenticationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public void ConfigureServicePrincipalAuthentication()
    {
        var environmentVariables = GetServicePrincipalEnvironmentVariables();
        if (environmentVariables.Count == 0)
        {
            return;
        }

        foreach (var (key, value) in environmentVariables)
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        _logger.LogInformation("Azure Service Principal authentication configured for current process");
    }

    public IDictionary<string, string?> GetServicePrincipalEnvironmentVariables()
    {
        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
        var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
        var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");

        if (string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret) ||
            string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(subscriptionId))
        {
            _logger.LogInformation("Azure Service Principal credentials not found. Falling back to Azure default credential chain.");
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }

        return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["ARM_CLIENT_ID"] = clientId,
            ["ARM_CLIENT_SECRET"] = clientSecret,
            ["ARM_TENANT_ID"] = tenantId,
            ["ARM_SUBSCRIPTION_ID"] = subscriptionId
        };
    }

    public bool ValidateServicePrincipalCredentials()
    {
        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
        var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
        var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");

        var hasServicePrincipal = !string.IsNullOrEmpty(clientId) &&
                                  !string.IsNullOrEmpty(clientSecret) &&
                                  !string.IsNullOrEmpty(tenantId) &&
                                  !string.IsNullOrEmpty(subscriptionId);

        if (hasServicePrincipal)
        {
            _logger.LogInformation("Azure Service Principal authentication validated successfully");
            return true;
        }

        _logger.LogInformation("Service Principal not configured. Azure SDK will use default authentication chain (Azure CLI, Managed Identity, etc.)");
        return true;
    }
}

