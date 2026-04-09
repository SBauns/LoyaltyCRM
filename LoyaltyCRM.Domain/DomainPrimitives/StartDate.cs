using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class StartDate
    {
        public DateTime Value { get; }

        public StartDate(DateTime Value)
        {
            ValidateDate(Value);
            this.Value = Value;
        }

        private void ValidateDate(DateTime Value)
        {
            // if (DateTime.Now.Date > Value.Date)
            // {
            //     throw new ArgumentException("Valid date must be later than today");
            // }
        }
    }
}