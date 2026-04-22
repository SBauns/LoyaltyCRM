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
                throw new ArgumentException("translation.card_number.too_short");
            }

            if (Value > 1000000)
            {
                throw new ArgumentException("translation.card_number.too_long");
            }
        }
    }
}
