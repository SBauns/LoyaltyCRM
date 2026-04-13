using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.Models;

namespace LoyaltyCRM.Services.Services.Interfaces
{
    public interface ISettingsService
    {
        Task<IEnumerable<AppSetting>> GetAllSettingsAsync();
        Task<AppSetting> UpsertSettingAsync(string key, string value);
        Task<bool> DeleteSettingAsync(string key);
    }
}