using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LoyaltyCRM.DTOs.Requests.Yearcard
{
    public class YearcardCreateRequest
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