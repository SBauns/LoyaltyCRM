using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.DTOs.Requests.Yearcard;
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

            YearcardCreateRequest request = YearcardCreateRequestFactory.Create();

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
            var result = await _sut.CreateOrExtendYearcard(request);

            // Assert
            Assert.NotNull(result);

            _yearcardRepoMock.Verify(r => r.CreateYearcard(It.IsAny<Yearcard>()), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateOrExtendYearcard_ShouldExtendExistingYearcard_WhenCustomerHasCard()
        {
            // Arrange

            var customer = ApplicationUserFactory.Create();

            var request = YearcardCreateRequestFactory.Create();

            customer.Yearcard = YearcardFactory.Create(customer);

            customer.Yearcard.AddValidityInterval(ValidityFactory.CreateValid());

            var createdCard = YearcardFactory.Create(customer);

            customer.Yearcard = createdCard;

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
            var result = await _sut.CreateOrExtendYearcard(request);

            // Assert
            Assert.NotNull(result);

            _yearcardRepoMock.Verify(
                r => r.UpdateYearcard(createdCard.Id!.Value, It.IsAny<Yearcard>()),
                Times.Once);

            transactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateOrExtendYearcard_ShouldRollbackTransaction_WhenExceptionOccurs()
        {
            // Arrange
            var request = YearcardCreateRequestFactory.Create();

            var transactionMock = new Mock<IDbContextTransaction>();
            _transactionMock
                .Setup(t => t.BeginTransactionAsync())
                .ReturnsAsync(transactionMock.Object);

            _customerRepoMock
                .Setup(r => r.CreateOrReturnFirstCustomer(It.IsAny<ApplicationUser>()))
                .ThrowsAsync(new Exception("DB failure"));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _sut.CreateOrExtendYearcard(request));

            // Assert
            Assert.Contains("Data invalid", ex.Message);

            transactionMock.Verify(t => t.RollbackAsync(), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(), Times.Never);
        }
    }
}