using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Infrastructure.Factories;
using Moq;
using Xunit;

namespace LoyaltyCRM.Tests.YearcardServiceTests
{
    public class GetTests : YearcardServiceTestBase
    {
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
    }
}