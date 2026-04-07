using System.Text.RegularExpressions;
using static PapasCRM_API.Services.TranslationService;

namespace PapasCRM_API.DomainPrimitives
{
    public class PhoneNumber
    {
        private string value;

        public PhoneNumber(string value)
        {
            ValidateNumber(value);
            this.value = value;
        }

        public string GetValue()
        {
            return this.value;
        }

        private void ValidateNumber(string value)
        {
            string pattern = @"^(\+\d{1,3}-)?\d+$";

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(Translate("Phone number cannot be null or empty."));
            }

            if(value.Length > 21){
                throw new ArgumentException(Translate("Phone number is too long"));
            }

            if(value.Length < 5){
                throw new ArgumentException(Translate("Phone number is too short"));
            }

            if (!Regex.IsMatch(value, pattern))
            {
                throw new ArgumentException(Translate("Phone number must be only numbers"));
            }
        }
    }
}
