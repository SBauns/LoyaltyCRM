using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.Domain.Models
{
    public class AppSetting
    {
        [Key]
        public string Key { get; set; } = default!;

        [Required]
        public string Value { get; set; } = default!;
    }
}