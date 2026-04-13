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

        private void ValidateNumber(string value)
        {
            string pattern = @"^\+\d{1,3}-(\d+(-\d+)*)$";

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number cannot be null or empty.");

            if (value.Length > 21)
                throw new ArgumentException("Phone number is too long");

            if (value.Length < 5)
                throw new ArgumentException("Phone number is too short");

            if (!Regex.IsMatch(value, pattern))
                throw new ArgumentException("Phone number must follow format +<countrycode>-<number>");
        }
    }
}
