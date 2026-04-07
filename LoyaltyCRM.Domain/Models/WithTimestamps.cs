using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.Domain.Models
{
    public class WithTimestamps
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public void UpdateTimestamps()
        {
            UpdatedAt = DateTime.UtcNow;
            if (CreatedAt == default)
            {
                CreatedAt = DateTime.UtcNow;
            }
        }
    }
}