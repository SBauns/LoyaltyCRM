using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.DTOs.Enums;

namespace LoyaltyCRM.DTOs.Requests.Checkin
{
    public class CheckInResponse
    {
        public bool IsValid { get; set;} = false;
        public int CardId { get; set; }
        public string Name { get; set; } = "";
        public string ExpirationDate { get; set; } = "";
        public IsValidForDiscount IsValidForDiscount { get; set; } = IsValidForDiscount.NotApplicable;
    }
}