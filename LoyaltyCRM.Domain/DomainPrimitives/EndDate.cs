using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class EndDate
    {
        public DateTime Value { get; }

        private int thresholdDays = 30;

        public EndDate(DateTime Value)
        {
            ValidateDate(Value);
            this.Value = Value;
        }

        public bool DetermineIfEligibleForDiscount()
        {
            // Define the threshold date 30 days before the validTo date
            DateTime thresholdDate = Value.AddDays(thresholdDays);

            // Check if today is within the 30-day window
            return DateTime.Now <= thresholdDate;
        }

        private void ValidateDate(DateTime Value)
        {
            //TODO Validation rules: Format
        }
    }
}