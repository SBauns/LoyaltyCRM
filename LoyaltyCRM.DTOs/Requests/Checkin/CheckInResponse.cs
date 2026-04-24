using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.DTOs.Requests.Checkin
{
    public class CheckInResponse
    {
        public bool IsValid { get; set;} = false;
        public bool IsValidForDiscount { get; set; } = false;
    }
}