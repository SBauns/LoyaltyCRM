using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.Domain.Exceptions
{
    public class MailChimpException : Exception
    {
        public MailChimpException(string message) : base(message) { }
    }
}