using System.Text.RegularExpressions;

namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class UserName
    {
        public string Value { get; }

        public UserName(string Value = "")
        {
            ValidateName(Value);
            this.Value = Value;
        }

        private bool ValidateName(string Value)
        {
            //Check if name is empty
            // if (string.IsNullOrWhiteSpace(Value))
            // {
            //     throw new ArgumentException("UserName cannot be empty.");
            // }

            //Allows only letters of all kinds and the special characters ( '-.,~)
            string pattern = @"^[a-zA-ZÀ-ÖØ-öø-ÿ]+([@.-][a-zA-ZÀ-ÖØ-öø-ÿ]+)*$";

            if (!Regex.IsMatch(Value, pattern) && !string.IsNullOrWhiteSpace(Value))
                return false;

            return true;
        }
    }
}
