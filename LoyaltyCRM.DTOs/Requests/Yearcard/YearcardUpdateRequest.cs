using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using PapasCRM_API.Entities;
using PapasCRM_API.Requests.PropertyInterfaces;

namespace PapasCRM_API.Requests.Yearcard
{
    public class YearcardUpdateRequest : Request, IHasId, IHasCardId, IHasEmail, IHasPhoneNumber, IHasName, IHasUserName, IHasValidTo
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

        [Required]
        public DateTime ValidTo { get; set; }
        
        public List<ValidityIntervalEntity> ValidityIntervals { get; set; } = new List<ValidityIntervalEntity>();
    }
}