using System.Net.Http.Json;
using LoyaltyCRM.DTOs.Requests.Checkin;

namespace LoyaltyCRM.MobileApp;

public class YearcardService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _auth;

    public YearcardService(HttpClient httpClient, AuthService auth)
    {
        _httpClient = httpClient;
        _auth = auth;
    }

    public async Task<YearcardDTO> CreateYearcardAsync(YearcardDTO yearcard)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/yearcards")
        {
            Content = JsonContent.Create(yearcard)
        };

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());
        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<YearcardDTO>() ?? throw new Exception("Failed to read Yearcard from response.");
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("You are not authorized. Please log in again.");
        }
        else
        {
            throw new Exception(response.Content.ReadFromJsonAsync<ErrorMessage>().Result?.Message ?? "Failed to create Yearcard. Please try again.");
        }
    }

    private async Task<CheckInResponse> CreateCheckInResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CheckInResponse>() ?? new CheckInResponse();
        }
        else
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new ArgumentException(error?.Message ?? "Unknown error");
        }
    }

    public async Task<CheckInResponse> CheckinWithPhonenumberAsync(PhoneNumberCheckInRequest phone)
    {
        if (phone == null || string.IsNullOrWhiteSpace(phone.Phone))
        {
            throw new Exception("The number is not valid");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/yearcards/checkinphone/")
        {
            Content = JsonContent.Create(phone)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);
        return await CreateCheckInResponse(response);
    }

    public async Task<CheckInResponse> CheckinWithEmailAsync(EmailCheckInRequest email)
    {
        if (email == null || string.IsNullOrWhiteSpace(email.Email))
        {
            throw new Exception("The Email is not valid");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/yearcards/checkinemail/")
        {
            Content = JsonContent.Create(email)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);
        return await CreateCheckInResponse(response);
    }

    public async Task<CheckInResponse> CheckinWithUsernameAsync(UsernameCheckInRequest username)
    {
        if (username == null || string.IsNullOrWhiteSpace(username.UserName))
        {
            throw new Exception("The username is not valid");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/yearcards/checkinusername/")
        {
            Content = JsonContent.Create(username)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);
        return await CreateCheckInResponse(response);
    }
}