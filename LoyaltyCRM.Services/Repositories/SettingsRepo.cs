using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Services.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyCRM.Services.Repositories
{
    public class SettingsRepo : ISettingsRepo
    {
        private readonly LoyaltyContext _context;

        public SettingsRepo(LoyaltyContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppSetting>> GetAllAsync()
        {
            return await _context.Settings.AsNoTracking().ToListAsync();
        }

        public async Task<AppSetting?> GetByKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            return await _context.Settings.FindAsync(key);
        }

        public async Task<AppSetting> UpsertAsync(AppSetting setting)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            var existing = await _context.Settings.FindAsync(setting.Key);
            if (existing is null)
            {
                _context.Settings.Add(setting);
            }
            else
            {
                existing.Value = setting.Value;
            }

            await _context.SaveChangesAsync();
            return existing ?? setting;
        }

        public async Task<bool> DeleteAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            var existing = await _context.Settings.FindAsync(key);
            if (existing is null)
            {
                return false;
            }

            _context.Settings.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}