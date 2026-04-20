using System.Diagnostics.CodeAnalysis;

namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class CardNumber
    {
        public int Value { get; }

        [ExcludeFromCodeCoverage]
        public CardNumber()
        {
            //EF CORE
        }
        public CardNumber(int Value)
        {
            ValidateNumber(Value);
            this.Value = Value;
        }

        private void ValidateNumber(int Value)
        {
            if (Value < 1)
            {
                throw new ArgumentException("Card Number is not allowed to be less than 1"); //Translate
            }

            if (Value > 1000000)
            {
                throw new ArgumentException("Card Number is not allowed to be more than 100000"); //Translate
            }
        }
    }
}
