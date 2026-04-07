using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class EndDate
    {
        private DateTime value;

        private int thresholdDays = 30;

        public EndDate(DateTime value)
        {
            ValidateDate(value);
            this.value = value;
        }

        public DateTime GetValue()
        {
            return this.value;
        }

        public bool DetermineIfEligibleForDiscount()
        {
            // Define the threshold date 30 days before the validTo date
            DateTime thresholdDate = value.AddDays(thresholdDays);

            // Check if today is within the 30-day window
            return DateTime.Now <= thresholdDate;
        }

        private void ValidateDate(DateTime value)
        {
            //TODO Validation rules: Format
        }
    }
}