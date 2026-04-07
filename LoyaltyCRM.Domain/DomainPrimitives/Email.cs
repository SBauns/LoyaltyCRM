using System.Text.RegularExpressions;

namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class Email
    {
        private string value;

        public Email(string value)
        {
            ValidateEmail(value);
            this.value = value;
        }

        public string GetValue()
        {
            return this.value;
        }

        private void ValidateEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Email cannot be empty."); //TRANSLATE
            }

            //Allows only letters of all kinds and the special characters ( '-.,~)
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            bool isValid = Regex.IsMatch(value, pattern);

            if (!isValid)
                throw new ArgumentException($"{value} is invalid.");

        }
    }
}
