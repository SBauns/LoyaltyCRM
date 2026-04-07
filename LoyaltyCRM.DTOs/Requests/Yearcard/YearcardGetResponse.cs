using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PapasCRM_API.Entities;
using PapasCRM_API.Requests.PropertyInterfaces;

namespace PapasCRM_API.Requests.Yearcard
{
    public class YearcardGetResponse : IHasId, IHasPhoneNumber, IHasCardId, IHasFirstName, IHasName, IHasValidTo, IHasEmail, IHasIsValidForDiscount
    {
        public Guid? Id { get; set; }

        [Range(1, 100000)]
        public int CardId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [MaxLength(50)]
        public string? FirstName {get; set;}

        [MaxLength(50)]
        public string? UserName {get; set;}

        [MaxLength(50)]
        public string? Name {get; set;}
        public string? Email { get; set; }
        public DateTime ValidTo { get; set; }

        public List<ValidityIntervalResponseAndRequest> ValidityIntervals { get; set; } = new List<ValidityIntervalResponseAndRequest>();
        public bool IsValidForDiscount { get; set; }
    }
}