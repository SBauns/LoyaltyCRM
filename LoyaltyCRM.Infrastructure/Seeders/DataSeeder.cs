using System.Transactions;
using Microsoft.AspNetCore.Identity;
using PapasCRM_API.Context;
using PapasCRM_API.Entities;
using PapasCRM_API.Enums;
using PapasCRM_API.Factories;

namespace PapasCRM_API.Seeders
{
    public static class DataSeeder
    {
        public static async Task SeedUsersAsync(IServiceProvider serviceProvider, WebApplicationBuilder builder, ILogger logger)
        {

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUserEntity>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure roles exist
            await EnsureRoleAsync(roleManager, Role.Papa.ToString());
            await EnsureRoleAsync(roleManager, Role.Bartender.ToString());
            await EnsureRoleAsync(roleManager, Role.Customer.ToString());

            string papauser = builder.Configuration["Users:PapaUser"] ?? Environment.GetEnvironmentVariable("USERS_PAPAUSER");
            string papapassword = builder.Configuration["Users:PapaPassword"] ?? Environment.GetEnvironmentVariable("USERS_PAPAPASSWORD");

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
                builder.Configuration["Users:BartenderUser"] ?? Environment.GetEnvironmentVariable("USERS_BARTENDERUSER"),
                builder.Configuration["Users:BartenderPassword"] ?? Environment.GetEnvironmentVariable("USERS_PAPAPASSWORD"),
                Role.Bartender.ToString());

            LoggerExtensions.LogInformation(logger, "Bartender user seeded with username: {UserName}", builder.Configuration["Users:BartenderUser"] ?? Environment.GetEnvironmentVariable("USERS_BARTENDERUSER"));

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

        private static async Task EnsureUserAsync(UserManager<ApplicationUserEntity> userManager, string userName, string password, string role)
        {
            if (await userManager.FindByNameAsync(userName) == null)
            {
                var user = new ApplicationUserEntity
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

        public static async Task SeedTestData(IServiceProvider serviceProvider, BarContext context)
        {
            if(context.Yearcards.Count() > 0){
                return;
            }

            var transaction = context.Database.BeginTransaction();

            try
            {
                // var users = ApplicationUserFactory.CreateMany(10000); //TODO USE FOR STRESS TESTING
                var users = ApplicationUserFactory.CreateMany(10);
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUserEntity>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                int counter = 1;
                foreach (var user in users)
                {
                    // Create the user asynchronously and check the result
                    var result = await userManager.CreateAsync(user, "DefaultPassword123!"); // Specify a default password

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, Role.Customer.ToString());
                        // Create a YearcardEntity for the user after successful creation
                        YearcardEntity yearcard = YearcardEntityFactory.Create(user);

                        yearcard.CardId = counter;
                        // Add the YearcardEntity to the context
                        await context.Yearcards.AddAsync(yearcard);
                    }
                    else
                    {
                        // Log any issues with user creation (optional)
                        Console.WriteLine($"Failed to create user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                    counter++;
                }

                // Save changes to the database after all users and yearcards are created
                await context.SaveChangesAsync();

                transaction.Commit();
                
            }
            catch (System.Exception)
            {
                transaction.Rollback();
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

        //                     var yearcard = YearcardEntityFactory.Create(user);
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
