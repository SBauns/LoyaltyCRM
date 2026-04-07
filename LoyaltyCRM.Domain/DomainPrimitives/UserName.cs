using System.Text.RegularExpressions;

namespace PapasCRM_API.DomainPrimitives
{
    public class UserName
    {
        private string value;

        public UserName(string value = "")
        {
            ValidateName(value);
            this.value = value;
        }

        public string GetValue()
        {
            return this.value;
        }

        private bool ValidateName(string value)
        {
            //Check if name is empty
            // if (string.IsNullOrWhiteSpace(value))
            // {
            //     throw new ArgumentException("UserName cannot be empty.");
            // }

            //Allows only letters of all kinds and the special characters ( '-.,~)
            string pattern = @"^[a-zA-ZÀ-ÖØ-öø-ÿ]+([@.-][a-zA-ZÀ-ÖØ-öø-ÿ]+)*$";

            if (!Regex.IsMatch(value, pattern) && !string.IsNullOrWhiteSpace(value))
                return false;

            return true;
        }
    }
}
