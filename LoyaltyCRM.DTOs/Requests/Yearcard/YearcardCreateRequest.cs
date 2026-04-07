using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PapasCRM_API.Requests.PropertyInterfaces;

namespace PapasCRM_API.Requests.Yearcard
{
    public class YearcardCreateRequest : Request, IHasEmail, IHasPhoneNumber, IHasName, IHasUserName, IHasStartDate
    {

        [MaxLength(50)]
        [Required]
        public string? Email { get; set; }

        [MaxLength(50)]
        [Required]
        public string? PhoneNumber { get; set; }

        [MaxLength(50)]
        [Required]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? UserName { get; set; }
        public DateTime StartDate { get; set; }

    }
}