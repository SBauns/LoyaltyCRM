using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Factories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;

namespace LoyaltyCRM.Tests.YearcardServiceTests
{
    public class UpdateTests : YearcardServiceTestBase
    {
        [Fact]
        public async Task UpdateYearcard_ShouldUpdateYearcard_AndUser()
        {
            // Arrange
            var user = ApplicationUserFactory.Create();
            var yearcard = YearcardFactory.Create(user);
            var request = YearcardUpdateRequestFactory.Create();
            var id = request.Id!.Value;

            _yearcardRepoMock
                .Setup(x => x.GetYearcard(id))
                .ReturnsAsync(yearcard);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var transactionMock = new Mock<IDbContextTransaction>();

            _transactionMock
                .Setup(x => x.BeginTransactionAsync())
                .ReturnsAsync(transactionMock.Object);

            // Act
            var result = await _sut.UpdateYearcard(id, request);

            // Assert
            Assert.Equal(yearcard, result);
            Assert.NotNull(yearcard.User);

            _userManagerMock
                .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);
        }
    }
}