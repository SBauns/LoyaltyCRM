using Microsoft.AspNetCore.Mvc;
using PapasCRM_API.Requests;
using PapasCRM_API.Entities;
using PapasCRM_API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PapasCRM_API.DomainPrimitives;

namespace PapasCRM_API.Repositories.Interfaces
{
    public interface IYearcardRepo
    {
        Task<IEnumerable<YearcardEntity>> GetYearcards();
        Task<IEnumerable<YearcardEntity>> GetUnconfirmedYearcards();
        Task<YearcardEntity> GetYearcard(Guid id);
        Task<YearcardEntity> UpdateYearcard(Guid id, YearcardEntity yearcard);
        Task<YearcardEntity> CreateYearcard(YearcardEntity yearcard, string customerId);
        Task<YearcardEntity> AddValidityToCurrentYearcard(YearcardEntity NewYearCard);
        Task<YearcardEntity> UnconfirmYearcard(YearcardEntity yearcard);
        Task<bool> ConfirmYearcard(Guid id);
        Task<bool> RejectYearcard(Guid id);
        Task<bool> DeleteYearcard(Guid id);
        Task<bool> CheckInWithYearcards(Guid id);
        Task<bool> CheckInWithPhone(PhoneNumber phoneNumber);
        Task<bool> CheckInWithEmail(Email email);
        Task<bool> CheckInWithUserName(UserName userName);

        Task<IEnumerable<YearcardEntity>> CheckInWithName(string fullName, int similarityThreshold = 80);
    }
}