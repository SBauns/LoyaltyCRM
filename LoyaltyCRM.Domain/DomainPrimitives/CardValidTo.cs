namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class CardValidTo
    {
        private DateTime value;

        private int thresholdDays = 30;

        public CardValidTo(DateTime value)
        {
            ValidateDate(value);
            this.value = value;
        }

        public DateTime GetValue()
        {
            return this.value;
        }

        public void AddOneYear()
        {
            this.value = value.AddYears(1);
        }

        public bool DetermineIfEligibleForDiscount()
        {
            // Define the threshold date 30 days before the validTo date
            DateTime thresholdDate = value.AddDays(thresholdDays);

            // Check if today is within the 30-day window
            return DateTime.Now <= thresholdDate;
        }

        private void ValidateDate(DateTime value)
        {
            //if (DateTime.Now.Date > value.Date)
            //{
            //    throw new ArgumentException("Valid date must be later than today");
            //}
            //TODO Validation rules: Format
        }
    }
}
