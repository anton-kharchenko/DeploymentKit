namespace DeploymentKit.Validators
{
    /// <summary>
    /// Validator for InfrastructureBuilder configuration.
    /// </summary>
    public interface IInfrastructureBuilderValidator
    {
        /// <summary>
        /// Validates the builder configuration.
        /// </summary>
        List<string> Validate(
            string? deploymentName,
            string? environment,
            string? location,
            string? subscriptionId,
            bool hasResourcesConfigured,
            string? keyVaultEnvFilePath);
    }
}
