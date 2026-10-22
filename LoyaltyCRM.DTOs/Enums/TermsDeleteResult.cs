namespace LoyaltyCRM.Application.Models;

public enum TermsDeleteResult
{
    Deleted,
    NotFound,
    IsCurrentVersion
}

public sealed record TermsVersionSummary(int Id, DateTime CreatedAtUtc, bool IsCurrent);