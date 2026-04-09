using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.DTOs.Requests.PropertyInterfaces;

namespace LoyaltyCRM.DTOs.Requests.Yearcard
{
    public class YearcardUpdateRequest : Request, IHasId, IHasCardId, IHasEmail, IHasPhoneNumber, IHasName, IHasUserName
    {
        public Guid? Id { get; set; }

        [Required]
        [Range(1, 100000)]
        public int CardId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [MaxLength(50)]
        public string? Name { get; set; }

        public string? UserName { get; set; }
        
        public List<ValidityIntervalResponseAndRequest> ValidityIntervals { get; set; } = new List<ValidityIntervalResponseAndRequest>();
    }
}