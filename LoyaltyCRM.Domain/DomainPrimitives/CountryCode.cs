namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class CountryCode
    {
        public string PhoneCode { get; set; }

        public CountryCode(string phoneCode)
        {
            PhoneCode = phoneCode;
        }
    }

    public static class CountryCodes
    {
        public static readonly List<CountryCode> Countries = new List<CountryCode>
        {
            new CountryCode("+45"),
            new CountryCode("+1"),
            new CountryCode("+44"),
            new CountryCode("+49"),
            new CountryCode("+33"),
            // Add more countries as needed
        };
    }
}
