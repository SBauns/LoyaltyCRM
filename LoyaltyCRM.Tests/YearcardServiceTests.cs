using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Services.Services;
using LoyaltyCRM.Services.Repositories.Interfaces;
using LoyaltyCRM.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using LoyaltyCRM.Infrastructure.Factories;
using Microsoft.EntityFrameworkCore.Storage;
using LoyaltyCRM.Services.Services.Interfaces;

public class YearcardServiceTests : IDisposable
{
    private readonly Mock<IYearcardRepo> _yearcardRepoMock;
    private readonly Mock<ICustomerRepo> _customerRepoMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<YearcardService>> _loggerMock;
    private readonly Mock<LoyaltyContext> _contextMock;
    private readonly Mock<ITransactionService> _transactionMock;

    private readonly YearcardService _sut;

    public YearcardServiceTests()
    {
        _yearcardRepoMock = new Mock<IYearcardRepo>();
        _customerRepoMock = new Mock<ICustomerRepo>();
        _loggerMock = new Mock<ILogger<YearcardService>>();

        // UserManager mock (requires special setup)
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        _contextMock = new Mock<LoyaltyContext>();
        _transactionMock = new Mock<ITransactionService>();



        _sut = new YearcardService(
            _yearcardRepoMock.Object,
            _customerRepoMock.Object,
            _contextMock.Object,
            _userManagerMock.Object,
            _loggerMock.Object,
            _transactionMock.Object
        );
    }

    public void Dispose()
    {
        // Cleanup if needed later (DB, static state, etc.)
    }

    [Fact]
    public async Task CreateOrExtendYearcard_ShouldCreateNewYearcard_WhenCustomerHasNoExistingCard()
    {
        // Arrange
        var startDate = new StartDate(DateTime.UtcNow);

        var customer = ApplicationUserFactory.Create();
        customer.Yearcard = null;

        var inputCard = YearcardFactory.Create(customer);

        var createdCard = YearcardFactory.Create(customer);

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
        Assert.True(result.ValidityIntervals.Count >= 1);

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

    [Fact]
    public async Task AddValidityToCurrentYearcard_ShouldAddAndSortIntervals()
    {
        // Arrange
        var customer = ApplicationUserFactory.Create();

        var card = YearcardFactory.Create(customer);

        var updatedCard = YearcardFactory.Create(customer);

        card.ValidityIntervals = new List<ValidityInterval>
        {
            ValidityFactory.CreateValid()
        };

        var startDate = new StartDate(DateTime.UtcNow);

        _yearcardRepoMock
            .Setup(r => r.UpdateYearcard(It.IsAny<Guid>(), It.IsAny<Yearcard>()))
            .ReturnsAsync(updatedCard);

        // Act
        var result = await _sut.AddValidityToCurrentYearcard(card, startDate);

        // Assert
        Assert.Equal(2, result.ValidityIntervals.Count);

        var ordered = result.ValidityIntervals
            .OrderBy(v => v.StartDate.Value)
            .ToList();

        Assert.Equal(ordered, result.ValidityIntervals);

        _yearcardRepoMock.Verify(
            r => r.UpdateYearcard(card.Id!.Value, It.IsAny<Yearcard>()),
            Times.Once);
    }

    [Fact]
    public async Task AddValidityToCurrentYearcard_ShouldFixOverlappingIntervals()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var customer = ApplicationUserFactory.Create();
        var card = YearcardFactory.Create(customer);

        var updatedCard = YearcardFactory.Create(customer);

        card.ValidityIntervals = new List<ValidityInterval>
        {
            ValidityFactory.CreateValid()
        };

        var overlappingStart = new StartDate(now.AddMonths(6));

        _yearcardRepoMock
            .Setup(r => r.UpdateYearcard(It.IsAny<Guid>(), It.IsAny<Yearcard>()))
            .ReturnsAsync(updatedCard);

        // Act
        var result = await _sut.AddValidityToCurrentYearcard(card, overlappingStart);

        // Assert
        var intervals = result.ValidityIntervals;

        for (int i = 1; i < intervals.Count; i++)
        {
            Assert.True(intervals[i].StartDate.Value >= intervals[i - 1].EndDate.Value);
        }
    }

    [Fact]
    public async Task GetYearcards_ShouldReturnAllYearcards()
    {
        // Arrange
        var user = ApplicationUserFactory.Create();
        var yearcards = YearcardFactory.CreateMany(3, user);

        _yearcardRepoMock
            .Setup(x => x.GetYearcards())
            .ReturnsAsync(yearcards);

            // Act
            var result = await _sut.GetYearcards();

            // Assert
            Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetYearcard_ShouldReturnCorrectYearcard()
    {
        // Arrange
        var user = ApplicationUserFactory.Create();
        var yearcard = YearcardFactory.Create(user);

        _yearcardRepoMock
            .Setup(x => x.GetYearcard(yearcard.Id!.Value))
            .ReturnsAsync(yearcard);

        // Act
        var result = await _sut.GetYearcard(yearcard.Id!.Value);

        // Assert
        Assert.Equal(yearcard.Id, result.Id);
    }

    [Fact]
    public async Task DeleteYearcard_ShouldReturnTrue_WhenDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();

        _yearcardRepoMock
            .Setup(x => x.DeleteYearcard(id))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteYearcard(id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateYearcard_ShouldUpdateYearcard_AndUser()
    {
        // Arrange
        var user = ApplicationUserFactory.Create();
        var yearcard = YearcardFactory.Create(user);
        var id = yearcard.Id!.Value;

        _yearcardRepoMock
            .Setup(x => x.UpdateYearcard(id, yearcard))
            .ReturnsAsync(yearcard);

        _userManagerMock
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var transactionMock = new Mock<IDbContextTransaction>();

        _transactionMock
            .Setup(x => x.BeginTransactionAsync())
            .ReturnsAsync(transactionMock.Object);

        // Act
        var result = await _sut.UpdateYearcard(id, yearcard);

        // Assert
        Assert.Equal(yearcard, result);

        _yearcardRepoMock.Verify(x => x.UpdateYearcard(id, yearcard), Times.Once);
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task CheckInWithYearcards_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        var user = ApplicationUserFactory.Create();

        var yearcard = YearcardFactory.Create(user, y =>
        {
            y.ValidityIntervals = new List<ValidityInterval>
            {
                ValidityFactory.CreateValid()
            };
        });

        _yearcardRepoMock
            .Setup(x => x.GetYearcard(yearcard.Id!.Value))
            .ReturnsAsync(yearcard);

        // Act
        var result = await _sut.CheckInWithYearcards(yearcard.Id!.Value);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckInWithYearcards_ShouldReturnFalse_WhenExpired()
    {
        // Arrange
        var user = ApplicationUserFactory.Create();

        var yearcard = YearcardFactory.Create(user, y =>
        {
            y.ValidityIntervals = new List<ValidityInterval>
            {
                ValidityFactory.CreateExpired()
            };
        });

        _yearcardRepoMock
            .Setup(x => x.GetYearcard(yearcard.Id!.Value))
            .ReturnsAsync(yearcard);

        // Act
        var result = await _sut.CheckInWithYearcards(yearcard.Id!.Value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckInWithPhone_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        var user = ApplicationUserFactory.Create();

        user.Yearcard = YearcardFactory.Create(user, y =>
        {
            y.ValidityIntervals = new List<ValidityInterval>
            {
                ValidityFactory.CreateValid()
            };
        });

        _customerRepoMock
            .Setup(x => x.GetUserByPhone(It.IsAny<PhoneNumber>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.CheckInWithPhone(new PhoneNumber("12345678"));

        // Assert
        Assert.True(result);
    }

        [Fact]
    public async Task CheckInWithPhone_ShouldReturnFalse_WhenInvalid()
    {
        // Arrange
        var user = ApplicationUserFactory.Create();

        user.Yearcard = YearcardFactory.Create(user, y =>
        {
            y.ValidityIntervals = new List<ValidityInterval>
            {
                ValidityFactory.CreateExpired()
            };
        });

        _customerRepoMock
            .Setup(x => x.GetUserByPhone(It.IsAny<PhoneNumber>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.CheckInWithPhone(new PhoneNumber("12345678"));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckInWithName_ShouldReturnMatchingValidYearcards()
    {
        // Arrange
        var user = ApplicationUserFactory.Create();

        var validCard = YearcardFactory.Create(user, y =>
        {
            y.Name = new Name("John");
            y.ValidityIntervals = new List<ValidityInterval>
            {
                ValidityFactory.CreateValid()
            };
        });

        var invalidCard = YearcardFactory.Create(user, y =>
        {
            y.Name = new Name("Johnny");
            y.ValidityIntervals = new List<ValidityInterval>(); // invalid
        });

        _yearcardRepoMock
            .Setup(x => x.GetYearcards())
            .ReturnsAsync(new List<Yearcard> { validCard, invalidCard });

        // Act
        var result = await _sut.CheckInWithName("john");

        // Assert
        Assert.Single(result);
    }
}