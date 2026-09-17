using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available SKU types for Azure PostgreSQL Flexible Server
/// </summary>
public enum DatabaseSkuType
{
    /// <summary>
    /// Burstable B1ms - 1 vCore, 2 GB RAM
    /// </summary>
    [Description("Standard_B1ms")]
    StandardB1ms,

    /// <summary>
    /// Burstable B2s - 2 vCores, 4 GB RAM
    /// </summary>
    [Description("Standard_B2s")]
    StandardB2s,

    /// <summary>
    /// Burstable B2ms - 2 vCores, 8 GB RAM
    /// </summary>
    [Description("Standard_B2ms")]
    StandardB2ms,

    /// <summary>
    /// Burstable B4ms - 4 vCores, 16 GB RAM
    /// </summary>
    [Description("Standard_B4ms")]
    StandardB4ms,

    /// <summary>
    /// Burstable B8ms - 8 vCores, 32 GB RAM
    /// </summary>
    [Description("Standard_B8ms")]
    StandardB8ms,

    /// <summary>
    /// Burstable B12ms - 12 vCores, 48 GB RAM
    /// </summary>
    [Description("Standard_B12ms")]
    StandardB12ms,

    /// <summary>
    /// Burstable B16ms - 16 vCores, 64 GB RAM
    /// </summary>
    [Description("Standard_B16ms")]
    StandardB16ms,

    /// <summary>
    /// Burstable B20ms - 20 vCores, 80 GB RAM
    /// </summary>
    [Description("Standard_B20ms")]
    StandardB20ms,

    /// <summary>
    /// General Purpose D2s_v3 - 2 vCores, 8 GB RAM
    /// </summary>
    [Description("Standard_D2s_v3")]
    StandardD2sV3,

    /// <summary>
    /// General Purpose D4s_v3 - 4 vCores, 16 GB RAM
    /// </summary>
    [Description("Standard_D4s_v3")]
    StandardD4sV3,

    /// <summary>
    /// General Purpose D8s_v3 - 8 vCores, 32 GB RAM
    /// </summary>
    [Description("Standard_D8s_v3")]
    StandardD8sV3,

    /// <summary>
    /// General Purpose D16s_v3 - 16 vCores, 64 GB RAM
    /// </summary>
    [Description("Standard_D16s_v3")]
    StandardD16sV3,

    /// <summary>
    /// General Purpose D32s_v3 - 32 vCores, 128 GB RAM
    /// </summary>
    [Description("Standard_D32s_v3")]
    StandardD32sV3,

    /// <summary>
    /// General Purpose D48s_v3 - 48 vCores, 192 GB RAM
    /// </summary>
    [Description("Standard_D48s_v3")]
    StandardD48sV3,

    /// <summary>
    /// General Purpose D64s_v3 - 64 vCores, 256 GB RAM
    /// </summary>
    [Description("Standard_D64s_v3")]
    StandardD64sV3,

    /// <summary>
    /// Memory Optimized E2s_v3 - 2 vCores, 16 GB RAM
    /// </summary>
    [Description("Standard_E2s_v3")]
    StandardE2sV3,

    /// <summary>
    /// Memory Optimized E4s_v3 - 4 vCores, 32 GB RAM
    /// </summary>
    [Description("Standard_E4s_v3")]
    StandardE4sV3,

    /// <summary>
    /// Memory Optimized E8s_v3 - 8 vCores, 64 GB RAM
    /// </summary>
    [Description("Standard_E8s_v3")]
    StandardE8sV3,

    /// <summary>
    /// Memory Optimized E16s_v3 - 16 vCores, 128 GB RAM
    /// </summary>
    [Description("Standard_E16s_v3")]
    StandardE16sV3,

    /// <summary>
    /// Memory Optimized E32s_v3 - 32 vCores, 256 GB RAM
    /// </summary>
    [Description("Standard_E32s_v3")]
    StandardE32sV3,

    /// <summary>
    /// Memory Optimized E48s_v3 - 48 vCores, 384 GB RAM
    /// </summary>
    [Description("Standard_E48s_v3")]
    StandardE48sV3,

    /// <summary>
    /// Memory Optimized E64s_v3 - 64 vCores, 432 GB RAM
    /// </summary>
    [Description("Standard_E64s_v3")]
    StandardE64sV3
}
