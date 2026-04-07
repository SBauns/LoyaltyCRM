using System.Text.RegularExpressions;

namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class Name
    {
        private string value;

        public Name(string value = "")
        {
            ValidateName(value);
            this.value = value;
        }

        public string GetValue()
        {
            return value;
        }
        private void ValidateName(string value)
        {
            if(value.Length > 50)
                throw new ArgumentException("Name is too long."); //TRANSLATE

            //Allows only letters of all kinds and the special characters ( '-.,~)
            string pattern = @"^[a-zA-ZÀ-ÖØ-öø-ÿ]+([ '-.,~][a-zA-ZÀ-ÖØ-öø-ÿ]+)*$";

            if (!Regex.IsMatch(value, pattern) && !string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name is invalid.");

        }

    }
}
