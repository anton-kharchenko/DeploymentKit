using DeploymentKit.Interfaces;
using Azure.Identity;
using Azure.ResourceManager;

namespace DeploymentKit.Services;

/// <summary>
/// Provides Azure Resource Manager (ARM) client instances using DefaultAzureCredential
/// </summary>
public class ArmClientProvider : IArmClientProvider
{
    private readonly ILogger<ArmClientProvider> _logger;
    private ArmClient? _armClient;
    private readonly Lock _lock = new();

    /// <summary>
    /// Initializes a new instance of the ArmClientProvider class
    /// </summary>
    /// <param name="logger">Logger instance for logging operations</param>
    public ArmClientProvider(ILogger<ArmClientProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets an Azure Resource Manager client instance
    /// Uses lazy initialization with thread safety
    /// </summary>
    /// <returns>An ArmClient instance configured with DefaultAzureCredential</returns>
    public ArmClient GetArmClient()
    {
        if (_armClient != null)
            return _armClient;

        lock (_lock)
        {
            if (_armClient != null)
                return _armClient;

            try
            {
                _logger.LogDebug("Creating new ArmClient with DefaultAzureCredential");
                _armClient = new ArmClient(new DefaultAzureCredential());
                _logger.LogDebug("ArmClient created successfully");
                return _armClient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ArmClient with DefaultAzureCredential");
                throw;
            }
        }
    }

    /// <summary>
    /// Gets an Azure Resource Manager client instance asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>An ArmClient instance configured with DefaultAzureCredential</returns>
    public Task<ArmClient> GetArmClientAsync(CancellationToken cancellationToken = default)
    {
        // Since ArmClient creation is synchronous, we return a completed task
        // This method is provided for future extensibility if async initialization is needed
        return Task.FromResult(GetArmClient());
    }
}

