using System.Text.RegularExpressions;

namespace LoyaltyCRM.Domain.DomainPrimitives
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
                throw new ArgumentException("Phone number cannot be null or empty."); //TRANSLATE
            }

            if(value.Length > 21){
                throw new ArgumentException("Phone number is too long"); //TRANSLATE
            }

            if(value.Length < 5){
                throw new ArgumentException("Phone number is too short"); //TRANSLATE
            }

            if (!Regex.IsMatch(value, pattern))
            {
                throw new ArgumentException("Phone number must be only numbers"); //TRANSLATE
            }
        }
    }
}
