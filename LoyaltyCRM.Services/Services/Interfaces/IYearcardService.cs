using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;

namespace LoyaltyCRM.Services.Services.Interfaces
{
    public interface IYearcardService
    {
        Task<IEnumerable<Yearcard>> GetYearcards();
        Task<Yearcard> GetYearcard(Guid Id);
        Task<Yearcard> UpdateYearcard(Guid Id, Yearcard yearcard);
        Task<bool> DeleteYearcard(Guid Id);
        Task<Yearcard> CreateOrExtendYearcard(Yearcard NewYearCard, StartDate startDate);
        Task<Yearcard> AddValidityToCurrentYearcard(Yearcard NewYearCard, StartDate startDate);
        Task<bool> CheckInWithYearcards(Guid id);
        Task<bool> CheckInWithPhone(PhoneNumber phoneNumber);
        Task<bool> CheckInWithEmail(Email email);
        Task<bool> CheckInWithUserName(UserName userName);
        Task<IEnumerable<Yearcard>> CheckInWithName(string fullName, int similarityThreshold = 80);
    }
}