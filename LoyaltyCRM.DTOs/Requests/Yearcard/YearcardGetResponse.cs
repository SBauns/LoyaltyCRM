using System.ComponentModel.DataAnnotations;

namespace LoyaltyCRM.DTOs.Requests.Yearcard
{
    public class YearcardGetResponse
    {
        public Guid? Id { get; set; }

        [Range(1, 100000)]
        public int CardId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        // [MaxLength(50)]
        // public string? FirstName {get; set;}

        [MaxLength(50)]
        public string? UserName {get; set;}

        [MaxLength(50)]
        public string? Name {get; set;}
        public string? Email { get; set; }
        public DateTime ValidTo { get; set; }

        public DateTime CreatedAt { get; set;}

        public DateTime UpdatedAt { get; set;}

        public List<ValidityIntervalResponseAndRequest> ValidityIntervals { get; set; } = new List<ValidityIntervalResponseAndRequest>();
        public bool IsValidForDiscount { get; set; }
    }
}