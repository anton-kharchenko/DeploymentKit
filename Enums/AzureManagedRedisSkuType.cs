using System.ComponentModel;

namespace DeploymentKit.Enums;

/// <summary>
/// Defines the available SKU types for Azure Managed Redis
/// </summary>
public enum AzureManagedRedisSkuType
{
    // Enterprise tier - Backward compatible with older Pulumi API versions
    /// <summary>
    /// Enterprise E1 - 1 GB memory, designed for dev/test scenarios (GA)
    /// </summary>
    [Description("Enterprise_E1")]
    Enterprise_E1,

    /// <summary>
    /// Enterprise E10 - 12 GB memory
    /// </summary>
    [Description("Enterprise_E10")]
    Enterprise_E10,

    /// <summary>
    /// Enterprise E20 - 25 GB memory
    /// </summary>
    [Description("Enterprise_E20")]
    Enterprise_E20,

    // Memory Optimized - High memory-to-core ratio (1:8)
    /// <summary>
    /// Memory Optimized M10 - 13 GB memory, 2 vCPUs
    /// </summary>
    [Description("MemoryOptimized_M10")]
    MemoryOptimized_M10,

    /// <summary>
    /// Memory Optimized M20 - 24 GB memory, 4 vCPUs
    /// </summary>
    [Description("MemoryOptimized_M20")]
    MemoryOptimized_M20,

    /// <summary>
    /// Memory Optimized M50 - 60 GB memory, 8 vCPUs
    /// </summary>
    [Description("MemoryOptimized_M50")]
    MemoryOptimized_M50,

    /// <summary>
    /// Memory Optimized M100 - 120 GB memory, 16 vCPUs
    /// </summary>
    [Description("MemoryOptimized_M100")]
    MemoryOptimized_M100,

    /// <summary>
    /// Memory Optimized M150 - 180 GB memory, 24 vCPUs (Preview)
    /// </summary>
    [Description("MemoryOptimized_M150")]
    MemoryOptimized_M150,

    /// <summary>
    /// Memory Optimized M250 - 240 GB memory, 32 vCPUs (Preview)
    /// </summary>
    [Description("MemoryOptimized_M250")]
    MemoryOptimized_M250,

    /// <summary>
    /// Memory Optimized M350 - 360 GB memory, 48 vCPUs (Preview)
    /// </summary>
    [Description("MemoryOptimized_M350")]
    MemoryOptimized_M350,

    /// <summary>
    /// Memory Optimized M500 - 480 GB memory, 64 vCPUs (Preview)
    /// </summary>
    [Description("MemoryOptimized_M500")]
    MemoryOptimized_M500,

    /// <summary>
    /// Memory Optimized M700 - 720 GB memory, 96 vCPUs (Preview)
    /// </summary>
    [Description("MemoryOptimized_M700")]
    MemoryOptimized_M700,

    /// <summary>
    /// Memory Optimized M1000 - 960 GB memory, 128 vCPUs (Preview)
    /// </summary>
    [Description("MemoryOptimized_M1000")]
    MemoryOptimized_M1000,

    /// <summary>
    /// Memory Optimized M1500 - 1,440 GB memory, 192 vCPUs (Preview)
    /// </summary>
    [Description("MemoryOptimized_M1500")]
    MemoryOptimized_M1500,

    /// <summary>
    /// Memory Optimized M2000 - 1,920 GB memory, 256 vCPUs (Preview)
    /// </summary>
    [Description("MemoryOptimized_M2000")]
    MemoryOptimized_M2000,

    // Balanced - Balanced CPU-to-memory ratio (1:4)
    /// <summary>
    /// Balanced B0 - 1 GB memory, 2 vCPUs
    /// </summary>
    [Description("Balanced_B0")]
    Balanced_B0,

    /// <summary>
    /// Balanced B1 - 2 GB memory, 2 vCPUs
    /// </summary>
    [Description("Balanced_B1")]
    Balanced_B1,

    /// <summary>
    /// Balanced B3 - 3 GB memory, 2 vCPUs
    /// </summary>
    [Description("Balanced_B3")]
    Balanced_B3,

    /// <summary>
    /// Balanced B5 - 6 GB memory, 2 vCPUs
    /// </summary>
    [Description("Balanced_B5")]
    Balanced_B5,

    /// <summary>
    /// Balanced B10 - 12 GB memory, 4 vCPUs
    /// </summary>
    [Description("Balanced_B10")]
    Balanced_B10,

    /// <summary>
    /// Balanced B20 - 24 GB memory, 8 vCPUs
    /// </summary>
    [Description("Balanced_B20")]
    Balanced_B20,

    /// <summary>
    /// Balanced B50 - 60 GB memory, 16 vCPUs
    /// </summary>
    [Description("Balanced_B50")]
    Balanced_B50,

    /// <summary>
    /// Balanced B100 - 120 GB memory, 32 vCPUs
    /// </summary>
    [Description("Balanced_B100")]
    Balanced_B100,

    /// <summary>
    /// Balanced B150 - 180 GB memory, 48 vCPUs (Preview)
    /// </summary>
    [Description("Balanced_B150")]
    Balanced_B150,

    /// <summary>
    /// Balanced B250 - 240 GB memory, 64 vCPUs (Preview)
    /// </summary>
    [Description("Balanced_B250")]
    Balanced_B250,

    /// <summary>
    /// Balanced B350 - 360 GB memory, 96 vCPUs (Preview)
    /// </summary>
    [Description("Balanced_B350")]
    Balanced_B350,

    /// <summary>
    /// Balanced B500 - 480 GB memory, 128 vCPUs (Preview)
    /// </summary>
    [Description("Balanced_B500")]
    Balanced_B500,

    /// <summary>
    /// Balanced B700 - 720 GB memory, 192 vCPUs (Preview)
    /// </summary>
    [Description("Balanced_B700")]
    Balanced_B700,

    /// <summary>
    /// Balanced B1000 - 960 GB memory, 256 vCPUs (Preview)
    /// </summary>
    [Description("Balanced_B1000")]
    Balanced_B1000,

    // Compute Optimized - High CPU-to-memory ratio (1:2)
    /// <summary>
    /// Compute Optimized X3 - 3 GB memory, 4 vCPUs
    /// </summary>
    [Description("ComputeOptimized_X3")]
    ComputeOptimized_X3,

    /// <summary>
    /// Compute Optimized X5 - 6 GB memory, 8 vCPUs
    /// </summary>
    [Description("ComputeOptimized_X5")]
    ComputeOptimized_X5,

    /// <summary>
    /// Compute Optimized X10 - 12 GB memory, 16 vCPUs
    /// </summary>
    [Description("ComputeOptimized_X10")]
    ComputeOptimized_X10,

    /// <summary>
    /// Compute Optimized X20 - 24 GB memory, 32 vCPUs
    /// </summary>
    [Description("ComputeOptimized_X20")]
    ComputeOptimized_X20,

    /// <summary>
    /// Compute Optimized X50 - 60 GB memory, 80 vCPUs
    /// </summary>
    [Description("ComputeOptimized_X50")]
    ComputeOptimized_X50,

    /// <summary>
    /// Compute Optimized X100 - 120 GB memory, 160 vCPUs
    /// </summary>
    [Description("ComputeOptimized_X100")]
    ComputeOptimized_X100,

    /// <summary>
    /// Compute Optimized X150 - 180 GB memory, 240 vCPUs (Preview)
    /// </summary>
    [Description("ComputeOptimized_X150")]
    ComputeOptimized_X150,

    /// <summary>
    /// Compute Optimized X250 - 240 GB memory, 320 vCPUs (Preview)
    /// </summary>
    [Description("ComputeOptimized_X250")]
    ComputeOptimized_X250,

    /// <summary>
    /// Compute Optimized X350 - 360 GB memory, 320 vCPUs (Preview)
    /// </summary>
    [Description("ComputeOptimized_X350")]
    ComputeOptimized_X350,

    /// <summary>
    /// Compute Optimized X500 - 480 GB memory, 320 vCPUs (Preview)
    /// </summary>
    [Description("ComputeOptimized_X500")]
    ComputeOptimized_X500,

    /// <summary>
    /// Compute Optimized X700 - 720 GB memory, 320 vCPUs (Preview)
    /// </summary>
    [Description("ComputeOptimized_X700")]
    ComputeOptimized_X700,

    // Flash Optimized - RAM + NVMe storage (Preview)
    /// <summary>
    /// Flash Optimized A250 - 256 GB, 8 vCPUs (Preview)
    /// </summary>
    [Description("FlashOptimized_A250")]
    FlashOptimized_A250,

    /// <summary>
    /// Flash Optimized A500 - 512 GB, 16 vCPUs (Preview)
    /// </summary>
    [Description("FlashOptimized_A500")]
    FlashOptimized_A500,

    /// <summary>
    /// Flash Optimized A700 - 723 GB, 24 vCPUs (Preview)
    /// </summary>
    [Description("FlashOptimized_A700")]
    FlashOptimized_A700,

    /// <summary>
    /// Flash Optimized A1000 - 1,024 GB, 32 vCPUs (Preview)
    /// </summary>
    [Description("FlashOptimized_A1000")]
    FlashOptimized_A1000,

    /// <summary>
    /// Flash Optimized A1500 - 1,536 GB, 48 vCPUs (Preview)
    /// </summary>
    [Description("FlashOptimized_A1500")]
    FlashOptimized_A1500,

    /// <summary>
    /// Flash Optimized A2000 - 2,048 GB, 64 vCPUs (Preview)
    /// </summary>
    [Description("FlashOptimized_A2000")]
    FlashOptimized_A2000,

    /// <summary>
    /// Flash Optimized A3000 - 3,072 GB, 96 vCPUs (Preview)
    /// </summary>
    [Description("FlashOptimized_A3000")]
    FlashOptimized_A3000,

    /// <summary>
    /// Flash Optimized A4500 - 4,723 GB, 144 vCPUs (Preview)
    /// </summary>
    [Description("FlashOptimized_A4500")]
    FlashOptimized_A4500
}

