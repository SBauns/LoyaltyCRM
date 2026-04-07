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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltyCRM.Services.Services
{
    public class YearcardService : IYearcardService
    {

        private readonly IYearcardRepo _yearcardRepo;

        private readonly ICustomerRepo _customerRepo;

        private readonly LoyaltyContext _context;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;

        public YearcardService(IYearcardRepo yearcardRepo, ICustomerRepo customerRepo,LoyaltyContext context, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            _yearcardRepo = yearcardRepo;
            _customerRepo = customerRepo;
            _context = context;
            _userManager = userManager;
            _logger = logger;
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
            return await _yearcardRepo.UpdateYearcard(Id, yearcard);
        }

        public async Task<bool> DeleteYearcard(Guid Id)
        {
            return await _yearcardRepo.DeleteYearcard(Id);
        }

        public async Task<Yearcard> CreateOrExtendYearcard(Yearcard NewYearCard, StartDate startDate)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    NewYearCard.User = SetUsername(NewYearCard.User);
                    
                    ApplicationUser Customer = await _customerRepo.CreateOrReturnFirstCustomer(NewYearCard.User);
                    
                    if (Customer.Yearcard != null)
                        NewYearCard = Customer.Yearcard;
                    else
                        NewYearCard.CardId!.SetValue(_yearcardRepo.GetNewestCardId());
    
                    NewYearCard.UserId = Customer.Id;
                    NewYearCard.ValidityIntervals.Add(CreateValidityInterval(startDate.GetValue()));
                    NewYearCard.UpdateTimestamps();
            
                    await _yearcardRepo.CreateYearcard(NewYearCard);
                    return NewYearCard;
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
            NewYearCard.ValidityIntervals.Add(CreateValidityInterval(startDate.GetValue()));

            NewYearCard.ValidityIntervals = NewYearCard.ValidityIntervals
                .OrderByDescending(v => v.StartDate)
                .Reverse()
                .ToList();

            DateTime LastEndDate = new DateTime();

            foreach (ValidityInterval interval in NewYearCard.ValidityIntervals)
            {
                TimeSpan timeSpan = interval.EndDate.GetValue() - interval.StartDate.GetValue();
                if (interval.StartDate.GetValue() < LastEndDate)
                {
                    interval.StartDate = new StartDate(LastEndDate);
                    interval.EndDate = new EndDate(LastEndDate.Add(timeSpan)); // Assuming a 1-year validity interval
                }
                LastEndDate = interval.EndDate.GetValue();
            }

            await _yearcardRepo.UpdateYearcard((Guid)NewYearCard.Id!, NewYearCard);

            return NewYearCard;
        }

        public async Task<bool> CheckInWithYearcards(Guid id)
        {
            Yearcard? yearcard = await _context.Yearcards
                .FindAsync(id);

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
                var fullNameFromEntity = y.Name!.GetValue().ToLower();
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
                if (interval.StartDate.GetValue() <= DateTime.Now && interval.EndDate.GetValue() >= DateTime.Now)
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
            return new ValidityInterval(
                new StartDate(startDate),
                new EndDate(startDate.AddYears(1)), //TODO MAKE ABLE TO SET
                null
            );
        }

        private ApplicationUser SetUsername(ApplicationUser user)
        {
            
            if (user.UserName.IsNullOrEmpty())
            {
                user.UserName = user.Email ?? user.PhoneNumber;
            }
            return user;
        }
    }
}