using System.Text.RegularExpressions;

namespace DeploymentKit.Constants;

public static class AzureResourceNamingConstraints
{
    public static class StorageAccount
    {
        private const int MinLength = 3;
        public const int MaxLength = 24;
        private const string Pattern = "^[a-z0-9]+$";
        public const string Description = "Storage account names must be 3-24 characters, lowercase letters and numbers only";
        public static bool IsValid(string name) => !string.IsNullOrEmpty(name) && name.Length is >= MinLength and <= MaxLength && Regex.IsMatch(name, Pattern);
    }

    public static class LogAnalyticsWorkspace
    {
        private const int MinLength = 4;
        private const int MaxLength = 63;
        private const string Pattern = "^[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9]$";
        public const string Description = "Log Analytics Workspace names must be 4-63 characters, alphanumeric and hyphens only, cannot start/end with hyphen";
        public static bool IsValid(string name) => !string.IsNullOrEmpty(name) && name.Length is >= MinLength and <= MaxLength && Regex.IsMatch(name, Pattern);
    }

    public static class ContainerRegistry
    {
        private const int MinLength = 5;
        private const int MaxLength = 50;
        private const string Pattern = "^[a-zA-Z0-9]+$";
        public const string Description = "Container Registry names must be 5-50 characters, alphanumeric only";
        public static bool IsValid(string name) => !string.IsNullOrEmpty(name) && name.Length is >= MinLength and <= MaxLength && Regex.IsMatch(name, Pattern);
    }

    public static class KeyVault
    {
        private const int MinLength = 3;
        public const int MaxLength = 24;
        private const string Pattern = "^[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9]$";
        public const string Description = "Key Vault names must be 3-24 characters, alphanumeric and hyphens, must start with letter, cannot end with hyphen";
        public static bool IsValid(string name) => !string.IsNullOrEmpty(name) && name.Length is >= MinLength and <= MaxLength && Regex.IsMatch(name, Pattern);
    }

    public static class ResourceGroup
    {
        private const int MinLength = 1;
        private const int MaxLength = 90;
        private const string Pattern = @"^[a-zA-Z0-9_\-\.()]+[a-zA-Z0-9_\-()]$";
        public const string Description = "Resource Group names must be 1-90 characters, alphanumeric, underscores, hyphens, periods, and parentheses, cannot end with period";
        public static bool IsValid(string name) => !string.IsNullOrEmpty(name) && name.Length >= MinLength && name.Length <= MaxLength && Regex.IsMatch(name, Pattern);
    }

    public static class VirtualNetwork
    {
        private const int MinLength = 2;
        private const int MaxLength = 64;
        private const string Pattern = @"^[a-zA-Z0-9][a-zA-Z0-9_\-.]*[a-zA-Z0-9_]$";
        public const string Description = "Virtual Network names must be 2-64 characters, alphanumeric, underscores, hyphens, and periods";
        public static bool IsValid(string name) => !string.IsNullOrEmpty(name) && name.Length is >= MinLength and <= MaxLength && Regex.IsMatch(name, Pattern);
    }

    public static class Subnet
    {
        private const int MinLength = 1;
        private const int MaxLength = 80;
        private const string Pattern = @"^[a-zA-Z0-9][a-zA-Z0-9_\-.]*[a-zA-Z0-9_]$";
        public const string Description = "Subnet names must be 1-80 characters, alphanumeric, underscores, hyphens, and periods";
        public static bool IsValid(string name) => !string.IsNullOrEmpty(name) && name.Length is >= MinLength and <= MaxLength && Regex.IsMatch(name, Pattern);
    }

    public static class PostgreSqlServer
    {
        private const int MinLength = 3;
        private const int MaxLength = 63;
        private const string Pattern = "^[a-z0-9][a-z0-9-]*[a-z0-9]$";
        public const string Description = "PostgreSQL Server names must be 3-63 characters, lowercase letters, numbers, and hyphens, cannot start/end with hyphen";
        public static bool IsValid(string name) => !string.IsNullOrEmpty(name) && name.Length is >= MinLength and <= MaxLength && Regex.IsMatch(name, Pattern);
    }

    public static class RedisCache
    {
        private const int MinLength = 1;
        private const int MaxLength = 63;
        private const string Pattern = "^[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9]$";
        public const string Description = "Redis Cache names must be 1-63 characters, alphanumeric and hyphens, cannot start/end with hyphen";
        public static bool IsValid(string name) => !string.IsNullOrEmpty(name) && name.Length is >= MinLength and <= MaxLength && Regex.IsMatch(name, Pattern);
    }

    public static class ContainerApp
    {
        private const int MinLength = 2;
        private const int MaxLength = 32;
        private const string Pattern = "^[a-z0-9][a-z0-9-]*[a-z0-9]$";
        public const string Description = "Container App names must be 2-32 characters, lowercase letters, numbers, and hyphens, cannot start/end with hyphen";
        public static bool IsValid(string name) => !string.IsNullOrEmpty(name) && name.Length is >= MinLength and <= MaxLength && Regex.IsMatch(name, Pattern);
    }

    public static class ApplicationInsights
    {
        private const int MinLength = 1;
        private const int MaxLength = 255;
        private const string Pattern = @"^[a-zA-Z0-9][a-zA-Z0-9_\-\.()\[\]]*$";
        public const string Description = "Application Insights names must be 1-255 characters, alphanumeric and special characters";
        public static bool IsValid(string name) => !string.IsNullOrEmpty(name) && name.Length is >= MinLength and <= MaxLength && Regex.IsMatch(name, Pattern);
    }
}


