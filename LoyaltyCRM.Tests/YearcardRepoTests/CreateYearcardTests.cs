using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Factories;
using LoyaltyCRM.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LoyaltyCRM.Tests.YearcardRepoTests
{
    public class CreateYearcardTests : WithInMemoryDatabase
    {
        [Fact]
        public async Task CreateYearcard_ShouldThrow_OnDuplicateCardId()
        {
            var (context, connection) = CreateSqliteContext();
            var loggerMock = new Mock<ILogger<YearcardRepo>>();

            var user = ApplicationUserFactory.Create();
            var userTwo = ApplicationUserFactory.Create();

            context.Users.Add(user);
            context.Users.Add(userTwo);
            await context.SaveChangesAsync();

            var repo = new YearcardRepo(context, loggerMock.Object);

            var cardId = new LoyaltyCRM.Domain.DomainPrimitives.CardNumber(123);

            var y1 = YearcardFactory.Create(user, y => y.CardId = cardId);
            var y2 = YearcardFactory.Create(userTwo, y => y.CardId = cardId); // duplicate

            await repo.CreateYearcard(y1);

            // Act
            Func<Task> act = async () => await repo.CreateYearcard(y2);

            // Assert
            await act.Should().ThrowAsync<DbUpdateException>();

            connection.Close();
        }
    }
}