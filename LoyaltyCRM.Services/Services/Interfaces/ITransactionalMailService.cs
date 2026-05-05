using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyCRM.Services.Services.Interfaces
{
    public interface ITransactionalMailService
    {
        Task<bool> PingAsync();

        Task<bool> SendTemplateEmailAsync(
            string templateName,
            string toEmail,
            string fromEmail,
            Dictionary<string, string> variables,
            string subject = ""
        );
        Task<List<string>> GetTemplatesAsync();
    }
}