using System.Text.RegularExpressions;

namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class PhoneNumber
    {
        public string Value { get; }

        public PhoneNumber(string Value)
        {
            ValidateNumber(Value);
            this.Value = Value;
        }

        private void ValidateNumber(string Value)
        {
            string pattern = @"^(\+\d{1,3}-)?\d+$";

            if (string.IsNullOrEmpty(Value))
            {
                throw new ArgumentException("Phone number cannot be null or empty."); //TRANSLATE
            }

            if(Value.Length > 21){
                throw new ArgumentException("Phone number is too long"); //TRANSLATE
            }

            if(Value.Length < 5){
                throw new ArgumentException("Phone number is too short"); //TRANSLATE
            }

            if (!Regex.IsMatch(Value, pattern))
            {
                throw new ArgumentException("Phone number must be only numbers"); //TRANSLATE
            }
        }
    }
}
