using System.ComponentModel.DataAnnotations;

namespace LoyaltyCRM.DTOs.Requests.Yearcard
{
    public class YearcardGetResponse
    {
        public Guid? Id { get; set; }

        public int CardId { get; set; }

        public string? PhoneNumber { get; set; }

        public string? UserName {get; set;}

        public string? Name {get; set;}
        public string? Email { get; set; }
        public DateTime ValidTo { get; set; }

        public DateTime CreatedAt { get; set;}

        public DateTime UpdatedAt { get; set;}

        public List<ValidityIntervalResponseAndRequest> ValidityIntervals { get; set; } = new List<ValidityIntervalResponseAndRequest>();
        public bool IsValidForDiscount { get; set; }
    }
}