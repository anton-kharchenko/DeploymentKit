using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

public class BlobStorageSettings
{
    private BlobAccessTierType _accessTier = BlobAccessTierType.Hot;
    private string _accessTierString = string.Empty;

    /// <summary>
    /// Default access tier for blobs
    /// </summary>
    public BlobAccessTierType AccessTier
    {
        get => _accessTier;
        set
        {
            _accessTier = value;
            _accessTierString = value.ToStringValue();
        }
    }

    /// <summary>
    /// String representation of access tier
    /// </summary>
    public string AccessTierString => string.IsNullOrEmpty(_accessTierString) ? _accessTier.ToStringValue() : _accessTierString;

    /// <summary>
    /// Enable blob versioning
    /// </summary>
    public bool EnableVersioning { get; set; }

    /// <summary>
    /// Enable blob change feed
    /// </summary>
    public bool EnableChangeFeed { get; set; }

    /// <summary>
    /// Enable blob soft delete
    /// </summary>
    public bool EnableSoftDelete { get; set; } = true;

    /// <summary>
    /// Soft delete retention days
    /// </summary>
    [Range(1, 365, ErrorMessage = "Soft delete retention must be between 1 and 365 days")]
    public int SoftDeleteRetentionDays { get; set; } = 7;

    /// <summary>
    /// Container names to create
    /// </summary>
    public List<string> ContainerNames { get; set; } = new() { "uploads", "documents", "images" };

    /// <summary>
    /// Enable public access for containers
    /// </summary>
    public bool AllowPublicAccess { get; set; }

    /// <summary>
    /// Default content type for blobs
    /// </summary>
    [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9]/[a-zA-Z0-9][a-zA-Z0-9\-\+]*[a-zA-Z0-9]$", ErrorMessage = "Invalid content type format")]
    public string DefaultContentType { get; set; } = InfrastructureConstants.Storage.DefaultContentType;

    /// <summary>
    /// Enable lifecycle management
    /// </summary>
    public bool EnableLifecycleManagement { get; set; } = true;

    /// <summary>
    /// Days after which to move blobs to cool tier
    /// </summary>
    [Range(1, 999, ErrorMessage = "Cool tier transition days must be between 1 and 999")]
    public int CoolTierTransitionDays { get; set; } = 30;

    /// <summary>
    /// Days after which to move blobs to archive tier
    /// </summary>
    [Range(1, 999, ErrorMessage = "Archive tier transition days must be between 1 and 999")]
    public int ArchiveTierTransitionDays { get; set; } = 90;

    /// <summary>
    /// Days after which to delete blobs
    /// </summary>
    [Range(1, 999, ErrorMessage = "Delete after days must be between 1 and 999")]
    public int DeleteAfterDays { get; set; } = 365;
}


