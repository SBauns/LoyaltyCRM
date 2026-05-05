namespace LoyaltyCRM.Services
{
    public sealed record DiscountNotificationRule(int DaysBeforeDiscountPeriodExpires, string TemplateName);
}
