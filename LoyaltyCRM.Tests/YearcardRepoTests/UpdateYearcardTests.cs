using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LoyaltyCRM.Infrastructure.Factories;
using LoyaltyCRM.Services.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LoyaltyCRM.Tests.YearcardRepoTests
{
    public class UpdateYearcardTests : WithInMemoryDatabase
    {
        [Fact]
        public async Task UpdateYearcard_ShouldDiffIntervals_Correctly()
        {
            var (context, connection) = CreateSqliteContext();
            var loggerMock = new Mock<ILogger<YearcardRepo>>();

            var user = ApplicationUserFactory.Create();

            var existing = YearcardFactory.Create(user, y =>
            {
                y.ValidityIntervals.Add(ValidityFactory.CreateValid());
            });

            context.Users.Add(user);
            context.Yearcards.Add(existing);
            await context.SaveChangesAsync();

            var repo = new YearcardRepo(context, loggerMock.Object);

            existing.ValidityIntervals.Clear();
            existing.ValidityIntervals.Add(ValidityFactory.CreateValid());

            var updated = existing;

            var result = await repo.UpdateYearcard((Guid)existing.Id, updated);

            result.ValidityIntervals.Should().HaveCount(1);

            connection.Close();
        }
    }
}