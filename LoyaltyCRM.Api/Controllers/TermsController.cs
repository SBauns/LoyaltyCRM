using System.IO;
using System.Threading.Tasks;
using LoyaltyCRM.Authorization;
using LoyaltyCRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermsController : ControllerBase
    {
        private const string TermsFileName = "terms.html"; // Ensure lowercase to match Linux

        private static string GetTermsFilePath()
        {
            // Use AppContext.BaseDirectory instead of Directory.GetCurrentDirectory()
            // This ensures it works regardless of where the process is started
            var basePath = AppContext.BaseDirectory;
            
            // Construct the path relative to the base directory
            // Assuming Resources is a sibling folder to the bin/ or the root of the app
            // If Resources is inside the project root, we might need to go up or down depending on structure
            
            // STRATEGY A: If Resources is next to the DLLs (common in publish)
            var path = Path.Combine(basePath, "Resources", TermsFileName);
            
            // STRATEGY B: If Resources is in the project root and you are in /app/LoyaltyCRM.Api
            // You might need: Path.Combine(basePath, "..", "Resources", TermsFileName);
            
            // Let's stick to the standard publish structure where Resources is copied alongside the app
            // If your Dockerfile copies the whole project, Resources might be at:
            // /app/LoyaltyCRM.Api/Resources/terms.html
            // In that case, we need to find the project root.
            
            // SAFER APPROACH: Check multiple locations
            var possiblePaths = new[]
            {
                Path.Combine(basePath, "Resources", TermsFileName),
                Path.Combine(basePath, "..", "Resources", TermsFileName), // If in subfolder
                Path.Combine(Directory.GetCurrentDirectory(), "Resources", TermsFileName) // Fallback
            };

            foreach (var p in possiblePaths)
            {
                if (System.IO.File.Exists(p))
                    return p;
            }

            // If none found, return the primary guess so the error message is clear
            return Path.Combine(path, "Resources", TermsFileName);
        }

        // PUT: api/terms
        [HttpPut]
        [Consumes("text/html", "text/plain", "application/json")]
        [RequireRole(Role.Papa)]
        public async Task<IActionResult> UpdateTerms()
        {
            string html;
            using (var reader = new StreamReader(Request.Body))
            {
                html = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(html))
                return BadRequest("Cannot update empty terms.");

            var path = GetTermsFilePath();

            var willCreate = !System.IO.File.Exists(path);

            try
            {
                var dirName = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
                
                await System.IO.File.WriteAllTextAsync(path, html);
                
                if (willCreate)
                {
                    return Created("/api/terms", "Terms published.");
                }
                return Ok("Terms updated.");
            }
            catch (System.Exception ex)
            {
                // Log the path for debugging
                Console.WriteLine($"Error saving terms to: {path}. Exception: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        // GET: api/terms
        [HttpGet]
        public IActionResult GetTerms()
        {
            var path = GetTermsFilePath();

            if (!System.IO.File.Exists(path))
            {
                // Log the attempted path for debugging
                Console.WriteLine($"Terms file not found at: {path}");
                return NotFound("Terms not found.");
            }

            try
            {
                var html = System.IO.File.ReadAllText(path);
                return Content(html, "text/html");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}