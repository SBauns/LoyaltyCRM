using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Domain.DomainPrimitives;

namespace LoyaltyCRM.Services.Repositories.Interfaces
{
    public interface IYearcardRepo
    {
        Task<IEnumerable<Yearcard>> GetYearcards();
        Task<Yearcard> GetYearcard(Guid id);
        Task<Yearcard> UpdateYearcard(Guid id, Yearcard updated);
        Task<Yearcard> CreateYearcard(Yearcard NewYearCard);
        int GetNewestCardId();
        Task<bool> DeleteYearcard(Guid id);
    }
}