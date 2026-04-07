using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PapasCRM_API.Services.Interfaces;

public class MailchimpService : IMailService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _serverPrefix;

    public MailchimpService(HttpClient httpClient, IConfiguration configuration)
    {
        if (httpClient == null) {
            throw new ArgumentNullException(nameof(httpClient), "HttpClient cannot be null.");
        }
        if (configuration == null) {
            throw new ArgumentNullException(nameof(configuration), "IConfiguration cannot be null.");
        }
        _httpClient = httpClient;
        _apiKey = configuration["Mailchimp:ApiKey"] ?? throw new ArgumentNullException("Mailchimp API Key is not configured.");
        _serverPrefix = configuration["Mailchimp:ServerPrefix"] ?? throw new ArgumentNullException("Mailchimp Server Prefix is not configured.");
        _httpClient.BaseAddress = new Uri($"https://{_serverPrefix}.api.mailchimp.com/3.0/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<bool> SendEmailAsync(string toEmail, string toName = "", string subject = "", string htmlContent = "", string fromEmail = "", string fromName = "")
    {
        // Construct the message object
        var message = new
        {
            html = htmlContent,
            subject = subject,
            from_email = fromEmail,
            from_name = fromName,
            to = new[]
            {
                new {
                    email = toEmail,
                    name = toName,
                    type = "to"
                }
            }
        };

        // Construct the request body
        var body = new
        {
            key = _apiKey,
            message = message
        };

        // Serialize the body to JSON
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        // Send the request to the Mailchimp API
        var response = await _httpClient.PostAsync("messages/send", content);

        // Return true if the status code is successful (200 OK)
        if(response.IsSuccessStatusCode)
            return response.IsSuccessStatusCode;
        else
            throw new Exception(response.ToString());
    }

    public async Task<bool> PingAsync()
    {
        var response = await _httpClient.GetAsync("ping");
        return response.IsSuccessStatusCode;
    }
}
