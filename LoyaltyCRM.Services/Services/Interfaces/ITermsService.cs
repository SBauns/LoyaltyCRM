using LoyaltyCRM.Application.Models;
using LoyaltyCRM.Domain.Models;

namespace LoyaltyCRM.Application.Services;

public interface ITermsService
{
    Task<TermsDocument?> GetCurrentAsync(CancellationToken cancellationToken);
    Task<TermsDocument> PublishNewVersionAsync(string html, CancellationToken cancellationToken);
    Task<IReadOnlyList<TermsVersionSummary>> GetVersionHistoryAsync(CancellationToken cancellationToken);
    Task<TermsDocument?> GetVersionAsync(int id, CancellationToken cancellationToken);
    Task<TermsDocument?> SetCurrentVersionAsync(int id, CancellationToken cancellationToken);
    Task<TermsDeleteResult> DeleteVersionAsync(int id, CancellationToken cancellationToken);
}