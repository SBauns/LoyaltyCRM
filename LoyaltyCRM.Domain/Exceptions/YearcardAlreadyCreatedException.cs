using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.Domain.Exceptions
{
    public class YearcardAlreadyCreatedException : Exception
    {
        public YearcardAlreadyCreatedException(string message) : base(message) { }
    }
}