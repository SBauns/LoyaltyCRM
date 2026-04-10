using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Factories;
using LoyaltyCRM.Services.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LoyaltyCRM.Tests.YearcardRepoTests
{
    public class GetYearcardTests : YearcardRepoTestBase
    {
        [Fact]
        public async Task GetYearcard_ShouldInclude_User_And_Intervals()
        {
            var (context, connection) = CreateSqliteContext();
            var loggerMock = new Mock<ILogger<YearcardRepo>>();

            var user = ApplicationUserFactory.Create();

            var yearcard = YearcardFactory.Create(user, y =>
            {
                y.ValidityIntervals.Add(ValidityFactory.CreateValid());
            });

            context.Users.Add(user);
            context.Yearcards.Add(yearcard);
            await context.SaveChangesAsync();

            var repo = new YearcardRepo(context, loggerMock.Object);

            var result = await repo.GetYearcard((Guid)yearcard.Id);

            result.User.Should().NotBeNull();
            result.ValidityIntervals.Should().HaveCount(1);

            connection.Close();
        }
    }
}