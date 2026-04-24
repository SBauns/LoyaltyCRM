using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.DTOs.DTOs
{
    public class TemplateInfo
    {
        public string Name { get; set; }
        public string Source { get; set; } // "Mandrill" or "Mailchimp"
    }
}