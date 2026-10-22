namespace LoyaltyCRM.Domain.Models;

public class TermsDocument
{
    public int Id { get; set; }
    public string Html { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public bool IsCurrent { get; set; }
}