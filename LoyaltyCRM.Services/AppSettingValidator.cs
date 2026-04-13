using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LoyaltyCRM.Services
{
    public static class AppSettingValidator
    {
        private static readonly Regex KeyPattern = new("^[a-zA-Z0-9_]+$", RegexOptions.Compiled);
        private static readonly Regex PositiveIntegerPattern = new("^[0-9]+$", RegexOptions.Compiled);

        private static readonly IReadOnlyDictionary<string, SettingDefinition> Definitions = typeof(AppSettings)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(
                prop => prop.Name,
                prop => new SettingDefinition(prop.Name, $"Typed value for {prop.Name}.", GetParser(prop.PropertyType)),
                StringComparer.OrdinalIgnoreCase);

        public static bool TryValidateSetting(string key, string value, out object? parsedValue, out string? errorMessage)
        {
            parsedValue = null;
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(key))
            {
                errorMessage = "Setting key is required.";
                return false;
            }

            if (!KeyPattern.IsMatch(key))
            {
                errorMessage = "Setting key may only contain letters, numbers, and underscores.";
                return false;
            }

            if (value is null)
            {
                errorMessage = "Setting value is required.";
                return false;
            }

            if (!Definitions.TryGetValue(key, out var definition))
            {
                errorMessage = $"Unsupported setting key '{key}'. Add a definition for it in AppSettingValidator.";
                return false;
            }

            return definition.TryParse(value, out parsedValue, out errorMessage);
        }

        private static TryParseDelegate GetParser(Type propertyType)
        {
            return propertyType == typeof(int)
                ? TryParsePositiveInt
                : propertyType == typeof(TimeOnly)
                    ? TryParseTimeOnly
                    : TryParseString;
        }

        private static bool TryParseString(string rawValue, out object? parsedValue, out string? errorMessage)
        {
            parsedValue = rawValue?.Trim();
            errorMessage = null;
            return true;
        }

        private static bool TryParsePositiveInt(string rawValue, out object? parsedValue, out string? errorMessage)
        {
            parsedValue = null;
            errorMessage = null;

            var trimmedValue = rawValue.Trim();
            if (!PositiveIntegerPattern.IsMatch(trimmedValue))
            {
                errorMessage = "Value must be a positive integer without special characters.";
                return false;
            }

            if (!int.TryParse(trimmedValue, out var result))
            {
                errorMessage = "Value must be a valid integer.";
                return false;
            }

            if (result <= 0)
            {
                errorMessage = "Value must be greater than zero.";
                return false;
            }

            parsedValue = result;
            return true;
        }

        private static bool TryParseTimeOnly(string rawValue, out object? parsedValue, out string? errorMessage)
        {
            parsedValue = null;
            errorMessage = null;

            var trimmedValue = rawValue.Trim();
            if (!TimeOnly.TryParse(trimmedValue, CultureInfo.InvariantCulture, out var result))
            {
                errorMessage = "Value must be a valid time of day, for example 02:00.";
                return false;
            }

            parsedValue = result;
            return true;
        }

        private sealed record SettingDefinition(string Key, string Description, TryParseDelegate TryParse);

        private delegate bool TryParseDelegate(string rawValue, out object? parsedValue, out string? errorMessage);
    }
}