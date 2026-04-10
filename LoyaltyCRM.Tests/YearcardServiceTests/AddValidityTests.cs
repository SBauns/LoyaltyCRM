using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Factories;
using Moq;
using Xunit;

namespace LoyaltyCRM.Tests.YearcardServiceTests
{
    public class AddValidityTests : YearcardServiceTestBase
    {
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
    }
}