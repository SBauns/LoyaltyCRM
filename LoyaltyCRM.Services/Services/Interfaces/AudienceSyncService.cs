using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.Models;

namespace LoyaltyCRM.Services.Services.Interfaces
{
    public interface IAudienceSyncService
    {
        Task SyncUserAsync(ApplicationUser? user);
        Task DeleteUserAsync(string email);
    }
}