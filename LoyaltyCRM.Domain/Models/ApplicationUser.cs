using Microsoft.AspNetCore.Identity;

namespace LoyaltyCRM.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Yearcard Yearcard { get; set; }
    }
}
        // public string? Id { get; }
        // public PhoneNumber PhoneNumber { get; }
        // public Email Email { get; }
        // public UserName UserName { get; }

        // public ApplicationUser(string? id, PhoneNumber phoneNumber, Email email, UserName userName)
        // {
        //     Id = id;
        //     PhoneNumber = phoneNumber;
        //     Email = email;
        //     UserName = userName;
        // }
