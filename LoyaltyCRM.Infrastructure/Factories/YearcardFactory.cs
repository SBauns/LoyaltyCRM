using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Bogus.DataSets;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;

namespace LoyaltyCRM.Infrastructure.Factories
{

public static class YearcardFactory
{
    private static readonly Faker<Yearcard> Faker = new Faker<Yearcard>()
        .CustomInstantiator(f => new Yearcard(
            Guid.NewGuid(),
            new CardNumber(f.Random.Int(1, 100000)),
            new Domain.DomainPrimitives.Name(f.Name.FirstName())
        ));

    public static Yearcard Create(ApplicationUser user, Action<Yearcard>? customizer = null)
    {
        // Create YearcardEntity with default fake data
        var yearcard = Faker.Clone()
            .RuleFor(y => y.UserId, _ => user.Id)
            .RuleFor(y => y.User, _ => user)
            .Generate();


        // Apply customizations if any
        customizer?.Invoke(yearcard);

        return yearcard;
    }

    public static List<Yearcard> CreateMany(int count, ApplicationUser user, Action<Yearcard>? customizer = null)
    {
        var yearcards = Enumerable.Range(0, count)
            .Select(_ => Create(user, customizer))
            .ToList();

        return yearcards;
    }
}


}