using System.Text;
using System.Text.Json;
using LoyaltyCRM.Domain.Exceptions;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

public class AudienceSyncService : IAudienceSyncService
{
    private readonly HttpClient _httpClient;
    private readonly string _listId;

    private readonly bool canSend = true;

    public AudienceSyncService(HttpClient httpClient, IAppSettingsProvider config)
    {
        _httpClient = httpClient;

        var apiKey = config.Current.MailChimpApiKey;
        var serverPrefix = config.Current.MailChimpServerPrefix;

        _listId = config.Current.MailChimpListId
            ?? throw new ArgumentNullException("ListId missing");

        _httpClient.BaseAddress =
            new Uri($"https://{serverPrefix}.api.mailchimp.com/3.0/");

        var auth = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"any:{apiKey}"));

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

        if(string.IsNullOrEmpty(apiKey) && string.IsNullOrEmpty(serverPrefix) && string.IsNullOrEmpty(_listId))
            canSend = false;
    }

    // 🔹 CREATE / UPDATE USER
    public async Task SyncUserAsync(ApplicationUser? user)
    {
        if (user == null)
            return;

        if (!canSend)
            return;

        var email = user.Email;
        if (email is null || string.IsNullOrWhiteSpace(email))
            return;

        var normalizedEmail = email.ToLowerInvariant();
        var hash = MailchimpHashHelper.CreateMD5(normalizedEmail);

        var payload = new
        {
            email_address = email,
            status_if_new = user.IsSubscribed ? "subscribed" : "unsubscribed",
            status = user.IsSubscribed ? "subscribed" : "unsubscribed",
            merge_fields = new
            {
                FNAME = user.UserName ?? ""
            }
        };

        var response = await _httpClient.PutAsync(
            $"lists/{_listId}/members/{hash}",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Mailchimp sync failed: {content}");
    }

    // 🔹 DELETE USER
    public async Task DeleteUserAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !canSend)
            return;

        var hash = MailchimpHashHelper.CreateMD5(email.ToLower());

        var response = await _httpClient.DeleteAsync(
            $"lists/{_listId}/members/{hash}");

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new MailChimpException($"Mailchimp delete failed: {content}");
        }
    }
}