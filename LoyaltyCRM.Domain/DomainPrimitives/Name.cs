using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Text.RegularExpressions;
using static PapasCRM_API.Services.TranslationService;


namespace PapasCRM_API.DomainPrimitives
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
            return this.value;
        }
        private void ValidateName(string value)
        {
            if(value.Length > 50)
                throw new ArgumentException(Translate("Name is too long."));

            //Allows only letters of all kinds and the special characters ( '-.,~)
            string pattern = @"^[a-zA-ZÀ-ÖØ-öø-ÿ]+([ '-.,~][a-zA-ZÀ-ÖØ-öø-ÿ]+)*$";

            if (!Regex.IsMatch(value, pattern) && !string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(Translate("Name is invalid."));

        }

    }
}
