using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FuzzySharp;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Services.Repositories.Interfaces;
using LoyaltyCRM.Services.Services.Interfaces;
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

        public YearcardService(
            IYearcardRepo yearcardRepo, 
            ICustomerRepo customerRepo,
            UserManager<ApplicationUser> userManager, 
            ILogger<YearcardService> logger,
            ITransactionService transactionService,
            IAppSettingsProvider appSettingsProvider)
        {
            _yearcardRepo = yearcardRepo;
            _customerRepo = customerRepo;
            _userManager = userManager;
            _logger = logger;
            _transactionService = transactionService;
            _appSettingsProvider = appSettingsProvider;
        }

        public async Task<IEnumerable<Yearcard>> GetYearcards()
        {
            return await _yearcardRepo.GetYearcards();
        }

        public async Task<Yearcard> GetYearcard(Guid Id)
        {
            return await _yearcardRepo.GetYearcard(Id);
        }

        public async Task<Yearcard> UpdateYearcard(Guid Id, Yearcard yearcard)
        {
            using (var transaction = await _transactionService.BeginTransactionAsync())
            {
                try
                {
                    Yearcard existingYearcard = await _yearcardRepo.GetYearcard(Id);

                    UpdateYearcardGraph(existingYearcard, yearcard);

                    var result = await _userManager.UpdateAsync(existingYearcard.User);
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Failed to update yearcard and user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    await transaction.CommitAsync();
                    return existingYearcard;
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
            if (updated.Name != null)
            {
                existing.Name = updated.Name;
            }

            if (updated.CardId != null)
            {
                existing.CardId = updated.CardId;
            }

            if (updated.User != null)
            {
                UpdateUser(existing.User, updated.User);
                existing.User.Yearcard = existing;
            }

            UpdateValidityIntervals(existing, updated.ValidityIntervals);
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
            return await _yearcardRepo.DeleteYearcard(Id);
        }

        public async Task<Yearcard> CreateOrExtendYearcard(Yearcard NewYearCard, StartDate startDate, bool ShouldExtend = true)
        {
            using (var transaction = await _transactionService.BeginTransactionAsync())
            {
                try
                {
                    NewYearCard.User = SetUsername(NewYearCard.User);
                    
                    ApplicationUser Customer = await _customerRepo.CreateOrReturnFirstCustomer(NewYearCard.User);
                    Yearcard createdYearcard;
                    
                    if (Customer.Yearcard != null && ShouldExtend) //Means we found a existing customer
                    {
                        createdYearcard = await AddValidityToCurrentYearcard(Customer.Yearcard, startDate);
                    }
                    else
                    {
                        NewYearCard.CardId = new CardNumber(_yearcardRepo.GetNewestCardId());
                        NewYearCard.UserId = Customer.Id;
                        NewYearCard.ValidityIntervals.Add(CreateValidityInterval(startDate.Value));
                        NewYearCard.UpdateTimestamps();
                        createdYearcard = await _yearcardRepo.CreateYearcard(NewYearCard);
                    }
    
                    await transaction.CommitAsync();
                    return createdYearcard;
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

        public async Task<bool> CheckInWithYearcards(Guid id)
        {
            Yearcard? yearcard = await _yearcardRepo.GetYearcard(id);

            return ConfirmValidityOfYearcard(yearcard);
        }

        public async Task<bool> CheckInWithPhone(PhoneNumber phoneNumber)
        {
            ApplicationUser user = await _customerRepo.GetUserByPhone(phoneNumber);

            return ConfirmValidityOfYearcard(user.Yearcard);
        }
        
        public async Task<bool> CheckInWithEmail(Email email)
        {
            ApplicationUser? user = await _customerRepo.GetUserByEmail(email);

            return ConfirmValidityOfYearcard(user.Yearcard);
        }

        public async Task<bool> CheckInWithUserName(UserName userName)
        {
            ApplicationUser? user = await _customerRepo.GetUserByUserName(userName);

            return ConfirmValidityOfYearcard(user.Yearcard);
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
                    returnCards.Add(matchedYearcard);
                }
            }

            return returnCards;
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
                    _logger.LogInformation($"Yearcard with ID {yearcard.Id} is valid."); //TRANSLATE
                    return true;
                }
            }

            // If no valid interval found, return false
            _logger.LogWarning($"Yearcard with ID {yearcard.Id} is not valid."); //TRANSLATE

            return false;
        }

        private ValidityInterval CreateValidityInterval(DateTime startDate)
        {
            if (startDate.Date <= DateTime.UtcNow.Date)
            {
                startDate = DateTime.UtcNow.Date;
                _logger.LogWarning("Start date was in the past, setting it to current UTC time.");
            }

            var validityDays = _appSettingsProvider.Current.LengthOfYearcardInDays;
            return new ValidityInterval(
                new StartDate(startDate),
                new EndDate(startDate.AddDays(validityDays)),
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