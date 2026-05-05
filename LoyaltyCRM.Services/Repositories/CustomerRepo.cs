using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Enums;
using LoyaltyCRM.Domain.Exceptions;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Services.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltyCRM.Services.Repositories
{
    public class CustomerRepo : ICustomerRepo
    {
        private readonly LoyaltyContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerRepo(LoyaltyContext context, UserManager<ApplicationUser> _UserManager)
        {
            _context = context;
            _userManager = _UserManager;
        }

        public async Task<ApplicationUser> GetUserByPhone(PhoneNumber phoneNumber)
        {
            return await _context.Users
                .Where(y => y.PhoneNumber == phoneNumber.Value)
                .Include(y => y.Yearcard!.ValidityIntervals)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException("translation.user.phone_not_found");
        }

        public async Task<ApplicationUser> GetUserByEmail(Email email)
        {
            return await _context.Users
                .Where(y => y.Email == StringNormalizationExtensions.Normalize(email.Value))
                .Include(y => y.Yearcard!.ValidityIntervals)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException("translation.user.email_not_found");
        }

        public async Task<ApplicationUser> GetUserByUserName(UserName userName)
        {
            return await _context.Users
                .Where(y => y.UserName == StringNormalizationExtensions.Normalize(userName.Value))
                .Include(y => y.Yearcard!.ValidityIntervals)
                .FirstOrDefaultAsync() ?? throw new EntityNotFoundException("translation.user.username_not_found");
        }

        public async Task<ApplicationUser> CreateOrReturnFirstCustomer(ApplicationUser newCustomer)
        {
            ApplicationUser? existingCustomer = await GetUserIfAlreadyExists(newCustomer);
            ApplicationUser customerToReturn;
            IdentityResult result;

            //Do Not allow for changes of the user through Create, but return the existing user
            if (existingCustomer != null)
            {
                return existingCustomer;
            }
            else
            {
                string randomPassword = GenerateRandomPassword();
                result = await _userManager.CreateAsync(newCustomer, randomPassword);
                customerToReturn = newCustomer;
            }

            if (result.Succeeded)
            {
                // Assign the role to the correct user
                await _userManager.AddToRoleAsync(customerToReturn, nameof(Role.Customer));

                if (customerToReturn.Id != null)
                {
                    return customerToReturn;
                }
                else
                {
                    throw new Exception("translation.user.failed_to_create_or_update");
                }
            }
            else
            {
                throw new Exception("translation.user.failed_to_create_or_update");
            }
        }

        private string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8) + "!1a"; // Example pattern
        }
        
        private async Task<ApplicationUser?> GetUserIfAlreadyExists(ApplicationUser user)
        {
            if (user.PhoneNumber == null && user.Email == null && user.UserName == null)
                return null;

            var existingUser = await _context.Users
                .Include(u => u.Yearcard!.ValidityIntervals)
                .FirstOrDefaultAsync(y =>
                    (user.PhoneNumber != null && y.PhoneNumber == user.PhoneNumber) ||
                    (user.Email != null && y.Email == user.Email) ||
                    (user.UserName != null && y.UserName == user.UserName)
                );

            return existingUser;
        }
    }
}
