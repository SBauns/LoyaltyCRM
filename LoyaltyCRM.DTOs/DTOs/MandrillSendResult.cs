using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.DTOs.DTOs
{
    public class MandrillSendResult
    {
        public string Email { get; set; }
        public string Status { get; set; } // sent, queued, rejected, invalid
        public string Reject_Reason { get; set; }
        public string Id { get; set; }
    }
}