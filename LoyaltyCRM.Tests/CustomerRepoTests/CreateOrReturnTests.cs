using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Services.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace LoyaltyCRM.Tests.CustomerRepoTests
{
    public class CreateOrReturnTests : CustomerRepoBaseTest
    {
           // -------------------------
        // CreateOrReturnFirstCustomer
        // -------------------------
        [Fact]
        public async Task CreateOrReturnFirstCustomer_ReturnsExistingUser_WhenAlreadyExists()
        {
            var (context, connection) = CreateSqliteContext();
            CustomerRepo _repo = new CustomerRepo(context, _userManagerMock.Object);
            
            var existingUser = ApplicationUserFactory.Create();
            existingUser.Email = "existing@test.com";

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var newUser = ApplicationUserFactory.Create();
            newUser.Email = "existing@test.com";

            var result = await _repo.CreateOrReturnFirstCustomer(newUser);

            Assert.Equal(existingUser.Id, result.Id);

            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrReturnFirstCustomer_CreatesUser_WhenNotExists()
        {
            var (context, connection) = CreateSqliteContext();
            CustomerRepo _repo = new CustomerRepo(context, _userManagerMock.Object);

            var newUser = ApplicationUserFactory.Create();
            newUser.Email = "new@test.com";

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _repo.CreateOrReturnFirstCustomer(newUser);

            Assert.NotNull(result);

            _userManagerMock.Verify(x => x.CreateAsync(newUser, It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(newUser, "Customer"), Times.Once);
        }

        [Fact]
        public async Task CreateOrReturnFirstCustomer_Throws_WhenCreateFails()
        {
            var (context, connection) = CreateSqliteContext();
            CustomerRepo _repo = new CustomerRepo(context, _userManagerMock.Object);

            var newUser = ApplicationUserFactory.Create();

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "fail" }));

            await Assert.ThrowsAsync<Exception>(() =>
                _repo.CreateOrReturnFirstCustomer(newUser));
        }

        [Fact]
        public async Task CreateOrReturnFirstCustomer_Throws_WhenIdIsNull()
        {
            var (context, connection) = CreateSqliteContext();
            CustomerRepo _repo = new CustomerRepo(context, _userManagerMock.Object);

            var newUser = ApplicationUserFactory.Create();
            newUser.Id = null;

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            await Assert.ThrowsAsync<Exception>(() =>
                _repo.CreateOrReturnFirstCustomer(newUser));
        }
    }
}