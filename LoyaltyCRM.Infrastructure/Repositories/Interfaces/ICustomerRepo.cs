using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PapasCRM_API.Entities;

namespace PapasCRM_API.Repositories.Interfaces
{
    public interface ICustomerRepo
    {
        Task<ApplicationUserEntity> CreateOrReturnFirstCustomer(ApplicationUserEntity newCustomer);
    }
}