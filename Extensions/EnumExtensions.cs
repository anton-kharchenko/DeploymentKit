using DeploymentKit.Enums;
using System.ComponentModel;
using System.Reflection;

namespace DeploymentKit.Extensions
{
    /// <summary>
    /// Extension methods for enum types to provide string conversion and utility methods.
    /// These extensions maintain backward compatibility while providing type safety.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the string value from the Description attribute of an enum value.
        /// If no Description attribute is found, returns the enum name as string.
        /// </summary>
        /// <param name="enumValue">The enum value to convert.</param>
        /// <returns>The string representation of the enum value.</returns>
        public static string ToStringValue(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var descriptionAttribute = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute?.Description ?? enumValue.ToString();
        }

        /// <summary>
        /// Converts a string value to the corresponding enum value using Description attributes.
        /// </summary>
        /// <typeparam name="T">The enum type to convert to.</typeparam>
        /// <param name="stringValue">The string value to convert.</param>
        /// <param name="ignoreCase">Whether to ignore case when comparing strings.</param>
        /// <returns>The corresponding enum value.</returns>
        /// <exception cref="ArgumentException">Thrown when the string value doesn't match any enum value.</exception>
        private static T ToEnum<T>(this string stringValue, bool ignoreCase = true) where T : Enum
        {
            var enumType = typeof(T);
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
                var enumValue = (T)field.GetValue(null)!;

                // Check description attribute first
                if (descriptionAttribute != null &&
                    string.Equals(descriptionAttribute.Description, stringValue, comparison))
                {
                    return enumValue;
                }

                // Fallback to enum name
                if (string.Equals(field.Name, stringValue, comparison))
                {
                    return enumValue;
                }
            }

            throw new ArgumentException($"Unable to convert '{stringValue}' to enum type {enumType.Name}", nameof(stringValue));
        }

        /// <summary>
        /// Tries to convert a string value to the corresponding enum value.
        /// </summary>
        /// <typeparam name="T">The enum type to convert to.</typeparam>
        /// <param name="stringValue">The string value to convert.</param>
        /// <param name="result">The resulting enum value if conversion succeeds.</param>
        /// <param name="ignoreCase">Whether to ignore case when comparing strings.</param>
        /// <returns>True if conversion succeeds, false otherwise.</returns>
        public static bool TryToEnum<T>(this string stringValue, out T result, bool ignoreCase = true) where T : Enum
        {
            try
            {
                result = stringValue.ToEnum<T>(ignoreCase);
                return true;
            }
            catch
            {
                result = default(T)!;
                return false;
            }
        }

        /// <summary>
        /// Converts an AzureLocationType enum to its Azure region string value.
        /// </summary>
        /// <param name="locationType">The Azure location type.</param>
        /// <returns>The Azure region string (e.g., "eastus", "westeurope").</returns>
        public static string ToAzureRegion(this AzureLocationType locationType)
        {
            return locationType.ToStringValue();
        }

        /// <summary>
        /// Parses a string to an AzureLocationType enum with fuzzy matching.
        /// Supports common variations and abbreviations.
        /// </summary>
        /// <param name="location">The location string to parse.</param>
        /// <returns>The corresponding AzureLocationType.</returns>
        /// <exception cref="ArgumentException">Thrown when the location string cannot be parsed.</exception>
        public static AzureLocationType ParseAzureLocation(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException("Location cannot be null or empty", nameof(location));
            }

            var normalized = location.ToLowerInvariant().Replace(" ", "").Replace("-", "").Replace("_", "");

            if (TryToEnum<AzureLocationType>(normalized, out var result))
            {
                return result;
            }

            var fuzzyMatches = new Dictionary<string, AzureLocationType>(StringComparer.OrdinalIgnoreCase)
            {
                ["eastus"] = AzureLocationType.EastUS,
                ["eastus2"] = AzureLocationType.EastUS2,
                ["westus"] = AzureLocationType.WestUS,
                ["westus2"] = AzureLocationType.WestUS2,
                ["westus3"] = AzureLocationType.WestUS3,
                ["centralus"] = AzureLocationType.CentralUS,
                ["northcentralus"] = AzureLocationType.NorthCentralUS,
                ["southcentralus"] = AzureLocationType.SouthCentralUS,
                ["westcentralus"] = AzureLocationType.WestCentralUS,
                ["northeurope"] = AzureLocationType.NorthEurope,
                ["westeurope"] = AzureLocationType.WestEurope,
                ["germanywestcentral"] = AzureLocationType.GermanyWestCentral,
                ["germanynorth"] = AzureLocationType.GermanyNorth,
                ["ukwest"] = AzureLocationType.UKWest,
                ["uksouth"] = AzureLocationType.UKSouth,
                ["francecentral"] = AzureLocationType.FranceCentral,
                ["francesouth"] = AzureLocationType.FranceSouth,
                ["switzerlandnorth"] = AzureLocationType.SwitzerlandNorth,
                ["switzerlandwest"] = AzureLocationType.SwitzerlandWest,
                ["norwayeast"] = AzureLocationType.NorwayEast,
                ["norwaywest"] = AzureLocationType.NorwayWest,
                ["swedencentral"] = AzureLocationType.SwedenCentral,
                ["southeastasia"] = AzureLocationType.SoutheastAsia,
                ["eastasia"] = AzureLocationType.EastAsia,
                ["australiaeast"] = AzureLocationType.AustraliaEast,
                ["australiasoutheast"] = AzureLocationType.AustraliaSoutheast,
                ["australiacentral"] = AzureLocationType.AustraliaCentral,
                ["japaneast"] = AzureLocationType.JapanEast,
                ["japanwest"] = AzureLocationType.JapanWest,
                ["koreacentral"] = AzureLocationType.KoreaCentral,
                ["koreasouth"] = AzureLocationType.KoreaSouth,
                ["canadacentral"] = AzureLocationType.CanadaCentral,
                ["canadaeast"] = AzureLocationType.CanadaEast,
                ["brazilsouth"] = AzureLocationType.BrazilSouth,
                ["southafricanorth"] = AzureLocationType.SouthAfricaNorth,
                ["southafricawest"] = AzureLocationType.SouthAfricaWest,
                ["uaenorth"] = AzureLocationType.UAENorth,
                ["uaecentral"] = AzureLocationType.UAECentral,
                ["centralindia"] = AzureLocationType.CentralIndia,
                ["southindia"] = AzureLocationType.SouthIndia,
                ["westindia"] = AzureLocationType.WestIndia,
                ["polandcentral"] = AzureLocationType.PolandCentral,
                ["northgermany"] = AzureLocationType.GermanyWestCentral,
                ["germany"] = AzureLocationType.GermanyWestCentral,
            };

            if (fuzzyMatches.TryGetValue(normalized, out var match))
            {
                return match;
            }

            throw new ArgumentException($"Unable to parse location '{location}' to a valid Azure region. " +
                $"Please use a valid Azure region name like 'eastus', 'westeurope', 'germanywestcentral', etc.", 
                nameof(location));
        }

        /// <summary>
        /// Converts an EnvironmentType enum to its string value.
        /// </summary>
        /// <param name="environment">The environment type.</param>
        /// <returns>The environment string (e.g., "development", "production").</returns>
        public static string ToEnvironmentString(this EnvironmentType environment)
        {
            return environment.ToStringValue();
        }

        /// <summary>
        /// Parses a string to an EnvironmentType enum.
        /// </summary>
        /// <param name="environment">The environment string to parse.</param>
        /// <returns>The corresponding EnvironmentType.</returns>
        /// <exception cref="ArgumentException">Thrown when the environment string cannot be parsed.</exception>
        public static EnvironmentType ParseEnvironmentType(string environment)
        {
            if (string.IsNullOrWhiteSpace(environment))
            {
                throw new ArgumentException("Environment cannot be null or empty", nameof(environment));
            }

            var normalized = environment.ToLowerInvariant().Replace(" ", "").Replace("-", "").Replace("_", "");

            var environmentMap = new Dictionary<string, EnvironmentType>(StringComparer.OrdinalIgnoreCase)
            {
                ["development"] = EnvironmentType.Development,
                ["dev"] = EnvironmentType.Development,
                ["production"] = EnvironmentType.Production,
                ["prod"] = EnvironmentType.Production,
            };

            if (environmentMap.TryGetValue(normalized, out var result))
            {
                return result;
            }

            if (TryToEnum<EnvironmentType>(normalized, out var enumResult))
            {
                return enumResult;
            }

            throw new ArgumentException($"Unable to parse environment '{environment}' to a valid EnvironmentType. " +
                $"Supported values: development, dev, production, prod", 
                nameof(environment));
        }
    }
}

