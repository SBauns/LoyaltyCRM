using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PapasCRM_API.Services.Interfaces
{
    public interface IMailService
    {
        Task<bool> SendEmailAsync(string toEmail, string toName = "", string subject = "", string htmlContent = "", string fromEmail = "", string fromName = "");

        Task<bool> PingAsync();
    }
}