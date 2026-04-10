using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Configuration;
using static LoyaltyCRM.Services.Services.TranslationService;
using LoyaltyCRM.Authorization;
using LoyaltyCRM.Domain.Enums;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Api.Services.Interfaces;


namespace LoyaltyCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            IConfiguration configuration,
            IJwtTokenService jwtTokenService
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
        }

        //TODO For now Register is disabled as customers register by creating/buying a yearcard 
        //[HttpPost("register")]
        //[RequireRole(Role.Admin)]
        //public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        //{
        //    ApplicationUserEntity user = new ApplicationUserEntity { UserName = request.Email, Email = request.Email };
        //    IdentityResult result = await _userManager.CreateAsync(user, request.Password);

        //    if (result.Succeeded)
        //    {
        //        return Ok(new { Message = "User registered successfully" });
        //    }

        //    return BadRequest(result.Errors);
        //}

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(request.UserName, request.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // _logger.LogInformation("User {UserName} logged in successfully.", request.UserName);
                ApplicationUser? user = await _userManager.FindByNameAsync(request.UserName);
                if (user != null)
                {
                    string token = await _jwtTokenService.GenerateTokenAsync(user);
                    // string token = GenerateJwtToken(user);
                    return Ok(new { Token = token });
                }
            }
            // _logger.LogWarning("Invalid login attempt for user {UserName}.}", request.UserName);

            return Unauthorized(new { Message = Translate("Invalid login attempt") });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            string secretKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("Secret key is not configured.");
            var keyBytes = Convert.FromBase64String(secretKey); // Decode from Base64
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Add the user's roles as claims
            var roles = _userManager.GetRolesAsync(user).Result;
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        [HttpPost("change-password")]
        [RequireRole(Role.Papa)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                return NotFound(new { Message = Translate("User not found.") });
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { Message = Translate("Password changed successfully.") });
            }
            else
            {
                return BadRequest(new { Message = Translate("Password change failed."), Errors = result.Errors });
            }
        }
    }

    // public class RegisterRequest
    // {
    //     public string Email { get; set; }
    //     public string Password { get; set; }
    // }

    public class ChangePasswordRequest
    {
        public required string UserName { get; set; }
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }

    public class LoginRequest
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
