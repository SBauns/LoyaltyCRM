using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.DTOs.Requests.Yearcard;

namespace LoyaltyCRM.Infrastructure.Factories
{
    public class YearcardUpdateRequestFactory
    {

        private static readonly Faker<YearcardUpdateRequest> Faker =
            new Faker<YearcardUpdateRequest>()
                .CustomInstantiator(f => new YearcardUpdateRequest
                {
                    Id = Guid.NewGuid(),
                    CardId = f.Random.Int(1, 100000),
                    Email = f.Internet.Email(),
                    PhoneNumber = f.Phone.PhoneNumber(),
                    Name = f.Name.FirstName(),
                    UserName = f.Internet.UserName(),
                });

        public static YearcardUpdateRequest Create(Action<YearcardUpdateRequest>? customizer = null)
        {
            // Create YearcardEntity with default fake data
            var request = Faker.Clone()
                .Generate();

            // Apply customizations if any
            customizer?.Invoke(request);

            return request;
        }

        public static List<YearcardUpdateRequest> CreateMany(int count, Action<YearcardUpdateRequest>? customizer = null)
        {
            return Enumerable.Range(0, count)
                .Select(_ => Create(customizer))
                .ToList();
        }
    }
}