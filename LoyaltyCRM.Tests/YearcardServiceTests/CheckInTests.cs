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
    public class CheckInTests : YearcardServiceTestBase
    {
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
}