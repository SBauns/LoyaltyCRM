using FuzzySharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PapasCRM_API.Context;
using PapasCRM_API.Requests;
using PapasCRM_API.Entities;
using PapasCRM_API.Exceptions;
using PapasCRM_API.Mappers;
using PapasCRM_API.Models;
using PapasCRM_API.Repositories.Interfaces;
using System;
using PapasCRM_API.DomainPrimitives;
using NuGet.Common;
using PapasCRM_API.Services;
using static PapasCRM_API.Services.TranslationService;


namespace PapasCRM_API.Repositories
{
    public class YearcardRepo : IYearcardRepo
    {
        private readonly BarContext _context;

        private readonly IBarMapper _mapper;

        private readonly ILogger<YearcardRepo> _logger;

        public YearcardRepo(BarContext context, IBarMapper mapper, ILogger<YearcardRepo> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<YearcardEntity>> GetYearcards()
        {
            // Eagerly load the User navigation property
            List<YearcardEntity> YearcardEntityList = await _context.Yearcards
                .Include(y => y.User)
                .Include(y => y.ValidityIntervals)
                .ToListAsync();

            List<YearcardEntity> yearcardReturnList = new List<YearcardEntity>();
            foreach (YearcardEntity yearcardEntity in YearcardEntityList)
            {
                Yearcard yearcard = _mapper.EntityToYearcard(yearcardEntity);
                // Map back to entity, but preserve the loaded User
                YearcardEntity mappedEntity = _mapper.YearcardToEntity(yearcard);
                mappedEntity.User = yearcardEntity.User;
                yearcardReturnList.Add(mappedEntity);
            }
            return yearcardReturnList;
        }

        public Task<IEnumerable<YearcardEntity>> GetUnconfirmedYearcards()
        {
            throw new NotImplementedException("This method is not implemented yet. Please implement it according to your business logic.");
            // List<YearcardEntity> YearcardEntityList = await _context.Yearcards
            //     .Where(yearcard => yearcard.IsConfirmed == false)
            //     .Include(y => y.User)
            //     .ToListAsync();

            // List<YearcardEntity> yearcardReturnList = new List<YearcardEntity>();
            // foreach (YearcardEntity yearcardEntity in YearcardEntityList)
            // {
            //     Yearcard yearcard = _mapper.EntityToYearcard(yearcardEntity);
            //     yearcard.SetValidityForDiscount();
            //     var mappedEntity = _mapper.YearcardToEntity(yearcard);
            //     mappedEntity.User = yearcardEntity.User;
            //     mappedEntity = ChooseExposedIdentification(mappedEntity);
            //     yearcardReturnList.Add(mappedEntity);
            // }
            // return yearcardReturnList;
        }

        private YearcardEntity ChooseExposedIdentification(YearcardEntity yearcardEntity)
        {
            throw new NotImplementedException("This method is not implemented yet. Please implement it according to your business logic.");
            // if (string.IsNullOrEmpty(yearcardEntity.ExposedIdentification))
            // {
            //     var user = yearcardEntity.User;
            //     string? value = null;
            //     if (user != null)
            //     {
            //         value = !string.IsNullOrEmpty(user.UserName) ? user.UserName :
            //                 !string.IsNullOrEmpty(user.PhoneNumber) ? user.PhoneNumber :
            //                 !string.IsNullOrEmpty(user.Email) ? user.Email : null;
            //     }
            //     if (string.IsNullOrEmpty(value))
            //     {
            //         value = $"{yearcardEntity.Name}";
            //     }
            //     yearcardEntity.ExposedIdentification = value;
            // }
            // return yearcardEntity;
        }

        public async Task<YearcardEntity> GetYearcard(Guid id)
        {
            var yearcardEntity = await _context.Yearcards
                .Include(y => y.User)
                .FirstOrDefaultAsync(y => y.Id == id);

            if (yearcardEntity == null)
            {
                throw new EntityNotFoundException("We could not find this yearcard");
            }

            Yearcard yearcard = _mapper.EntityToYearcard(yearcardEntity);
            // Map back to entity, but preserve the loaded User
            var mappedEntity = _mapper.YearcardToEntity(yearcard);
            mappedEntity.User = yearcardEntity.User;

            return mappedEntity;
        }

        public async Task<YearcardEntity> UpdateYearcard(Guid id, YearcardEntity yearcardEntity)
        {
            YearcardEntity? oldYearcard = _context.Yearcards
                .Include(y => y.User)
                .Include(y => y.ValidityIntervals)
                .FirstOrDefault(y => y.Id == id);

            if(oldYearcard == null){
                throw new ArgumentException(Translate("Could not find old Yearcard"));
            }

            Yearcard yearcard = _mapper.EntityToYearcard(yearcardEntity);

            oldYearcard.CardId = yearcard.CardId.GetValue();
            oldYearcard.Name = yearcard.Name?.GetValue();
            oldYearcard.User!.Email = yearcardEntity.User!.Email;
            if (oldYearcard.User.Email != null)
            {
                oldYearcard.User.NormalizedEmail = StringNormalizationExtensions.Normalize(oldYearcard.User.Email);
            }
            oldYearcard.User.PhoneNumber = yearcardEntity.User.PhoneNumber;
            oldYearcard.User.UserName = yearcardEntity.User.UserName;
            if (oldYearcard.User.UserName != null) {
                oldYearcard.User.NormalizedUserName = StringNormalizationExtensions.Normalize(oldYearcard.User.UserName); 
            }
            
            List<ValidityIntervalEntity> updatedIntervals = new List<ValidityIntervalEntity>();

            foreach (ValidityInterval validityInterval in yearcard.ValidityIntervals)
            {

                ValidityIntervalEntity newValidityIntervalEntity = new ValidityIntervalEntity();

                newValidityIntervalEntity.StartDate = validityInterval.StartDate.GetValue();
                newValidityIntervalEntity.EndDate = validityInterval.EndDate.GetValue();

                updatedIntervals.Add(newValidityIntervalEntity);
            }

            oldYearcard.ValidityIntervals = updatedIntervals;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!YearcardExists(id))
                {
                    throw new DbUpdateException(Translate("Yearcard does not exist in Database"));
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("unique index"))
                {
                    string innerMsg = ex.InnerException.Message;

                    // Example parsing logic — adjust based on your DB error message format:
                    string duplicateField = "unknown field";

                    // Try to detect which unique index or constraint caused it
                    if (innerMsg.Contains("CardId"))
                        duplicateField = "CardId";
                    else if (innerMsg.Contains("Email"))
                        duplicateField = "Email";
                    else if (innerMsg.Contains("Phone"))
                        duplicateField = "Phone";

                    throw new DbUpdateException(Translate("A duplicate entry exists for ") + $"'{duplicateField}'.", ex);
                }

                throw;
            }

            return oldYearcard;
        }

        public async Task<YearcardEntity> CreateYearcard(YearcardEntity NewYearCard, string customerId)
        {
            NewYearCard.CardId = GetNewestCardId();
            NewYearCard.UserId = customerId;
            NewYearCard.ValidityIntervals.Add(CreateValidityInterval(NewYearCard.StartDate));

            //Basically a validation check
            _mapper.EntityToYearcard(NewYearCard);

            try
            {
                NewYearCard.UpdateTimestamps();
                _context.Yearcards.Add(NewYearCard);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Check if it's a unique constraint violation
                if (ex.InnerException != null && ex.InnerException.Message.Contains("unique index"))
                {
                    throw new DbUpdateException(Translate("A duplicate entry exists"), ex);
                }

                // Re-throw other DbUpdateExceptions
                throw;
            }

            return NewYearCard;
        }

        public async Task<YearcardEntity> AddValidityToCurrentYearcard(YearcardEntity NewYearCard)
        {
            NewYearCard.ValidityIntervals.Add(CreateValidityInterval(NewYearCard.StartDate));

            NewYearCard.ValidityIntervals = NewYearCard.ValidityIntervals
                .OrderByDescending(v => v.StartDate)
                .Reverse()
                .ToList();

            DateTime LastEndDate = new DateTime();

            foreach (ValidityIntervalEntity interval in NewYearCard.ValidityIntervals)
            {
                TimeSpan timeSpan = interval.EndDate - interval.StartDate;
                if (interval.StartDate < LastEndDate)
                {
                    interval.StartDate = LastEndDate;
                    interval.EndDate = LastEndDate.Add(timeSpan); // Assuming a 1-year validity interval
                }
                LastEndDate = interval.EndDate;
            }

            await _context.SaveChangesAsync();

            return NewYearCard;
        }
        private ValidityIntervalEntity CreateValidityInterval(DateTime startDate)
        {
            if (startDate.Date <= DateTime.UtcNow.Date)
            {
                startDate = DateTime.UtcNow.Date;
                _logger.LogWarning("Start date was in the past, setting it to current UTC time.");
            }
            return new ValidityIntervalEntity
            {
                StartDate = startDate,
                EndDate = startDate.AddYears(1) // Assuming a 1-year validity interval
            };
        }

        private int GetNewestCardId()
        {
            try
            {
                return _context.Yearcards.Max(y => y.CardId) + 1;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting the newest card ID. Returning 1 as default.");
                return 1;
            }
        }

        public async Task<YearcardEntity> UnconfirmYearcard(YearcardEntity yearCard)
        {
            throw new NotImplementedException("This method is not implemented yet. Please implement it according to your business logic.");

            // yearCard.IsConfirmed = false;

            // _context.Entry(yearCard).State = EntityState.Modified;

            // try
            // {
            //     await _context.SaveChangesAsync();

            //     return yearCard;
            // }
            // catch (DbUpdateConcurrencyException)
            // {
            //     if (!YearcardExists(yearCard.Id ?? Guid.Empty))
            //     {
            //         throw new EntityNotFoundException("Yearcard not found");
            //     }
            //     else
            //     {
            //         throw;
            //     }
            // }
        }

        public async Task<bool> ConfirmYearcard(Guid id)
        {
            throw new NotImplementedException("This method is not implemented yet. Please implement it according to your business logic.");
            //TODO Consider making this a variable you can change
            // int yearsToAdd = 1;

            // YearcardEntity? yearcard = await _context.Yearcards.FindAsync(id);

            // if (yearcard != null)
            // {

            //     SetValidToDate(yearcard, yearsToAdd);

            //     yearcard.IsConfirmed = true;

            //     _context.Entry(yearcard).State = EntityState.Modified;
            // }
            // else
            // {
            //     throw new EntityNotFoundException("Yearcard not found");
            // }


            // try
            // {
            //     await _context.SaveChangesAsync();
            // }
            // catch (DbUpdateConcurrencyException)
            // {
            //     if (!YearcardExists(id))
            //     {
            //         return false;
            //     }
            //     else
            //     {
            //         throw;
            //     }
            // }

            // return true;
        }

        public async Task<bool> RejectYearcard(Guid id)
        {
            YearcardEntity? yearcardEntity = await _context.Yearcards.FindAsync(id);

            if (yearcardEntity == null)
            {
                return false;
            }

            Yearcard yearcard = _mapper.EntityToYearcard(yearcardEntity);
            if (yearcard.IsYearcardValidForDiscount())
            {
                // yearcardEntity.IsConfirmed = true;

                _context.Entry(yearcardEntity).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!YearcardExists(id))
                    {
                        return false;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                return await DeleteYearcard(id);
            }

            return false;
        }
        public async Task<bool> DeleteYearcard(Guid id)
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var yearcard = await _context.Yearcards
                    .Include(yearcard => yearcard.User)
                    .FirstOrDefaultAsync(yearcard => yearcard.Id == id);

                if (yearcard == null)
                {
                    return false;
                }

                _context.Yearcards.Remove(yearcard);
                _context.Users.Remove(yearcard.User);
                await _context.SaveChangesAsync();
                transaction.Commit();

                return true;
            }
            catch (System.Exception)
            {
                transaction.Rollback();
                return false;
            }
        }
        
        public async Task<bool> CheckInWithYearcards(Guid id)
        {
            YearcardEntity? yearcardEntity = await _context.Yearcards
                .FindAsync(id);

            return ConfirmValidityOfYearcard(yearcardEntity);
        }

        public async Task<bool> CheckInWithPhone(PhoneNumber phoneNumber)
        {
            ApplicationUserEntity? userEntity = await _context.Users
                .Where(y => y.PhoneNumber == phoneNumber.GetValue())
                .Include(y => y.Yearcard!.ValidityIntervals)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(Translate("User with this phone number not found"));

            return ConfirmValidityOfYearcard(userEntity.Yearcard);
        }
        
        public async Task<bool> CheckInWithEmail(Email email)
        {
            ApplicationUserEntity? userEntity = await _context.Users
                .Where(y => y.NormalizedEmail == StringNormalizationExtensions.Normalize(email.GetValue()))
                .Include(y => y.Yearcard!.ValidityIntervals)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(Translate("User with this email not found"));

            return ConfirmValidityOfYearcard(userEntity.Yearcard);
        }

        public async Task<bool> CheckInWithUserName(UserName userName)
        {
            ApplicationUserEntity? userEntity = await _context.Users
                .Where(y => y.NormalizedUserName == StringNormalizationExtensions.Normalize(userName.GetValue()))
                .Include(y => y.Yearcard!.ValidityIntervals)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException("User with this username not found");

            return ConfirmValidityOfYearcard(userEntity.Yearcard);
        }

        public async Task<IEnumerable<YearcardEntity>> CheckInWithName(string fullName, int similarityThreshold = 80)
        {
            // Normalize input
            var normalizedInput = fullName?.Trim().ToLower();

            if (string.IsNullOrEmpty(normalizedInput))
            {
                return Enumerable.Empty<YearcardEntity>();
            }

            // Fetch all entries
            var yearcards = await _context.Yearcards.ToListAsync();

            // Find matches based on similarity
            var matchingYearcards = yearcards.Where(y =>
            {
                var fullNameFromEntity = y.Name.ToLower();
                return Fuzz.Ratio(normalizedInput, fullNameFromEntity) >= similarityThreshold;
            });

            List<YearcardEntity> returnCards = new List<YearcardEntity>();

            foreach (var matchedYearcard in matchingYearcards)
            {
                if (ConfirmValidityOfYearcard(matchedYearcard))
                {
                    returnCards.Add(matchedYearcard);
                }
            }

            return returnCards;
        }


        private bool ConfirmValidityOfYearcard(YearcardEntity? yearcardEntity){
            if (yearcardEntity == null)
            {
                return false;
            }

            foreach (ValidityIntervalEntity interval in yearcardEntity.ValidityIntervals)
            {
                if (interval.StartDate <= DateTime.Now && interval.EndDate >= DateTime.Now)
                {
                    _logger.LogInformation($"Yearcard with ID {yearcardEntity.Id} is valid.");
                    return true;
                }
            }

            // If no valid interval found, return false
            _logger.LogWarning($"Yearcard with ID {yearcardEntity.Id} is not valid.");

            return false;
        }

        private bool YearcardExists(Guid id)
        {
            return _context.Yearcards.Any(e => e.Id == id);
        }

    }
}
