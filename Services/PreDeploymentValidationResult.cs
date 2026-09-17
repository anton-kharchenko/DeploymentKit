namespace DeploymentKit.Services;

public class PreDeploymentValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = [];
    public List<string> Warnings { get; } = [];
    public List<string> ValidatedResources { get; } = [];


    public void PrintSummary(ILogger logger)
    {
        logger.LogInformation("\n═══════════════════════════════════════════════════════");
        logger.LogInformation("        PRE-DEPLOYMENT VALIDATION REPORT");
        logger.LogInformation("═══════════════════════════════════════════════════════");

        if (IsValid)
        {
            logger.LogInformation("\n✅ STATUS: PASSED - All resource names are valid\n");
            logger.LogInformation("Validated Resources ({Count}):", ValidatedResources.Count);
            foreach (var resource in ValidatedResources)
            {
                logger.LogInformation("  {Resource}", resource);
            }
        }
        else
        {
            logger.LogError("\n❌ STATUS: FAILED - Found {ErrorCount} errors\n", Errors.Count);
            logger.LogError("Errors:");
            foreach (var error in Errors)
            {
                logger.LogError("  {Error}", error);
            }
        }

        if (Warnings.Count > 0)
        {
            logger.LogWarning("\n⚠️  Warnings ({Count}):", Warnings.Count);
            foreach (var warning in Warnings)
            {
                logger.LogWarning("  {Warning}", warning);
            }
        }

        logger.LogInformation("\n═══════════════════════════════════════════════════════\n");
    }
}

