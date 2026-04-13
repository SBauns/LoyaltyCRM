namespace LoyaltyCRM.Services
{
    public sealed record AppSettings
    {
        public int LengthOfYearcardInDays { get; set; } = 365;
        public int DiscountGracePeriodInDays { get; set; } = 90;
        public int TimeBeforeDeleteInvalidYearcard { get; set; } = 90;
        public TimeOnly TimeToCleanUpCards { get; set; } = new TimeOnly(2, 0);
    }
}
