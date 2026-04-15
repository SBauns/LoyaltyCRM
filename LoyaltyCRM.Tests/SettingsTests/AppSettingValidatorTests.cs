using FluentAssertions;
using LoyaltyCRM.Services;

namespace LoyaltyCRM.Tests.SettingsTests;

public class AppSettingValidatorTests
{
    [Theory]
    [InlineData("LengthOfYearcardInDays", "365", true)]
    [InlineData("DiscountGracePeriodInDays", "90", true)]
    [InlineData("TimeBeforeDeleteInvalidYearcard", "90", true)]
    [InlineData("TimeToCleanUpCards", "02:00", true)]
    [InlineData("TimeToCleanUpCards", "2:00", true)]
    [InlineData("TimeToCleanUpCards", "25:00", false)]
    [InlineData("TimeToCleanUpCards", "invalid", false)]
    public void TryValidateSetting_ParsesExpectedTypes(string key, string value, bool expectedResult)
    {
        var result = AppSettingValidator.TryValidateSetting(key, value, out var parsedValue, out var errorMessage);

        result.Should().Be(expectedResult);

        if (expectedResult)
        {
            parsedValue.Should().NotBeNull();
            errorMessage.Should().BeNull();
        }
        else
        {
            errorMessage.Should().NotBeNullOrWhiteSpace();
        }
    }
}
