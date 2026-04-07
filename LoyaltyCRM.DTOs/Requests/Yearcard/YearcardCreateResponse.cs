using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PapasCRM_API.Requests.PropertyInterfaces;

namespace PapasCRM_API.Requests.Yearcard
{
    public class YearcardCreateResponse : IHasId, IHasCardId, IHasName, IHasEmail, IHasPhoneNumber, IHasUserName
    {
        public Guid? Id { get; set; }
        public int CardId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
    }
}