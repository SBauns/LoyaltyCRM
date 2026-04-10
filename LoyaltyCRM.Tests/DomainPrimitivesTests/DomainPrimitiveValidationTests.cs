using System;
using System.Collections.Generic;
using FluentAssertions;
using LoyaltyCRM.Domain.DomainPrimitives;

namespace LoyaltyCRM.Tests.DomainPrimitivesTests
{
    public class DomainPrimitiveValidationTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(100000)]
        [InlineData(42)]
        public void CardNumber_ValidValue_DoesNotThrow(int value)
        {
            Action act = () => new CardNumber(value);
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(100001)]
        [InlineData(int.MaxValue)]
        public void CardNumber_InvalidValue_ThrowsArgumentException(int invalidValue)
        {
            Action act = () => new CardNumber(invalidValue);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user.name+tag+123@example.co.uk")]
        [InlineData("first.last@example.io")]
        public void Email_ValidValue_DoesNotThrow(string value)
        {
            Action act = () => new Email(value);
            act.Should().NotThrow();
        }

        public static IEnumerable<object[]> InvalidEmails => new[]
        {
            new object[] { string.Empty },
            new object[] { "plainaddress" },
            new object[] { "test@.com" },
            new object[] { "admin@example" },
            new object[] { "test'; DROP TABLE Users;--@example.com" },
            new object[] { new string('a', 500) + "@example" }
        };

        [Theory]
        [MemberData(nameof(InvalidEmails))]
        public void Email_InvalidValue_ThrowsArgumentException(string invalidValue)
        {
            Action act = () => new Email(invalidValue);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("https://example.com/image.png")]
        [InlineData("https://cdn.example.com/assets/picture.jpeg")]
        [InlineData("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUA")]
        public void Image_ValidValue_DoesNotThrow(string value)
        {
            Action act = () => new Image(value);
            act.Should().NotThrow();
        }

        public static IEnumerable<object[]> InvalidImages => new[]
        {
            new object[] { string.Empty },
            new object[] { "http://example.com/image.txt" },
            new object[] { "javascript:alert(1)" },
            new object[] { "https://example.com/image.png<script>alert('x')</script>" },
            new object[] { "data:image/png;base64,@@@!!!" }
        };

        [Theory]
        [MemberData(nameof(InvalidImages))]
        public void Image_InvalidValue_ThrowsArgumentException(string invalidValue)
        {
            Action act = () => new Image(invalidValue);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("+1-5551234567")]
        [InlineData("1234567890")]
        [InlineData("+44-2071234567")]
        public void PhoneNumber_ValidValue_DoesNotThrow(string value)
        {
            Action act = () => new PhoneNumber(value);
            act.Should().NotThrow();
        }

        public static IEnumerable<object[]> InvalidPhoneNumbers => new[]
        {
            new object[] { string.Empty },
            new object[] { "123" },
            new object[] { "+1-abc123" },
            new object[] { "+1-555123456789012345678" },
            new object[] { "555-1234; DROP TABLE" }
        };

        [Theory]
        [MemberData(nameof(InvalidPhoneNumbers))]
        public void PhoneNumber_InvalidValue_ThrowsArgumentException(string invalidValue)
        {
            Action act = () => new PhoneNumber(invalidValue);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("Alice")]
        [InlineData("Jean-Luc")]
        [InlineData("Mary Jane")]
        public void Name_ValidValue_DoesNotThrow(string value)
        {
            Action act = () => new Name(value);
            act.Should().NotThrow();
        }

        public static IEnumerable<object[]> InvalidNames => new[]
        {
            new object[] { "Alice123" },
            new object[] { "Robert'); DROP TABLE Students;--" },
            new object[] { new string('a', 51) }
        };

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public void Name_InvalidValue_ThrowsArgumentException(string invalidValue)
        {
            Action act = () => new Name(invalidValue);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("Alice")]
        [InlineData("Johnson")]
        public void FirstName_ValidValue_DoesNotThrow(string value)
        {
            Action act = () => new FirstName(value);
            act.Should().NotThrow();
        }

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public void FirstName_InvalidValue_ThrowsArgumentException(string invalidValue)
        {
            Action act = () => new FirstName(invalidValue);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("Smith")]
        [InlineData("O'Connor")]
        public void LastName_ValidValue_DoesNotThrow(string value)
        {
            Action act = () => new LastName(value);
            act.Should().NotThrow();
        }

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public void LastName_InvalidValue_ThrowsArgumentException(string invalidValue)
        {
            Action act = () => new LastName(invalidValue);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("Fortnite")]
        [InlineData("The Legend of Zelda")]
        public void FavoriteGameName_ValidValue_DoesNotThrow(string value)
        {
            Action act = () => new FavoriteGameName(value);
            act.Should().NotThrow();
        }

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public void FavoriteGameName_InvalidValue_ThrowsArgumentException(string invalidValue)
        {
            Action act = () => new FavoriteGameName(invalidValue);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("user.name")]
        [InlineData("user-name")]
        [InlineData("user@company")]
        public void UserName_ValidValue_DoesNotThrow(string value)
        {
            Action act = () => new UserName(value);
            act.Should().NotThrow();
        }

        public static IEnumerable<object[]> InvalidUserNames => new[]
        {
            new object[] { "user name" },
            new object[] { "user!name" },
            new object[] { "user@@name" },
            new object[] { "user'; DROP TABLE users;--" },
            new object[] { new string('a', 100) + "." }
        };

        [Theory]
        [MemberData(nameof(InvalidUserNames))]
        public void UserName_InvalidValue_ThrowsArgumentException(string invalidValue)
        {
            Action act = () => new UserName(invalidValue);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CountryCode_ValidValue_DoesNotThrow()
        {
            Action act = () => new CountryCode("+45");
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData("45")]
        [InlineData("++1")]
        [InlineData("abc")]
        public void CountryCode_InvalidValue_CurrentlyConstructsWithoutValidation(string invalidValue)
        {
            var code = new CountryCode(invalidValue);
            code.PhoneCode.Should().Be(invalidValue);
        }

        [Fact]
        public void CardValidTo_AcceptsValidDate()
        {
            Action act = () => new CardValidTo(DateTime.UtcNow.AddMonths(1));
            act.Should().NotThrow();
        }

        [Fact]
        public void CardValidTo_AcceptsPastDate_WhenNoValidationIsImplemented()
        {
            Action act = () => new CardValidTo(DateTime.UtcNow.AddYears(-1));
            act.Should().NotThrow();
        }

        [Fact]
        public void StartDate_AcceptsAnyDate_WhenNoValidationIsImplemented()
        {
            Action act = () => new StartDate(DateTime.MinValue);
            act.Should().NotThrow();
        }

        [Fact]
        public void EndDate_AcceptsAnyDate_WhenNoValidationIsImplemented()
        {
            Action act = () => new EndDate(DateTime.MaxValue);
            act.Should().NotThrow();
        }

        [Fact]
        public void IsGuru_ValidValue_DoesNotThrow()
        {
            Action act = () => new IsGuru(true);
            act.Should().NotThrow();
        }
    }
}
