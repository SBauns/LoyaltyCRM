using System.Text.RegularExpressions;
using System.Linq;

namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class PhoneNumber
    {
        public string Value { get; }

        public PhoneNumber(string value)
        {
            ValidateNumber(value);
            Value = value;
        }

        private void ValidateNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("translation.phone.cannot_be_empty");
            }

            // Length sanity check
            // Min: +45- (4 chars) is the shortest valid prefix we expect
            // Max: 30 chars allows for very long international numbers with formatting
            if (value.Length < 4 || value.Length > 30)
            {
                throw new ArgumentException("translation.phone.wrong_format");
            }

            // 1. Check if it looks like a Danish number (starts with +45-)
            if (value.StartsWith("+45-", StringComparison.Ordinal))
            {
                // STRICT Danish Validation
                // Must be exactly +45- followed by 8 digits and nothing else
                string danishPattern = @"^\+45-\d{8}$";
                
                if (!Regex.IsMatch(value, danishPattern))
                {
                    // If it starts with +45- but isn't exactly 8 digits, it's invalid.
                    // This catches "+45-22474--951" or "+45-123"
                    throw new ArgumentException("translation.phone.wrong_format");
                }
                
                return; // Valid Danish number
            }

            // 2. Fallback International Validation
            // Must start with +
            // Allow digits, spaces, hyphens, and parentheses
            string internationalPattern = @"^\+[0-9\s\-\(\)]+$";

            if (!Regex.IsMatch(value, internationalPattern))
            {
                throw new ArgumentException("translation.phone.wrong_format");
            }

            // Ensure there is at least one digit after the '+'
            // Prevents inputs like "+---" or "+ "
            if (!value.Substring(1).Any(char.IsDigit))
            {
                throw new ArgumentException("translation.phone.wrong_format");
            }
        }
    }
}