using System;
using System.Collections.Generic;
using System.Data;
using FuzzySharp;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.DTOs.Requests.Checkin;
using LoyaltyCRM.DTOs.Requests.Yearcard;
using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Services.Repositories.Interfaces;
using LoyaltyCRM.Services.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoyaltyCRM.Services.Services
{
    public class YearcardService : IYearcardService
    {

        private readonly IYearcardRepo _yearcardRepo;

        private readonly ICustomerRepo _customerRepo;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<YearcardService> _logger;
        private readonly ITransactionService _transactionService;
        private readonly IAppSettingsProvider _appSettingsProvider;
        private readonly IAudienceSyncService _audienceSyncService;

        public YearcardService(
            IYearcardRepo yearcardRepo, 
            ICustomerRepo customerRepo,
            UserManager<ApplicationUser> userManager, 
            ILogger<YearcardService> logger,
            ITransactionService transactionService,
            IAppSettingsProvider appSettingsProvider,
            IAudienceSyncService audienceSyncService)
        {
            _yearcardRepo = yearcardRepo;
            _customerRepo = customerRepo;
            _userManager = userManager;
            _logger = logger;
            _transactionService = transactionService;
            _appSettingsProvider = appSettingsProvider;
            _audienceSyncService = audienceSyncService;
        }

        public async Task<IEnumerable<YearcardGetResponse>> GetYearcards()
        {
            var yearcards = await _yearcardRepo.GetYearcards();
            foreach (var yearcard in yearcards)
            {
                yearcard.SetIsYearcardValidForDiscount(_appSettingsProvider.Current.DiscountGracePeriodInDays);
            }
            return yearcards.Adapt<IEnumerable<YearcardGetResponse>>();
        }

        public async Task<YearcardGetResponse> GetYearcard(Guid Id)
        {
            var yearcard = await _yearcardRepo.GetYearcard(Id);
            yearcard.SetIsYearcardValidForDiscount(_appSettingsProvider.Current.DiscountGracePeriodInDays);
            return yearcard.Adapt<YearcardGetResponse>();
        }

        public async Task<Yearcard> UpdateYearcard(Guid Id, YearcardUpdateRequest yearcard)
        {
            Yearcard updatedYearcard = yearcard.Adapt<Yearcard>();

            using (var transaction = await _transactionService.BeginTransactionAsync())
            {
                try
                {
                    Yearcard existingYearcard = await _yearcardRepo.GetYearcard(Id);

                    UpdateYearcardGraph(existingYearcard, updatedYearcard);
                    existingYearcard.UpdateTimestamps();

                    var result = await _userManager.UpdateAsync(existingYearcard.User);
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Failed to update yearcard and user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    await transaction.CommitAsync();
                    await _audienceSyncService.SyncUserAsync(existingYearcard.User);
                    return existingYearcard;
                }
                catch (DbUpdateException e)
                {
                    await transaction.RollbackAsync();
                    throw new DbUpdateException("translation.yearcard.update_failed");
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Data invalid. {exception.Message}");
                }
            }
        }

        private static void UpdateYearcardGraph(Yearcard existing, Yearcard updated)
        {
            if (updated.CardId != null)
            {
                existing.CardId = updated.CardId;
            }

            if (updated.User != null)
            {
                UpdateUser(existing.User, updated.User);
            }

            if (updated != null)
            {
                UpdateYearcard(existing, updated);
            }

            UpdateValidityIntervals(existing, updated.ValidityIntervals);
        }

        private static void UpdateYearcard(Yearcard existingCard, Yearcard updatedYearcard)
        {
            existingCard.CardId = updatedYearcard.CardId;
            existingCard.Name = updatedYearcard.Name;
        }

        private static void UpdateUser(ApplicationUser existingUser, ApplicationUser updatedUser)
        {
            existingUser.UserName = updatedUser.UserName;
            existingUser.NormalizedUserName = updatedUser.NormalizedUserName;
            existingUser.Email = updatedUser.Email;
            existingUser.NormalizedEmail = updatedUser.NormalizedEmail;
            existingUser.PhoneNumber = updatedUser.PhoneNumber;
            existingUser.EmailConfirmed = updatedUser.EmailConfirmed;
            existingUser.PhoneNumberConfirmed = updatedUser.PhoneNumberConfirmed;
            existingUser.LockoutEnabled = updatedUser.LockoutEnabled;
            existingUser.LockoutEnd = updatedUser.LockoutEnd;
            existingUser.AccessFailedCount = updatedUser.AccessFailedCount;
            existingUser.TwoFactorEnabled = updatedUser.TwoFactorEnabled;
            existingUser.PasswordHash = updatedUser.PasswordHash;
            existingUser.SecurityStamp = updatedUser.SecurityStamp;
            existingUser.ConcurrencyStamp = updatedUser.ConcurrencyStamp;
        }

        private static void UpdateValidityIntervals(Yearcard existing, List<ValidityInterval> updatedIntervals)
        {
            updatedIntervals ??= new List<ValidityInterval>();

            foreach (var oldInterval in existing.ValidityIntervals.ToList())
            {
                if (!updatedIntervals.Any(u => u.Id == oldInterval.Id))
                {
                    existing.ValidityIntervals.Remove(oldInterval);
                }
            }

            foreach (var newInterval in updatedIntervals)
            {
                var existingInterval = existing.ValidityIntervals
                    .FirstOrDefault(v => v.Id == newInterval.Id);

                if (existingInterval == null)
                {
                    existing.ValidityIntervals.Add(new ValidityInterval(
                        newInterval.StartDate,
                        newInterval.EndDate,
                        newInterval.Id
                    ));
                }
                else
                {
                    existingInterval.StartDate = newInterval.StartDate;
                    existingInterval.EndDate = newInterval.EndDate;
                }
            }
        }

        public async Task<bool> DeleteYearcard(Guid Id)
        {
            Yearcard yearcard = await _yearcardRepo.GetYearcard(Id);
            if(yearcard != null && yearcard.User != null && yearcard.User.Email != null)
                await _audienceSyncService.DeleteUserAsync(yearcard.User.Email);
            bool succes = await _yearcardRepo.DeleteYearcard(Id);
            return succes;
        }

        public async Task<YearcardCreateResponse> CreateOrExtendYearcard(YearcardCreateRequest request)
        {
            Yearcard NewYearCard = request.Adapt<Yearcard>();
            StartDate startDate = new StartDate(request.StartDate);

            using (var transaction = await _transactionService.BeginTransactionAsync())
            {
                try
                {
                    NewYearCard.User = SetUsername(NewYearCard.User);
                    
                    ApplicationUser Customer = await _customerRepo.CreateOrReturnFirstCustomer(NewYearCard.User);
                    Yearcard createdYearcard = null;
                    
                    if(Customer.Yearcard == null)
                    {
                        NewYearCard.CardId = new CardNumber(_yearcardRepo.GetNewestCardId());
                        NewYearCard.UserId = Customer.Id;
                        NewYearCard.ValidityIntervals.Add(CreateValidityInterval(startDate.Value));
                        NewYearCard.UpdateTimestamps();
                        createdYearcard = await _yearcardRepo.CreateYearcard(NewYearCard);
                        Customer.Yearcard = createdYearcard;
                        await _audienceSyncService.SyncUserAsync(Customer);
                    }
                    else if (Customer.Yearcard != null) //Means we found a existing customer
                    {
                        createdYearcard = await AddValidityToCurrentYearcard(Customer.Yearcard, startDate);
                    }

                    await transaction.CommitAsync();
                    if(createdYearcard != null){
                        return createdYearcard.Adapt<YearcardCreateResponse>();
                    }
                    return null;
                }
                catch (Exception exception)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"Data invalid. {exception.Message}");
                    }
                }
        }

        public async Task<YearcardCreateResponse> ImportYearcard(YearcardImportRequest request)
        {
            Yearcard NewYearCard = request.Adapt<Yearcard>();
            StartDate startDate = new StartDate(request.StartDate);

            using (var transaction = await _transactionService.BeginTransactionAsync())
            {
                try
                {
                    NewYearCard.User = SetUsername(NewYearCard.User);
                    
                    ApplicationUser Customer = await _customerRepo.CreateOrReturnFirstCustomer(NewYearCard.User);
                    Yearcard createdYearcard = null;
                    
                    if(Customer.Yearcard == null)
                    {
                        NewYearCard.CardId = new CardNumber(NewYearCard.CardId != null ? NewYearCard.CardId.Value : _yearcardRepo.GetNewestCardId());
                        NewYearCard.UserId = Customer.Id;
                        NewYearCard.ValidityIntervals.Add(CreateValidityInterval(startDate.Value, request.ValidTo != null ? new EndDate((DateTime)request.ValidTo) : null));
                        NewYearCard.UpdateTimestamps();
                        createdYearcard = await _yearcardRepo.CreateYearcard(NewYearCard);
                    }
                    else if (Customer.Yearcard != null) //Means we found a existing customer
                    {
                        throw new DataException("translation.user.already_created");
                    }
    
                    await transaction.CommitAsync();
                    if(createdYearcard != null){
                        return createdYearcard.Adapt<YearcardCreateResponse>();
                    }
                    return null;
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Data invalid. {exception.Message}");
                }
            }
        }

        public async Task<Yearcard> AddValidityToCurrentYearcard(Yearcard NewYearCard, StartDate startDate)
        {
            NewYearCard.ValidityIntervals.Add(CreateValidityInterval(startDate.Value));

            NewYearCard.ValidityIntervals = NewYearCard.ValidityIntervals
                .OrderBy(v => v.StartDate.Value)
                .ToList();

            DateTime LastEndDate = new DateTime();

            foreach (ValidityInterval interval in NewYearCard.ValidityIntervals)
            {
                TimeSpan timeSpan = interval.EndDate.Value - interval.StartDate.Value;
                if (interval.StartDate.Value < LastEndDate)
                {
                    interval.StartDate = new StartDate(LastEndDate);
                    interval.EndDate = new EndDate(LastEndDate.Add(timeSpan));
                }
                LastEndDate = interval.EndDate.Value;
            }

            NewYearCard.UpdateTimestamps();
            await _yearcardRepo.UpdateYearcard((Guid)NewYearCard.Id!, NewYearCard);

            return NewYearCard;
        }

        public async Task<CheckInResponse> CheckInWithYearcards(Guid id)
        {
            Yearcard? yearcard = await _yearcardRepo.GetYearcard(id);

            return CreateCheckInResponse(yearcard);
        }

        public async Task<CheckInResponse> CheckInWithPhone(PhoneNumber phoneNumber)
        {
            ApplicationUser user = await _customerRepo.GetUserByPhone(phoneNumber);

            return CreateCheckInResponse(user.Yearcard);
        }
        
        public async Task<CheckInResponse> CheckInWithEmail(Email email)
        {
            ApplicationUser? user = await _customerRepo.GetUserByEmail(email);

            return CreateCheckInResponse(user.Yearcard);
        }

        public async Task<CheckInResponse> CheckInWithUserName(UserName userName)
        {
            ApplicationUser? user = await _customerRepo.GetUserByUserName(userName);

            return CreateCheckInResponse(user.Yearcard);
        }

        public async Task<IEnumerable<Yearcard>> CheckInWithName(string fullName, int similarityThreshold = 80)
        {
            // Normalize input
            var normalizedInput = fullName?.Trim().ToLower();

            if (string.IsNullOrEmpty(normalizedInput))
            {
                return Enumerable.Empty<Yearcard>();
            }

            // Fetch all entries
            var yearcards = await _yearcardRepo.GetYearcards();

            // Find matches based on similarity
            var matchingYearcards = yearcards.Where(y =>
            {
                var fullNameFromEntity = y.Name!.Value.ToLower();
                return Fuzz.Ratio(normalizedInput, fullNameFromEntity) >= similarityThreshold;
            });

            List<Yearcard> returnCards = new List<Yearcard>();

            foreach (var matchedYearcard in matchingYearcards)
            {
                if (ConfirmValidityOfYearcard(matchedYearcard))
                {
                    matchedYearcard.SetIsYearcardValidForDiscount(_appSettingsProvider.Current.DiscountGracePeriodInDays);
                    returnCards.Add(matchedYearcard);
                }
            }

            return returnCards;
        }

        private CheckInResponse CreateCheckInResponse(Yearcard yearcard)
        {
            if (yearcard == null)
            {
                throw new ArgumentException("yearcard.not_found");
            }
            yearcard.SetIsYearcardValidForDiscount(_appSettingsProvider.Current.DiscountGracePeriodInDays);
            return new CheckInResponse()
            {
                IsValid = ConfirmValidityOfYearcard(yearcard),
                IsValidForDiscount = yearcard.IsValidForDiscount
            };
        }

        private bool ConfirmValidityOfYearcard(Yearcard? yearcard){
            if (yearcard == null)
            {
                return false;
            }

            foreach (ValidityInterval interval in yearcard.ValidityIntervals)
            {
                if (interval.StartDate.Value <= DateTime.Now && interval.EndDate.Value >= DateTime.Now)
                {
                    _logger.LogInformation($"Yearcard with ID {yearcard.Id} is valid.");
                    return true;
                }
            }

            // If no valid interval found, return false
            _logger.LogWarning($"Yearcard with ID {yearcard.Id} is not valid.");

            return false;
        }

        private ValidityInterval CreateValidityInterval(DateTime startDate, EndDate? endDate = null)
        {
            if (startDate.Date <= DateTime.UtcNow.Date)
            {
                startDate = DateTime.UtcNow.Date;
                _logger.LogWarning("Start date was in the past, setting it to current UTC time.");
            }

            var validityDays = _appSettingsProvider.Current.LengthOfYearcardInDays;
            return new ValidityInterval(
                new StartDate(startDate),
                endDate != null ? endDate : new EndDate(startDate.AddDays(validityDays)),
                null
            );
        }

        private ApplicationUser SetUsername(ApplicationUser user)
        {
            
            if (String.IsNullOrEmpty(user.UserName))
            {
                user.UserName = user.Email ?? user.PhoneNumber;
            }
            return user;
        }
    }
}