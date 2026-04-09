namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class CardNumber
    {
        public int Value { get; }

        public CardNumber()
        {
            //EF CORE
        }
        public CardNumber(int Value)
        {
            SetValue(Value);
            this.Value = Value;
        }

        public int SetValue(int newValue)
        {
            ValidateNumber(newValue);
            return Value ;
        }

        private void ValidateNumber(int Value)
        {
            if (Value < 1)
            {
                throw new ArgumentException("Card Number is not allowed to be less than 1"); //Translate
            }

            if (Value > 100000)
            {
                throw new ArgumentException("Card Number is not allowed to be more than 100000"); //Translate
            }
        }
    }
}
