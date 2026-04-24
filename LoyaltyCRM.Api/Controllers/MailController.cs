using LoyaltyCRM.Authorization;
using LoyaltyCRM.Domain.Enums;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly ITransactionalMailService _mailService;

        public MailController(ITransactionalMailService mailService)
        {
            _mailService = mailService;
        }

        // 🔹 HEALTH CHECK
        [HttpGet("ping")]
        [RequireRole(Role.Papa)]
        public async Task<ActionResult> CheckConnection()
        {
            var success = await _mailService.PingAsync();
            return success ? Ok("Mail service reachable") : StatusCode(500, "Mail service unavailable");
        }

        // // 🔹 RAW EMAIL (HTML)
        // [HttpPost("send")]
        // [RequireRole(Role.Papa)]
        // public async Task<ActionResult> SendMail([FromBody] MailDTO mail)
        // {
        //     var success = await _mailService.SendEmailAsync(
        //         mail.ToMail,
        //         mail.FromMail,
        //         mail.HtmlContent,
        //         mail.ToName,
        //         mail.FromName,
        //         mail.Subject
        //     );

        //     if (!success)
        //         return BadRequest("Email failed to send");

        //     return Ok("Email sent (or queued)");
        // }

        // 🔹 TEMPLATE EMAIL (single user)
        [HttpPost("send-template")]
        [RequireRole(Role.Papa)]
        public async Task<ActionResult> SendTemplateMail([FromBody] TemplateMailDTO mail)
        {
            var success = await _mailService.SendTemplateEmailAsync(
                mail.TemplateName,
                mail.ToMail,
                mail.FromMail,
                mail.Variables,
                mail.subject
            );

            if (!success)
                return BadRequest("Template email failed");

            return Ok("Template email sent");
        }

        // // 🔹 BULK / NEWSLETTER
        // [HttpPost("send-bulk")]
        // [RequireRole(Role.Papa)]
        // public async Task<ActionResult> SendBulkMail([FromBody] BulkMailDTO mail)
        // {
        //     var success = await _mailService.SendBulkTemplateAsync(
        //         mail.TemplateName,
        //         mail.Recipients,
        //         mail.FromMail,
        //         mail.Variables
        //     );

        //     if (!success)
        //         return BadRequest("One or more emails failed");

        //     return Ok("Bulk emails processed");
        // }

        // ================= DTOs =================

        public class MailDTO
        {
            public required string ToMail { get; set; }
            public required string FromMail { get; set; }
            public required string Subject { get; set; }
            public required string HtmlContent { get; set; }

            public string ToName { get; set; } = "";
            public string FromName { get; set; } = "";
        }

        public class TemplateMailDTO
        {
            public required string TemplateName { get; set; }
            public required string ToMail { get; set; }
            public required string FromMail { get; set; }

            public string ToName { get; set; } = "";
            public string FromName { get; set; } = "";

            public string subject { get; set; } = "";

            // KEY = merge tag, VALUE = content
            public Dictionary<string, string> Variables { get; set; } = new();
        }

        public class BulkMailDTO
        {
            public required string TemplateName { get; set; }
            public required List<string> Recipients { get; set; }
            public required string FromMail { get; set; }

            public Dictionary<string, string> Variables { get; set; } = new();
        }
    }
}