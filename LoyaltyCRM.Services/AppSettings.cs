namespace LoyaltyCRM.Services
{
    public sealed record AppSettings
    {
        public int LengthOfYearcardInDays { get; set; } = 365;
        public int DiscountGracePeriodInDays { get; set; } = 90;
        public int TimeBeforeDeleteInvalidYearcard { get; set; } = 90;
        public TimeOnly TimeToCleanUpCards { get; set; } = new TimeOnly(2, 0);
        public string MailChimpApiKey { get; set; } = string.Empty;
        public string SenderDomain { get; set; } = string.Empty;
        public string MailChimpServerPrefix { get; set; } = string.Empty;
        public string MailChimpListId { get; set; } = string.Empty;
        public string MandrillApiKey { get; set; } = string.Empty;
        public string DiscountNotificationRules { get; set; } = "[]";
    }
}
