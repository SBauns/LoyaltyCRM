using System;
using System.Collections.Generic;
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
                new StartDate(DateTime.UtcNow.AddDays(-1)),
                new EndDate(DateTime.UtcNow.AddDays(1)),
                null);
        }

        public static ValidityInterval CreateExpired()
        {
            return new ValidityInterval(
                new StartDate(DateTime.UtcNow.AddYears(-2)),
                new EndDate(DateTime.UtcNow.AddYears(-1)),
                null);
        }
    }
}