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
    public class DeleteYearcardTests : WithInMemoryDatabase
    {
        [Fact]
        public async Task DeleteYearcard_ShouldRollback_OnFailure()
        {
            var (context, connection) = CreateSqliteContext();
            var loggerMock = new Mock<ILogger<YearcardRepo>>();

            var user = ApplicationUserFactory.Create();

            var yearcard = YearcardFactory.Create(user);

            context.Users.Add(user);
            context.Yearcards.Add(yearcard);
            await context.SaveChangesAsync();

            // Force failure by breaking FK constraint (simulate issue)
            context.Users.Remove(user);
            await context.SaveChangesAsync();

            var repo = new YearcardRepo(context, loggerMock.Object);

            var result = await repo.DeleteYearcard((Guid)yearcard.Id);

            result.Should().BeFalse();

            connection.Close();
        }
    }
}