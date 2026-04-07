using FuzzySharp;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Exceptions;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Services.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace LoyaltyCRM.Services.Repositories
{
    public class YearcardRepo : IYearcardRepo
    {
        private readonly LoyaltyContext _context;

        private readonly ILogger<YearcardRepo> _logger;

        public YearcardRepo(LoyaltyContext context, ILogger<YearcardRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Yearcard>> GetYearcards()
        {
            // Eagerly load the User navigation property
            return await _context.Yearcards
                .Include(y => y.User)
                .Include(y => y.ValidityIntervals)
                .ToListAsync();
        }

        public async Task<Yearcard> GetYearcard(Guid id)
        {
            Yearcard? yearcard = await _context.Yearcards
                .Include(y => y.User)
                .FirstOrDefaultAsync(y => y.Id == id);

            if (yearcard == null)
            {
                throw new EntityNotFoundException("We could not find this yearcard"); //TRANSLATE
            }

            return yearcard;
        }

        public async Task<Yearcard> UpdateYearcard(Guid id, Yearcard updated)
        {
            Yearcard? existing = await _context.Yearcards
                .Include(y => y.ValidityIntervals)
                .FirstOrDefaultAsync(y => y.Id == id);

            if (existing == null)
                throw new ArgumentException("Could not find old Yearcard"); //TRANSLATE

            // Update scalar properties
            _context.Entry(existing).CurrentValues.SetValues(updated);

            // --- Update ValidityIntervals (diff-based) ---

            // 1. Remove intervals that no longer exist
            foreach (var oldInterval in existing.ValidityIntervals.ToList())
            {
                if (!updated.ValidityIntervals.Any(u => u.Id == oldInterval.Id))
                {
                    _context.ValidityInterval.Remove(oldInterval);
                }
            }

            // 2. Add or update intervals
            foreach (var newInterval in updated.ValidityIntervals)
            {
                var existingInterval = existing.ValidityIntervals
                    .FirstOrDefault(v => v.Id == newInterval.Id);

                if (existingInterval == null)
                {
                    // New interval → add it
                    existing.ValidityIntervals.Add(new ValidityInterval(
                        newInterval.StartDate,
                        newInterval.EndDate,
                        newInterval.Id
                    ));
                }
                else
                {
                    // Existing interval → update it
                    _context.Entry(existingInterval).CurrentValues.SetValues(newInterval);
                }
            }

            await _context.SaveChangesAsync();
            return existing;
        }


        public async Task<Yearcard> CreateYearcard(Yearcard NewYearCard)
        {
            try
            {
                _context.Yearcards.Add(NewYearCard);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Check if it's a unique constraint violation
                if (ex.InnerException != null && ex.InnerException.Message.Contains("unique index"))
                {
                    throw new DbUpdateException("A duplicate entry exists", ex); //Translate
                }

                // Re-throw other DbUpdateExceptions
                throw;
            }

            return NewYearCard;
        }

        public int GetNewestCardId()
        {
            try
            {
                return _context.Yearcards.Max(y => y.CardId!.GetValue()) + 1;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting the newest card ID. Returning 1 as default.");
                return 1;
            }
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
    }
}
