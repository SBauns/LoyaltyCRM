// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using PapasCRM_API.Context;
// using PapasCRM_API.Entities;
// using PapasCRM_API.Enums;

// namespace PapasCRM_API.Controllers
// {
//     [Route("api/[controller]")]
//     public class SetupController : ControllerBase
//     {
//         private readonly ILogger<SetupController> _logger;
//         private readonly UserManager<ApplicationUserEntity> _userManager;
//         private readonly RoleManager<IdentityRole> _roleManager;
//         private readonly BarContext _context;

//         public SetupController(ILogger<SetupController> logger, UserManager<ApplicationUserEntity> userManager, RoleManager<IdentityRole> roleManager, BarContext context)
//         {
//             _logger = logger;
//             _userManager = userManager;
//             _roleManager = roleManager;
//             _context = context;
//         }

//         //CHECK IF DATABASE IS SETUP
//         // GET: api/issetup
//         [HttpGet("issetup")]
//         public async Task<ActionResult<bool>> IsDatabaseSetup()
//         {
//             try
//             {
//                 _context.Database.Migrate();

//                 await EnsureRoleAsync(Role.Papa);
//                 await EnsureRoleAsync(Role.Bartender);
//                 await EnsureRoleAsync(Role.Customer);

//                 if (await doesDatabaseContainAnAdminAndaBartender())
//                 {
//                     return Ok(true);
//                 }
//                 else
//                 {
//                     return Ok(false);
//                 }       
//             }
//             catch (Exception)
//             {
//                 return StatusCode(500, "An error occurred while checking the database setup.");
//             }
//         }

//         private async Task<bool> doesDatabaseContainAnAdminAndaBartender()
//         {
//             if (await EnsureUserExistAsync(Role.Papa) &&
//                 await EnsureUserExistAsync(Role.Bartender))
//                 {
//                     return true;
//                 }
//                 else
//                 {
//                     return false;
//                 } 
//         }

//         // Initialize the first admin and bartender users
//         // POST: api/initialize
//         [HttpPost("initialize")]
//         public async Task<ActionResult> InitializeFirstAdminAndBartender([FromBody] InitializeUsersRequest request)
//         {
//             if (await doesDatabaseContainAnAdminAndaBartender() == true)
//             {
//                 return BadRequest("Database is already set up with users.");
//             }

//             if (IsDatabaseSetup().Result.Value == true)
//             {
//                 return BadRequest("Database is already set up.");   
//             }

//             using (var transaction = await _context.Database.BeginTransactionAsync())
//             {
//                 try
//                 {
//                     await EnsureUserAsync(request.Papa, Role.Papa);
//                     await EnsureUserAsync(request.Bartender, Role.Bartender);

//                     transaction.Commit();

//                     return Ok("Database initialized with first admin and bartender.");
//                 }
//                 catch (Exception ex)
//                 {
//                     await transaction.RollbackAsync();
//                     _logger.LogError("An error occurred while initializing the database with first admin and bartender.");
//                     return StatusCode(500, ex.Message);
//                 }
//             }
//         }

//         public class InitializeUsersRequest
//         {
//             public RegisterRequest Papa { get; set; }
//             public RegisterRequest Bartender { get; set; }
//         }

//         public class RegisterRequest
//         {
//             public string userName { get; set; } = "";
//             public string password { get; set; } = "";
//         }

//         private async Task EnsureUserAsync(RegisterRequest registerRequest, Role role)
//         {
//             if (await _userManager.FindByNameAsync(registerRequest.userName) == null)
//             {
//                 var user = new ApplicationUserEntity
//                 {
//                     UserName = registerRequest.userName,
//                     EmailConfirmed = true // You can set this to true if you don't need email confirmation
//                 };

//                 var result = await _userManager.CreateAsync(user, registerRequest.password);
//                 if (result.Succeeded)
//                 {
//                     await _userManager.AddToRoleAsync(user, role.ToString());
//                 }
//             }
//             else
//             {
//                 throw new Exception($"User with user name {registerRequest.userName} already exists.");
//             }
//         }

//         private async Task EnsureRoleAsync(Role role)
//         {
//             if (!await _roleManager.RoleExistsAsync(role.ToString()))
//             {
//                 await _roleManager.CreateAsync(new IdentityRole(role.ToString()));
//             }
//         }
        
//         private async Task<bool> EnsureUserExistAsync(Role role)
//         {
//             // Check if any user has the "admin" role
//             var users = await _userManager.GetUsersInRoleAsync(role.ToString());
//             return users.Any();
//         }
//     }
// }