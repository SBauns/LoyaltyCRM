using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PapasCRM_API.DomainPrimitives
{
    public class StartDate
    {
        private DateTime value;

        public StartDate(DateTime value)
        {
            ValidateDate(value);
            this.value = value;
        }

        public DateTime GetValue()
        {
            return this.value;
        }

        private void ValidateDate(DateTime value)
        {
            // if (DateTime.Now.Date > value.Date)
            // {
            //     throw new ArgumentException("Valid date must be later than today");
            // }
        }
    }
}