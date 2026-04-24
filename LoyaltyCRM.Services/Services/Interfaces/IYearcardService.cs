using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.DTOs.Requests.Checkin;
using LoyaltyCRM.DTOs.Requests.Yearcard;

namespace LoyaltyCRM.Services.Services.Interfaces
{
    public interface IYearcardService
    {
        Task<IEnumerable<YearcardGetResponse>> GetYearcards();
        Task<YearcardGetResponse> GetYearcard(Guid Id);
        Task<Yearcard> UpdateYearcard(Guid Id, YearcardUpdateRequest yearcard);
        Task<bool> DeleteYearcard(Guid Id);
        Task<YearcardCreateResponse> CreateOrExtendYearcard(YearcardCreateRequest NewYearCard);
        Task<YearcardCreateResponse> ImportYearcard(YearcardImportRequest request);
        Task<Yearcard> AddValidityToCurrentYearcard(Yearcard NewYearCard, StartDate startDate);
        Task<CheckInResponse> CheckInWithYearcards(Guid id);
        Task<CheckInResponse> CheckInWithPhone(PhoneNumber phoneNumber);
        Task<CheckInResponse> CheckInWithEmail(Email email);
        Task<CheckInResponse> CheckInWithUserName(UserName userName);
        Task<IEnumerable<Yearcard>> CheckInWithName(string fullName, int similarityThreshold = 80);
    }
}