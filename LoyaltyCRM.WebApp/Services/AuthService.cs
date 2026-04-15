
using System.Net.Http.Json;
using LoyaltyCRM.DTOs.Requests.Auth;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly StorageService _storageService;

    public Role _role { get; private set; }

    public event Action? OnChange;

    public AuthService(HttpClient httpClient, StorageService storageService)
    {
        _httpClient = httpClient;
        _storageService = storageService;
        SetLoginStateAsync();
    }

    public async Task<(bool Success, string ErrorMessage)> LoginAsync(LoginRequest loginDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                var token = (await response.Content.ReadFromJsonAsync<TokenDto>())?.token;
                if (token != null)
                {
                    await _storageService.SetItemAsync("authToken", token);
                    await SetLoginStateAsync();
                    return (true, string.Empty);
                }
                return (false, "No token returned from server.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, "Invalid email or password.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Login failed: {error}");
            }
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Unexpected error: {ex.Message}");
        }
    }

    public async Task SetLoginStateAsync()
    {
        await SetRoleAsync();
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public async Task<string> GetTokenAsync()
    {
        var token = await _storageService.GetItemAsync("authToken");
        if (string.IsNullOrEmpty(token))
        {
            throw new Exception("User is not authenticated.");
        }
        return token;
    }

    private async Task SetRoleAsync()
    {
        _role = Role.Unauthenticated;
        var token = await _storageService.GetItemAsync("authToken");
        if (string.IsNullOrEmpty(token)) return;

        var payload = await _storageService.ParseToken(token);
        if (payload != null && payload.TryGetValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var roleValue))
        {
            if (Enum.TryParse<Role>(roleValue.ToString(), out var role))
            {
                _role = role;
            }
        }
    }

    public async Task LogoutAsync()
    {
        _role = Role.Unauthenticated;
        await _storageService.RemoveItemAsync("authToken");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _storageService.GetItemAsync("authToken");
        return !string.IsNullOrEmpty(token);
    }

    public async Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        try
        {
            var token = await GetTokenAsync();
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/change-password")
            {
                Content = JsonContent.Create(new
                {
                    UserName = changePasswordDto.UserName,
                    CurrentPassword = changePasswordDto.CurrentPassword,
                    NewPassword = changePasswordDto.NewPassword
                })
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Change password failed: {error}");
            }
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public class ChangePasswordDto
    {
        public string UserName { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class TokenDto
    {
        public string? token { get; set; }
    }

    public enum Role
    {
        Customer,
        Bartender,
        Papa,
        Unauthenticated
    }
}
