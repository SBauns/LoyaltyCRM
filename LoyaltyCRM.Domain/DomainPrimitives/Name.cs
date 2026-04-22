using System.Text.RegularExpressions;

namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class Name
    {
        public string Value { get; }

        public Name(string Value = "")
        {
            Value = Value.Trim();
            ValidateName(Value);
            this.Value = Value;
        }

        private void ValidateName(string Value)
        {
            if(Value.Length > 50)
                throw new ArgumentException("translation.name.too_long");

            //Allows only letters of all kinds and the special characters ( '-.,~)
            string pattern = @"^[a-zA-ZÀ-ÖØ-öø-ÿ]+([ '-.,~][a-zA-ZÀ-ÖØ-öø-ÿ]+)*$";

            if (!Regex.IsMatch(Value, pattern) && !string.IsNullOrWhiteSpace(Value))
                throw new ArgumentException("translation.name.invalid");

        }

    }
}
