using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class EndDate
    {
        public DateTime Value { get; }

        public EndDate(DateTime Value)
        {
            ValidateDate(Value);
            this.Value = Value;
        }

        private void ValidateDate(DateTime Value)
        {
            
        }
    }
}