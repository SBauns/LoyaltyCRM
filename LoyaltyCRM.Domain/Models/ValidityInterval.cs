using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PapasCRM_API.DomainPrimitives;

namespace PapasCRM_API.Models
{
    public class ValidityInterval
    {
        public Guid? Id { get; }
        public StartDate StartDate { get; set; }
        public EndDate EndDate { get; set; }

        public ValidityInterval(Guid? id, StartDate startDate, EndDate? endDate)
        {
            Id = id;
            StartDate = startDate;
            EndDate = endDate ?? new EndDate(startDate.GetValue().AddYears(1)); // Default to one year validity from start date
        }
    }
}