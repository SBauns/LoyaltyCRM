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

        private void ValidateName(string Value)
        {
            if (string.IsNullOrWhiteSpace(Value))
                return;

            string pattern = @"^[a-zA-ZÀ-ÖØ-öø-ÿ]+([@.-][a-zA-ZÀ-ÖØ-öø-ÿ]+)*$";

            if (!Regex.IsMatch(Value, pattern))
                throw new ArgumentException("UserName is invalid.");
        }
    }
}
