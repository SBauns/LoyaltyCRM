using Microsoft.Maui.Storage;
using System.Text.Json;

namespace LoyaltyCRM.MobileApp;

public class StorageService
{
    public Task SetItemAsync(string key, string value)
    {
        Preferences.Set(key, value);
        return Task.CompletedTask;
    }

    public Task<string> GetItemAsync(string key)
    {
        return Task.FromResult(Preferences.ContainsKey(key) ? Preferences.Get(key, string.Empty) ?? string.Empty : string.Empty);
    }

    public Task RemoveItemAsync(string key)
    {
        Preferences.Remove(key);
        return Task.CompletedTask;
    }

    public Task<Dictionary<string, object>> ParseToken(string token)
    {
        var parts = token?.Split('.') ?? Array.Empty<string>();
        if (parts.Length < 2)
        {
            return Task.FromResult(new Dictionary<string, object>());
        }

        string payload = parts[1];
        payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');
        var bytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
        var json = JsonDocument.Parse(bytes);
        var result = new Dictionary<string, object>();
        foreach (var property in json.RootElement.EnumerateObject())
        {
            result[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                JsonValueKind.Number => property.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => property.Value.ToString() ?? string.Empty
            };
        }

        return Task.FromResult(result);
    }
}