using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;

namespace LoyaltyCRM.Infrastructure.Factories
{
    public static class ValidityFactory
    {
        public static ValidityInterval CreateValid()
        {
            return new ValidityInterval(
                new StartDate(DateTime.UtcNow),
                new EndDate(DateTime.UtcNow.AddYears(1)),
                null);
        }

        public static ValidityInterval CreateExpired()
        {
            return new ValidityInterval(
                new StartDate(DateTime.UtcNow.AddYears(-2)),
                new EndDate(DateTime.UtcNow.AddYears(-1)),
                null);
        }

        [ExcludeFromCodeCoverage]
        public static List<ValidityInterval> CreateMany(int count, bool valid = true)
        {
            List<ValidityInterval> returnList = new List<ValidityInterval>();
            for (int i = 0; i < count; i++)
            {
                if (valid)
                    returnList.Add(CreateValid());
                else
                    returnList.Add(CreateExpired());                    
            }
            return returnList;
        }
    }
}