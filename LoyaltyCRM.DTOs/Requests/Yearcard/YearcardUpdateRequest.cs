using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.DTOs.Requests.Yearcard
{
    public class YearcardUpdateRequest
    {
        public Guid? Id { get; set; }

        public int CardId { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Name { get; set; }

        public string? UserName { get; set; }
        
        public List<ValidityIntervalResponseAndRequest> ValidityIntervals { get; set; } = new List<ValidityIntervalResponseAndRequest>();
    }
}