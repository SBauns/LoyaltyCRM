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
