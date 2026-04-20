using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace LoyaltyCRM.Tests.YearcardServiceTests
{
    public class DeleteTests : YearcardServiceTestBase
    {
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
    }
}