using System.Text;
using System.Text.Json;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.Extensions.Configuration;

public class TransactionalMailService : ITransactionalMailService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public TransactionalMailService(HttpClient httpClient, IAppSettingsProvider config)
    {
        _httpClient = httpClient;

        _apiKey = config.Current.MandrillApiKey
            ?? throw new ArgumentNullException("Mandrill API key missing");

        _httpClient.BaseAddress = new Uri("https://mandrillapp.com/api/1.0/");
    }

    public async Task<bool> PingAsync()
    {
        var response = await _httpClient.PostAsync("users/ping.json",
            new StringContent(JsonSerializer.Serialize(new { key = _apiKey }),
            Encoding.UTF8, "application/json"));

        return response.IsSuccessStatusCode;
    }

    // 🔹 SEND TEMPLATE EMAIL
    public async Task<bool> SendTemplateEmailAsync(
        string templateName,
        string toEmail,
        string fromEmail,
        Dictionary<string, string> variables,
        string subject = "")
    {
        var body = new
        {
            key = _apiKey,
            template_name = templateName,
            template_content = new object[] { },
            message = new
            {
                from_email = fromEmail,
                subject = subject,
                to = new[]
                {
                    new { email = toEmail, type = "to" }
                },
                global_merge_vars = variables.Select(v => new
                {
                    name = v.Key,
                    content = v.Value
                })
            }
        };

        var response = await _httpClient.PostAsync(
            "messages/send-template.json",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Mandrill error: {content}");

        return true;
    }

    public async Task<List<string>> GetTemplatesAsync()
    {
        var body = new
        {
            key = _apiKey
        };

        var response = await _httpClient.PostAsync(
            "templates/list.json",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception(content);

        var templates = JsonSerializer.Deserialize<List<MandrillTemplate>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return templates.Select(t => t.Name).ToList();
    }

    public class MandrillTemplate
    {
        public string Name { get; set; }
    }
    }