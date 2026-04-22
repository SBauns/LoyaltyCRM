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
        public int? CardId { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Name { get; set; }

        public string? UserName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}