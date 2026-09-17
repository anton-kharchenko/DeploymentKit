using DeploymentKit.Constants;
using DeploymentKit.Enums;
using DeploymentKit.Extensions;
using System.ComponentModel.DataAnnotations;

namespace DeploymentKit.Settings;

public class DatabaseSettings
{
    private DatabaseSkuType _skuName = DatabaseSkuType.StandardB1ms;
    private string _skuNameString = string.Empty;
    private PostgreSqlVersionType _version = PostgreSqlVersionType.Version16;
    private string _versionString = string.Empty;

    /// <summary>
    /// PostgreSQL version
    /// </summary>
    public PostgreSqlVersionType Version
    {
        set
        {
            _version = value;
            _versionString = value.ToStringValue();
        }
    }

    /// <summary>
    /// Database SKU name
    /// </summary>
    public DatabaseSkuType SkuName
    {
        get => _skuName;
        init
        {
            _skuName = value;
            _skuNameString = value.ToStringValue();
        }
    }

    /// <summary>
    /// Availability zone for the database
    /// </summary>
    [RegularExpression(@"^[1-3]$", ErrorMessage = "Availability zone must be 1, 2, or 3")]
    public string AvailabilityZone { get; set; } = InfrastructureConstants.Database.DefaultAvailabilityZone;

    /// <summary>
    /// Storage size in GB
    /// </summary>
    [Range(InfrastructureConstants.Database.MinStorageSizeGb, InfrastructureConstants.Database.MaxStorageSizeGb, ErrorMessage = "Storage size must be between 32 and 32767 GB")]
    public int StorageSizeGb { get; set; } = InfrastructureConstants.Database.MinStorageSizeGb;

    /// <summary>
    /// Backup retention period in days
    /// </summary>
    [Range(7, 35, ErrorMessage = "Backup retention must be between 7 and 35 days")]
    public int BackupRetentionDays { get; set; } = 7;

    /// <summary>
    /// Enable high availability
    /// </summary>
    public bool EnableHighAvailability { get; set; }

    /// <summary>
    /// Enable geo-redundant backup
    /// </summary>
    public bool EnableGeoRedundantBackup { get; set; }

    /// <summary>
    /// Geo-redundant backup (alias for EnableGeoRedundantBackup)
    /// </summary>
    public bool GeoRedundantBackup
    {
        get => EnableGeoRedundantBackup;
        set => EnableGeoRedundantBackup = value;
    }

    /// <summary>
    /// Whether the database is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Database administrator username
    /// </summary>
    [Required(ErrorMessage = "Admin username is required")]
    [StringLength(63, MinimumLength = 1, ErrorMessage = "Admin username must be between 1 and 63 characters")]
    public string AdminUser { get; set; } = InfrastructureConstants.Database.DefaultAdminUser;

    /// <summary>
    /// Database administrator username (alias for AdminUser)
    /// </summary>
    public string AdminUsername
    {
        get => AdminUser;
        init => AdminUser = value;
    }

    /// <summary>
    /// Database administrator password
    /// </summary>
    [Required(ErrorMessage = "Admin password is required")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Admin password must be between 8 and 128 characters")]
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>
    /// Database password (alias for AdminPassword)
    /// </summary>
    public string Password
    {
        get => AdminPassword;
        init => AdminPassword = value;
    }

    // String properties for backward compatibility and Pulumi integration
    /// <summary>
    /// PostgreSQL version as string (synchronized with Version property)
    /// </summary>
    public string VersionString
    {
        get => _versionString;
        set
        {
            _versionString = value;
            // Try to parse the string value back to enum
            if (!string.IsNullOrEmpty(value) && value.TryToEnum<PostgreSqlVersionType>(out var versionEnum))
            {
                _version = versionEnum;
            }
        }
    }

    /// <summary>
    /// Database SKU name as string (synchronized with SkuName property)
    /// </summary>
    public string SkuNameString
    {
        get => _skuNameString;
        set
        {
            _skuNameString = value;
            // Try to parse the string value back to enum
            if (!string.IsNullOrEmpty(value) && value.TryToEnum<DatabaseSkuType>(out var skuEnum))
            {
                _skuName = skuEnum;
            }
        }
    }
}

