using LoyaltyCRM.Authorization;
using LoyaltyCRM.Domain.Enums;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoyaltyCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailChimp;

        public MailController(IMailService mailChimp)
        {
            _mailChimp = mailChimp;
        }

        [HttpGet("ping")]
        [RequireRole(Role.Papa)]
        public async Task<ActionResult<bool>> CheckConnection()
        {
            bool isSucces = await _mailChimp.PingAsync();
            if(isSucces){
                return Ok();
            }
            else{
                return NotFound();
            }
        }

        [HttpPost("send")]
        [RequireRole(Role.Papa)]
        public async Task<ActionResult<bool>> SendMail(MailDTO mail)
        {
            bool isSucces = await _mailChimp.SendEmailAsync(mail.ToMail, "Tester", mail.Subject, mail.HtmlContent);
            if(isSucces){
                return Ok();
            }
            else{
                return NotFound();
            }
        }

        public class MailDTO{
            public required string ToMail {get; set;}

            public required string Subject {get; set;}

            public required string HtmlContent {get; set;}
        }
    }
}
