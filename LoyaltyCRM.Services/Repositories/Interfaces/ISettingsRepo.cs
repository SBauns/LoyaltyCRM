using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.Models;

namespace LoyaltyCRM.Services.Repositories.Interfaces
{
    public interface ISettingsRepo
    {
        Task<IEnumerable<AppSetting>> GetAllAsync();
        Task<AppSetting?> GetByKeyAsync(string key);
        Task<AppSetting> UpsertAsync(AppSetting setting);
        Task<bool> DeleteAsync(string key);
    }
}