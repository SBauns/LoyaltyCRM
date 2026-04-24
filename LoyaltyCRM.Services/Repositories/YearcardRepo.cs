using LoyaltyCRM.Domain.Exceptions;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Services.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
                .Include(y => y.ValidityIntervals)
                .FirstOrDefaultAsync(y => y.Id == id);

            if (yearcard == null)
            {
                throw new EntityNotFoundException("translation.yearcard.not_found");
            }

            return yearcard;
        }

        public async Task<Yearcard> UpdateYearcard(Guid id, Yearcard updated)
        {
            Yearcard? existing = await _context.Yearcards
                .Include(y => y.User)
                .Include(y => y.ValidityIntervals)
                .FirstOrDefaultAsync(y => y.Id == id);

            if (existing == null)
                throw new ArgumentException("translation.yearcard.not_found");

            // Update scalar properties on the yearcard
            _context.Entry(existing).CurrentValues.SetValues(updated);

            if (updated.User != null)
            {
                _context.Entry(existing.User).CurrentValues.SetValues(updated.User);
                existing.User!.Yearcard = existing;
            }

            var updatedValidityIntervals = updated.ValidityIntervals ?? new List<ValidityInterval>();

            // --- Update ValidityIntervals (diff-based) ---
            foreach (var oldInterval in existing.ValidityIntervals.ToList())
            {
                if (!updatedValidityIntervals.Any(u => u.Id == oldInterval.Id))
                {
                    _context.ValidityInterval.Remove(oldInterval);
                }
            }

            foreach (var newInterval in updatedValidityIntervals)
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
                if (ex.InnerException?.Message.Contains("unique index") == true)
                {
                    var message = ex.InnerException.Message;

                    string indexName = null;
                    string duplicateValue = null;

                    var indexMatch = System.Text.RegularExpressions.Regex.Match(message, @"index '(.+?)'");
                    if (indexMatch.Success)
                        indexName = indexMatch.Groups[1].Value;

                    var valueMatch = System.Text.RegularExpressions.Regex.Match(message, @"\((.*?)\)");
                    if (valueMatch.Success)
                        duplicateValue = valueMatch.Groups[1].Value;

                    throw new DbUpdateException(
                        $"Duplicate entry detected. Index: {indexName}, Value: {duplicateValue}",
                        ex);
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
                var latest = _context.Yearcards
                    .OrderByDescending(y => y.CardId)
                    .Select(y => y.CardId)
                    .FirstOrDefault();

                return (latest.Value) + 1;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting the newest card ID. Returning 1 as default.");
                return 1;
            }
        }

        public async Task<bool> DeleteYearcard(Guid id)
        {
            var yearcard = await _context.Yearcards
                .Include(yearcard => yearcard.User)
                .FirstOrDefaultAsync(yearcard => yearcard.Id == id);

            if (yearcard == null)
            {
                return false;
            }

            _context.Users.Remove(yearcard.User);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
