namespace LoyaltyCRM.Services
{
    public sealed record AppSettings
    {
        public int LengthOfYearcardInDays { get; init; } = 365;
        public int DiscountGracePeriodInDays { get; init; } = 90;
        public int TimeBeforeDeleteInvalidYearcard { get; init; } = 90;
        public TimeOnly TimeToCleanUpCards = new TimeOnly(2, 0);
    }
}
