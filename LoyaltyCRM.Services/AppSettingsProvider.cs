using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Services.Repositories.Interfaces;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyCRM.Services
{
    public class AppSettingsProvider : IAppSettingsProvider
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly object _sync = new();
        private AppSettings _current = new();

        public AppSettingsProvider(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        public AppSettings Current
        {
            get
            {
                lock (_sync)
                {
                    return _current;
                }
            }
        }

        public async Task InitializeAsync()
        {
            await ReloadAsync();
        }

        public async Task ReloadAsync()
        {
            var settings = await LoadSettingsAsync();
            var values = BuildAppSettings(settings);

            lock (_sync)
            {
                _current = values;
            }
        }

        private async Task<IEnumerable<AppSetting>> LoadSettingsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ISettingsRepo>();
            return await repo.GetAllAsync();
        }

        private AppSettings BuildAppSettings(IEnumerable<AppSetting> settings)
        {
            var values = new AppSettings();
            var dictionary = settings?
                .Where(s => !string.IsNullOrWhiteSpace(s.Key))
                .ToDictionary(s => s.Key.Trim(), s => s.Value?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            return ApplyOverrides(values, dictionary);
        }

        private AppSettings ApplyOverrides(AppSettings baseSettings, Dictionary<string, string> overrides)
        {
            foreach (var property in typeof(AppSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead || !property.CanWrite)
                {
                    continue;
                }

                if (!overrides.TryGetValue(property.Name, out var rawValue))
                {
                    continue;
                }

                if (TryConvertPropertyValue(property.PropertyType, rawValue, out var converted))
                {
                    property.SetValue(baseSettings, converted);
                }
            }

            return baseSettings;
        }

        private static bool TryConvertPropertyValue(Type propertyType, string rawValue, out object? converted)
        {
            converted = null;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            if (propertyType == typeof(string))
            {
                converted = rawValue.Trim();
                return true;
            }

            var converter = TypeDescriptor.GetConverter(propertyType);
            if (converter != null && converter.IsValid(rawValue))
            {
                converted = converter.ConvertFromString(rawValue);
                return true;
            }

            if (propertyType.IsEnum)
            {
                try
                {
                    converted = Enum.Parse(propertyType, rawValue, ignoreCase: true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
    }
}
