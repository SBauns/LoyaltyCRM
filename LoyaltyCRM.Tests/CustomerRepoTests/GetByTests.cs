using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Exceptions;
using LoyaltyCRM.Services.Repositories;
using Xunit;

namespace LoyaltyCRM.Tests.CustomerRepoTests
{
    public class GetByTests : CustomerRepoBaseTest
    {
        // -------------------------
        // GetUserByPhone
        // -------------------------
        [Fact]
        public async Task GetUserByPhone_ReturnsUser_WhenExists()
        {
            var (context, connection) = CreateSqliteContext();
            CustomerRepo _repo = new CustomerRepo(context, _userManagerMock.Object);
            
            var user = ApplicationUserFactory.Create();
            user.PhoneNumber = "+45-12345678";

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await _repo.GetUserByPhone(new PhoneNumber(user.PhoneNumber));

            Assert.NotNull(result);
            Assert.Equal(user.PhoneNumber, result.PhoneNumber);
        }

        [Fact]
        public async Task GetUserByPhone_Throws_WhenNotFound()
        {
            var (context, connection) = CreateSqliteContext();
            CustomerRepo _repo = new CustomerRepo(context, _userManagerMock.Object);

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _repo.GetUserByPhone(new PhoneNumber("+45-99999999")));
        }

        // -------------------------
        // GetUserByEmail
        // -------------------------
        [Fact]
        public async Task GetUserByEmail_ReturnsUser_WhenExists()
        {
            var (context, connection) = CreateSqliteContext();
            CustomerRepo _repo = new CustomerRepo(context, _userManagerMock.Object);

            var user = ApplicationUserFactory.Create();
            user.Email = "test@test.com";

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await _repo.GetUserByEmail(new Email("test@test.com"));

            Assert.NotNull(result);
            Assert.Equal("test@test.com", result.Email);
        }

        [Fact]
        public async Task GetUserByEmail_Throws_WhenNotFound()
        {
            var (context, connection) = CreateSqliteContext();
            CustomerRepo _repo = new CustomerRepo(context, _userManagerMock.Object);

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _repo.GetUserByEmail(new Email("missing@test.com")));
        }

        // -------------------------
        // GetUserByUserName
        // -------------------------
        [Fact]
        public async Task GetUserByUserName_ReturnsUser_WhenExists()
        {
            var (context, connection) = CreateSqliteContext();
            CustomerRepo _repo = new CustomerRepo(context, _userManagerMock.Object);

            var user = ApplicationUserFactory.Create();
            user.UserName = "testuser";

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await _repo.GetUserByUserName(new UserName("testuser"));

            Assert.NotNull(result);
            Assert.Equal("testuser", result.UserName);
        }

        [Fact]
        public async Task GetUserByUserName_Throws_WhenNotFound()
        {
            var (context, connection) = CreateSqliteContext();
            CustomerRepo _repo = new CustomerRepo(context, _userManagerMock.Object);

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _repo.GetUserByUserName(new UserName("missing")));
        }
    }
}