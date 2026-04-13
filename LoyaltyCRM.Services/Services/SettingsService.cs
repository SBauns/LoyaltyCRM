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

namespace LoyaltyCRM.Services.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepo _settingsRepo;
        private readonly IAppSettingsProvider _appSettingsProvider;

        public SettingsService(
            ISettingsRepo settingsRepo,
            IAppSettingsProvider appSettingsProvider)
        {
            _settingsRepo = settingsRepo;
            _appSettingsProvider = appSettingsProvider;
        }

        public async Task<IEnumerable<AppSetting>> GetAllSettingsAsync()
        {
            var settings = await _settingsRepo.GetAllAsync();

            if (settings == null || !settings.Any())
            {
                return GetFallbackSettings();
            }

            return settings;
        }

        private IEnumerable<AppSetting> GetFallbackSettings()
        {
            var fallback = new AppSettings();

            return typeof(AppSettings)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .Select(property => new AppSetting
                {
                    Key = property.Name,
                    Value = Convert.ToString(property.GetValue(fallback)) ?? string.Empty
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .ToList();
        }

        public async Task<AppSetting> UpsertSettingAsync(string key, string value)
        {
            if (!AppSettingValidator.TryValidateSetting(key, value, out _, out var errorMessage))
            {
                throw new ArgumentException(errorMessage ?? "Invalid setting.");
            }

            var existing = await _settingsRepo.GetByKeyAsync(key);
            var setting = existing ?? new AppSetting { Key = key.Trim() };
            setting.Value = value.Trim();

            var result = await _settingsRepo.UpsertAsync(setting);
            await _appSettingsProvider.ReloadAsync();
            return result;
        }

        public async Task<bool> DeleteSettingAsync(string key)
        {
            var deleted = await _settingsRepo.DeleteAsync(key);
            if (deleted)
            {
                await _appSettingsProvider.ReloadAsync();
            }
            return deleted;
        }
    }
}