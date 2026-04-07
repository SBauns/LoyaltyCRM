using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PapasCRM_API.Entities;
using PapasCRM_API.Models;
using Bogus;
using Bogus.DataSets;

namespace PapasCRM_API.Factories
{

public static class YearcardEntityFactory
{
        private static readonly Faker<YearcardEntity> Faker = new Faker<YearcardEntity>()
            .RuleFor(y => y.Id, f => f.Random.Guid())
            .RuleFor(y => y.CardId, f => f.Random.Int(1, 100000))
            // .RuleFor(y => y.Phone, f => f.Phone.PhoneNumber("########"))
            .RuleFor(y => y.Name, f => f.Name.FirstName());
            // .RuleFor(y => y.ValidTo, f => f.Date.Future(2, DateTime.Today.AddYears(-1))); // One year from now
            // .RuleFor(y => y.Email, f => f.Internet.Email())
            // .RuleFor(y => y.IsConfirmed, f => f.Random.Bool());

    public static YearcardEntity Create(ApplicationUserEntity user, Action<YearcardEntity>? customizer = null)
    {
        // Create YearcardEntity with default fake data
        var yearcard = Faker.Clone()
            .RuleFor(y => y.UserId, _ => user.Id)
            // .RuleFor(y => y.Email, _ => user.Email)
            .Generate();

        // Apply customizations if any
        customizer?.Invoke(yearcard);

        return yearcard;
    }

    public static List<YearcardEntity> CreateMany(int count, ApplicationUserEntity user, Action<YearcardEntity>? customizer = null)
    {
        var yearcards = Enumerable.Range(0, count)
            .Select(_ => Create(user, customizer))
            .ToList();

        return yearcards;
    }
}


}