using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.DTOs.Requests.Yearcard;

namespace LoyaltyCRM.Infrastructure.Factories
{
    public class YearcardCreateRequestFactory
    {

        //CREATEREQUEST
        private static readonly Faker<YearcardCreateRequest> Faker =
            new Faker<YearcardCreateRequest>()
                .CustomInstantiator(f => new YearcardCreateRequest
                {
                    Email = f.Internet.Email(),
                    PhoneNumber = f.Phone.PhoneNumber(),
                    Name = f.Name.FirstName(),
                    UserName = f.Internet.UserName(),
                    StartDate = DateTime.Now
                });

        public static YearcardCreateRequest Create(Action<YearcardCreateRequest>? customizer = null)
        {
            // Create YearcardEntity with default fake data
            var request = Faker.Clone()
                .Generate();

            // Apply customizations if any
            customizer?.Invoke(request);

            return request;
        }


        public static List<YearcardCreateRequest> CreateMany(int count, Action<YearcardCreateRequest>? customizer = null)
        {
            return Enumerable.Range(0, count)
                .Select(_ => Create(customizer))
                .ToList();
        }
    }
}