namespace DeploymentKit.Interfaces;

public interface IAzureAuthenticationService
{
    void ConfigureServicePrincipalAuthentication();

    IDictionary<string, string?> GetServicePrincipalEnvironmentVariables();

    bool ValidateServicePrincipalCredentials();
}

