using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;

namespace LoyaltyCRM.Services.Repositories.Interfaces
{
    public interface ICustomerRepo
    {
        Task<ApplicationUser> CreateOrReturnFirstCustomer(ApplicationUser newCustomer);
        Task<ApplicationUser> GetUserByPhone(PhoneNumber phoneNumber);
        Task<ApplicationUser> GetUserByEmail(Email email);
        Task<ApplicationUser> GetUserByUserName(UserName userName);
    }
}