using System.Diagnostics.CodeAnalysis;
using System.Transactions;
using Bogus;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Enums;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Infrastructure.Factories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LoyaltyCRM.Infrastructure.Seeders
{
    [ExcludeFromCodeCoverage]
    public static class DataSeeder
    {
        public static async Task SeedUsersAsync(IServiceProvider serviceProvider, IConfiguration configuration, ILogger logger)
        {

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure roles exist
            await EnsureRoleAsync(roleManager, nameof(Role.Papa));
            await EnsureRoleAsync(roleManager, nameof(Role.Bartender));
            await EnsureRoleAsync(roleManager, nameof(Role.Customer));

            string papauser = configuration["Users:AdminUser"] ?? Environment.GetEnvironmentVariable("USERS_ADMINUSER");
            string papapassword = configuration["Users:AdminPassword"] ?? Environment.GetEnvironmentVariable("USERS_ADMINPASSWORD");

            // Seed Admin user
            await EnsureUserAsync(
                userManager,
                papauser,
                papapassword,
                Role.Papa.ToString());

            LoggerExtensions.LogInformation(logger, "Admin user seeded with username: {UserName} and Password {Password}", papauser, papapassword);

            // Seed Bartender user
            await EnsureUserAsync(
                userManager,
                configuration["Users:EmployeeUser"] ?? Environment.GetEnvironmentVariable("USERS_EMPLOYEEUSER"),
                configuration["Users:EmployeePassword"] ?? Environment.GetEnvironmentVariable("USERS_EMPLOYEEPASSWORD"),
                Role.Bartender.ToString());

            LoggerExtensions.LogInformation(logger, "Bartender user seeded with username: {UserName}", configuration["Users:EmployeeUser"] ?? Environment.GetEnvironmentVariable("USERS_EMPLOYEEUSER"));

            // Seed Customer user
            // await EnsureUserAsync(
            //     userManager,
            //     builder.Configuration["Users:CustomerEmail"],
            //     builder.Configuration["Users:CustomerPassword"],  
            //     Role.Customer.ToString());
        }

        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string userName, string password, string role)
        {
            if (await userManager.FindByNameAsync(userName) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userName,
                    EmailConfirmed = true // You can set this to true if you don't need email confirmation
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    throw new Exception($"Failed to create user {userName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        public static async Task SeedTestData(IServiceProvider serviceProvider, LoyaltyContext context)
        {
            if(context.Yearcards.Count() > 0){
                return;
            }

            // var users = ApplicationUserFactory.CreateMany(200); //TODO USE FOR STRESS TESTING
            var users = ApplicationUserFactory.CreateMany(10);
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            const int batchSize = 200;

            for (int i = 0; i < users.Count; i += batchSize)
            {
                var batch = users.Skip(i).Take(batchSize).ToList();

                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var user in batch)
                    {
                        var result = await userManager.CreateAsync(user, "DefaultPassword123!");

                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, Role.Customer.ToString());

                            var yearcard = YearcardFactory.Create(user);
                            var random = new Random();

                            bool valid = random.Next(2) == 0;

                            if(valid)
                                yearcard.AddValidityInterval(ValidityFactory.CreateValid());
                            else
                                yearcard.AddValidityInterval(ValidityFactory.CreateExpired());

                            await context.Yearcards.AddAsync(yearcard);
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        // TRY THIS IF NEED RESEED TO SEE IF IT IS MUCH FASTER
        // public static async Task SeedTestData(IServiceProvider serviceProvider, BarContext context)
        // {
        //     if (context.Yearcards.Any())
        //         return;

        //     using var transaction = await context.Database.BeginTransactionAsync();

        //     try
        //     {
        //         var users = ApplicationUserFactory.CreateMany(10000);
        //         var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUserEntity>>();
        //         var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        //         int counter = 1;
        //         int batchSize = 100; // Adjust as needed

        //         var userBatches = users.Chunk(batchSize);

        //         foreach (var batch in userBatches)
        //         {
        //             var tasks = batch.Select(async user =>
        //             {
        //                 var result = await userManager.CreateAsync(user, "DefaultPassword123!");
        //                 if (result.Succeeded)
        //                 {
        //                     await userManager.AddToRoleAsync(user, Role.Customer.ToString());

        //                     var yearcard = YearcardFactory.Create(user);
        //                     yearcard.CardId = Interlocked.Increment(ref counter);
        //                     await context.Yearcards.AddAsync(yearcard);
        //                 }
        //                 else
        //                 {
        //                     Console.WriteLine($"Failed to create user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        //                 }
        //             });

        //             await Task.WhenAll(tasks);

        //             // Save EF changes in batches to reduce memory use
        //             await context.SaveChangesAsync();
        //         }

        //         await transaction.CommitAsync();
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("Error during seeding: " + ex.Message);
        //         await transaction.RollbackAsync();
        //     }
        // }
    }

}
