using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PapasCRM_API.Context;
using PapasCRM_API.Requests;
using PapasCRM_API.Entities;
using PapasCRM_API.Enums;
using PapasCRM_API.Models;
using PapasCRM_API.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;
using PapasCRM_API.Mappers;
using static PapasCRM_API.Services.TranslationService;

namespace PapasCRM_API.Repositories
{
    public class CustomerRepo : ICustomerRepo
    {
        private readonly BarContext _context;
        private readonly UserManager<ApplicationUserEntity> _userManager;
        private readonly IBarMapper _mapper;

        public CustomerRepo(BarContext context, UserManager<ApplicationUserEntity> _UserManager, IBarMapper mapper)
        {
            _context = context;
            _userManager = _UserManager;
            _mapper = mapper;
        }

        public async Task<ApplicationUserEntity> CreateOrReturnFirstCustomer(ApplicationUserEntity newCustomer)
        {
            if (newCustomer.Email == null)
            {
                throw new ArgumentException(Translate("Email must be provided."));  
            }
            if (newCustomer.PhoneNumber == null)
            {
                throw new ArgumentException(Translate("Phone number must be provided."));  
            }

            // Map to entity to ensure consistency
            _mapper.EntityToUser(newCustomer);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    ApplicationUserEntity? existingCustomer = await CheckIfUserAlreadyExists(newCustomer);
                    ApplicationUserEntity customerToReturn;
                    IdentityResult result;

                    if (newCustomer.UserName.IsNullOrEmpty())
                    {
                        newCustomer.UserName = newCustomer.Email ?? newCustomer.PhoneNumber;
                    }

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
                        await _userManager.AddToRoleAsync(customerToReturn, Role.Customer.ToString());

                        if (customerToReturn.Id != null)
                        {
                            await transaction.CommitAsync();
                            return customerToReturn;
                        }
                        else
                        {
                            throw new Exception(Translate("Customer failed to be created or updated"));
                        }
                    }
                    else
                    {
                        throw new Exception(Translate("Error creating or updating user: ") + string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Data invalid. {exception.Message}");
                }
            }
        }

        private string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8) + "!1a"; // Example pattern
        }
        
        private async Task<ApplicationUserEntity?> CheckIfUserAlreadyExists(ApplicationUserEntity user)
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
