using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Factories;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;

namespace LoyaltyCRM.Tests.YearcardServiceTests
{
    public class CreateTests : YearcardServiceTestBase
    {
        [Fact]
        public async Task CreateOrExtendYearcard_ShouldCreateNewYearcard_WhenCustomerHasNoExistingCard()
        {
            // Arrange
            var startDate = new StartDate(DateTime.UtcNow);

            var customer = ApplicationUserFactory.Create();
            customer.Yearcard = null;

            var inputCard = YearcardFactory.Create(customer);

            var createdCard = YearcardFactory.Create(customer);

            var validity = ValidityFactory.CreateValid();

            createdCard.AddValidityInterval(validity);

            var transactionMock = new Mock<IDbContextTransaction>();
            
            _transactionMock
                .Setup(t => t.BeginTransactionAsync())
                .ReturnsAsync(transactionMock.Object);

            _customerRepoMock
                .Setup(r => r.CreateOrReturnFirstCustomer(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(customer);

            _yearcardRepoMock
                .Setup(r => r.GetNewestCardId())
                .Returns(100);

            _yearcardRepoMock
                .Setup(r => r.CreateYearcard(It.IsAny<Yearcard>()))
                .ReturnsAsync(createdCard);

            // Act
            var result = await _sut.CreateOrExtendYearcard(inputCard, startDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdCard, result);
            Assert.Equal(customer.Id, result.UserId);
            Assert.Single(result.ValidityIntervals);

            _yearcardRepoMock.Verify(r => r.CreateYearcard(It.IsAny<Yearcard>()), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateOrExtendYearcard_ShouldExtendExistingYearcard_WhenCustomerHasCard()
        {
            // Arrange
            var startDate = new StartDate(DateTime.UtcNow);

            var customer = ApplicationUserFactory.Create();
            var existingCard = YearcardFactory.Create(customer);

            existingCard.AddValidityInterval(ValidityFactory.CreateValid());

            var createdCard = YearcardFactory.Create(customer);

            customer.Yearcard = existingCard;

            var transactionMock = new Mock<IDbContextTransaction>();
            _transactionMock
                .Setup(t => t.BeginTransactionAsync())
                .ReturnsAsync(transactionMock.Object);

            _customerRepoMock
                .Setup(r => r.CreateOrReturnFirstCustomer(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(customer);

            _yearcardRepoMock
                .Setup(r => r.UpdateYearcard(It.IsAny<Guid>(), It.IsAny<Yearcard>()))
                .ReturnsAsync(createdCard);

            // Act
            var result = await _sut.CreateOrExtendYearcard(existingCard, startDate);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ValidityIntervals.Count > 1);

            _yearcardRepoMock.Verify(
                r => r.UpdateYearcard(existingCard.Id!.Value, It.IsAny<Yearcard>()),
                Times.Once);

            transactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateOrExtendYearcard_ShouldRollbackTransaction_WhenExceptionOccurs()
        {
            // Arrange
            var startDate = new StartDate(DateTime.UtcNow);

            var customer = ApplicationUserFactory.Create();
            var inputCard = YearcardFactory.Create(customer);

            var transactionMock = new Mock<IDbContextTransaction>();
            _transactionMock
                .Setup(t => t.BeginTransactionAsync())
                .ReturnsAsync(transactionMock.Object);

            _customerRepoMock
                .Setup(r => r.CreateOrReturnFirstCustomer(It.IsAny<ApplicationUser>()))
                .ThrowsAsync(new Exception("DB failure"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _sut.CreateOrExtendYearcard(inputCard, startDate));

            // Assert
            Assert.Contains("Data invalid", ex.Message);

            transactionMock.Verify(t => t.RollbackAsync(), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(), Times.Never);
        }
    }
}