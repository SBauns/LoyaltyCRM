using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PapasCRM_API.Authorization;
using PapasCRM_API.Requests;
using PapasCRM_API.Enums;
using PapasCRM_API.Models;
using PapasCRM_API.Repositories;
using PapasCRM_API.Services.Interfaces;
using System.Security.Claims;

namespace PapasCRM_API.Controllers
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
            public string ToMail {get; set;}

            public string Subject {get; set;}

            public string HtmlContent {get; set;}
        }
    }
}
