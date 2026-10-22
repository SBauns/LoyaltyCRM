using LoyaltyCRM.Application.Models;
using LoyaltyCRM.Application.Services;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

public class TermsService : ITermsService
{
    private readonly LoyaltyContext dbContext;

    public TermsService(LoyaltyContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<TermsDocument?> GetCurrentAsync(CancellationToken cancellationToken)
    {
        return await dbContext.TermsDocuments
            .Where(term => term.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TermsDocument> PublishNewVersionAsync(string html, CancellationToken cancellationToken)
    {
        await dbContext.TermsDocuments
            .Where(term => term.IsCurrent)
            .ExecuteUpdateAsync(setter => setter.SetProperty(term => term.IsCurrent, false), cancellationToken);

        TermsDocument newVersion = new TermsDocument
        {
            Html = html,
            CreatedAtUtc = DateTime.UtcNow,
            IsCurrent = true
        };

        dbContext.TermsDocuments.Add(newVersion);
        await dbContext.SaveChangesAsync(cancellationToken);
        return newVersion;
    }

        public async Task<IReadOnlyList<TermsVersionSummary>> GetVersionHistoryAsync(CancellationToken cancellationToken)
    {
        return await dbContext.TermsDocuments
            .OrderByDescending(term => term.CreatedAtUtc)
            .Select(term => new TermsVersionSummary(term.Id, term.CreatedAtUtc, term.IsCurrent))
            .ToListAsync(cancellationToken);
    }

    public async Task<TermsDocument?> GetVersionAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.TermsDocuments
            .FirstOrDefaultAsync(term => term.Id == id, cancellationToken);
    }

    public async Task<TermsDocument?> SetCurrentVersionAsync(int id, CancellationToken cancellationToken)
    {
        TermsDocument? sourceVersion = await dbContext.TermsDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(term => term.Id == id, cancellationToken);

        if (sourceVersion is null)
        {
            return null;
        }

        // Restoring publishes a new row with the old content,
        // keeping the version history append-only.
        return await PublishNewVersionAsync(sourceVersion.Html, cancellationToken);
    }

    public async Task<TermsDeleteResult> DeleteVersionAsync(int id, CancellationToken cancellationToken)
    {
        TermsDocument? version = await dbContext.TermsDocuments
            .FirstOrDefaultAsync(term => term.Id == id, cancellationToken);

        if (version is null)
        {
            return TermsDeleteResult.NotFound;
        }

        if (version.IsCurrent)
        {
            return TermsDeleteResult.IsCurrentVersion;
        }

        dbContext.TermsDocuments.Remove(version);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TermsDeleteResult.Deleted;
    }
}