namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class CardNumber
    {
        private int value;

        public CardNumber(int value)
        {
            SetValue(value);
            this.value = value;
        }

        public int GetValue()
        {
            return value;
        }

        public int SetValue(int newValue)
        {
            ValidateNumber(newValue);
            return value ;
        }

        private void ValidateNumber(int value)
        {
            if (value < 1)
            {
                throw new ArgumentException("Card Number is not allowed to be less than 1"); //Translate
            }

            if (value > 100000)
            {
                throw new ArgumentException("Card Number is not allowed to be more than 100000"); //Translate
            }
        }
    }
}
