using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LoyaltyCRM.DTOs.DTOs;

public class SettingsService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _auth;

    public SettingsService(HttpClient httpClient, AuthService auth)
    {
        _httpClient = httpClient;
        _auth = auth;
    }

    public async Task<List<SettingDto>> GetSettingsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/settings");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(await ReadErrorAsync(response));
        }

        return await response.Content.ReadFromJsonAsync<List<SettingDto>>() ?? new List<SettingDto>();
    }

    public async Task<SettingDto> UpsertSettingAsync(string key, string value)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"api/settings/{Uri.EscapeDataString(key)}")
        {
            Content = JsonContent.Create(new SettingDto { Key = key, Value = value })
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(await ReadErrorAsync(response));
        }

        return await response.Content.ReadFromJsonAsync<SettingDto>() ?? throw new Exception("Unable to read updated setting from response.");
    }

    public async Task DeleteSettingAsync(string key)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/settings/{Uri.EscapeDataString(key)}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(await ReadErrorAsync(response));
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                var json = JsonSerializer.Deserialize<JsonElement>(content);
                if (json.TryGetProperty("message", out var messageElement))
                {
                    return messageElement.GetString() ?? content;
                }
            }
            catch
            {
                // ignore parse errors and return raw content
            }

            return content;
        }

        return response.ReasonPhrase ?? "Request failed.";
    }
}