using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PapasCRM_API.Authorization;
using PapasCRM_API.Enums;

namespace PapasCRM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermsController : ControllerBase
    {
        private const string TermsFileName = "Terms.html";

        private static string GetTermsFilePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Resources", TermsFileName);
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
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "Resources");
                await System.IO.File.WriteAllTextAsync(path, html);
                if (willCreate)
                {
                    return Created("/api/terms", "Terms published.");
                }
                return Ok("Terms updated.");
            }
            catch (System.Exception ex)
            {
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
