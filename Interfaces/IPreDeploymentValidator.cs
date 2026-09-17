using DeploymentKit.Services;
using DeploymentKit.Settings;

namespace DeploymentKit.Interfaces;

public interface IPreDeploymentValidator
{
    PreDeploymentValidationResult ValidateAllResourceNames(InfrastructureSettings settings);
    
    PreDeploymentValidationResult ValidateSettings(InfrastructureSettings settings);
    
    Task<PreDeploymentValidationResult> ValidateAllAsync(InfrastructureSettings settings);
}



