using System.Text.RegularExpressions;

namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class Email
    {
        public string Value {get; private set; }

        public Email(string Value)
        {
            ValidateEmail(Value);
            this.Value = Value;
        }

        private void ValidateEmail(string Value)
        {
            if (string.IsNullOrWhiteSpace(Value))
            {
                throw new ArgumentException("Email cannot be empty."); //TRANSLATE
            }

            //Allows only letters of all kinds and the special characters ( '-.,~)
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            bool isValid = Regex.IsMatch(Value, pattern);

            if (!isValid)
                throw new ArgumentException($"{Value} is invalid.");

        }
    }
}
