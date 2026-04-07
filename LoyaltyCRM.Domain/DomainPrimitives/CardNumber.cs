using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Newtonsoft.Json.Linq;
using System.Configuration;
using static PapasCRM_API.Services.TranslationService;


namespace PapasCRM_API.DomainPrimitives
{
    public class CardNumber
    {
        private int value;

        public CardNumber(int value)
        {
            ValidateNumber(value);
            this.value = value;
        }

        public int GetValue()
        {
            return this.value;
        }

        private void ValidateNumber(int value)
        {
            if (value < 1)
            {
                throw new ArgumentException(Translate("Card Number is not allowed to be less than 1"));
            }

            if (value > 100000)
            {
                throw new ArgumentException(Translate("Card Number is not allowed to be more than 100000"));
            }
        }
    }
}
