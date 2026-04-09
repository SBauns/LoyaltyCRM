using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.DomainPrimitives;

namespace LoyaltyCRM.Domain.Models
{
    public class ValidityInterval
    {
        public Guid? Id { get; }
        public StartDate StartDate { get; set; }
        public EndDate EndDate { get; set; }

        //Relationship
        public Guid YearcardEntityId { get; set; }
        public Yearcard Yearcard { get; set; } 

        public ValidityInterval(StartDate startDate, EndDate? endDate, Guid? id)
        {
            Id = id;
            StartDate = startDate;
            EndDate = endDate ?? new EndDate(startDate.Value.AddYears(1)); // Default to one year validity from start date
        }
    }
}