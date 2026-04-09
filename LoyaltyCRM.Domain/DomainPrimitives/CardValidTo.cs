namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class CardValidTo
    {
        public DateTime Value { get; private set;}

        private int thresholdDays = 30;

        public CardValidTo(DateTime Value)
        {
            ValidateDate(Value);
            this.Value = Value;
        }

        public void AddOneYear()
        {
            this.Value = Value.AddYears(1);
        }

        public bool DetermineIfEligibleForDiscount()
        {
            // Define the threshold date 30 days before the validTo date
            DateTime thresholdDate = Value.AddDays(thresholdDays);

            // Check if today is within the 30-day window
            return DateTime.Now <= thresholdDate;
        }

        private void ValidateDate(DateTime Value)
        {
            //if (DateTime.Now.Date > Value.Date)
            //{
            //    throw new ArgumentException("Valid date must be later than today");
            //}
            //TODO Validation rules: Format
        }
    }
}
