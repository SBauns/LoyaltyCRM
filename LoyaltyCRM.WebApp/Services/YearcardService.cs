using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LoyaltyCRM.DTOs.Requests.Checkin;
using Microsoft.JSInterop;

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
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/yearcards")
            {
                Content = JsonContent.Create(yearcard) // Add DTO as JSON content
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
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}");
        }
    }

    public async Task<bool> UpdateYearcardAsync(YearcardDTO yearcard)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/yearcards/{yearcard.Id}")
            {
                Content = JsonContent.Create(yearcard) // Add DTO as JSON content
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                throw new Exception(response.Content.ReadFromJsonAsync<ErrorMessage>().Result?.Message ?? "Failed to create Yearcard. Please try again.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}");
        }
    }

    public async Task<List<YearcardDTO>> GetAllYearcardsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/yearcards");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<YearcardDTO>>() ?? new List<YearcardDTO>();
        }
        else
        {
            throw new Exception("Failed to load yearcards.");
        }
    }

    public async Task<List<YearcardDTO>> GetAllUnconfirmedYearcardsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/yearcards/unconfirmed");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<YearcardDTO>>() ?? new List<YearcardDTO>();
        }
        else
        {
            throw new Exception("Failed to load yearcards.");
        }
    }

    public async Task<bool> ConfirmYearcardAsync(Guid? id)
    {
        if (id == null)
        {
            throw new Exception("The card is not valid");
        }
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/yearcards/confirm/{id}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            throw new Exception("Failed to load yearcards.");
        }
    }

    public async Task<bool> CheckinWithPhonenumberAsync(PhoneNumberCheckInRequest phone)
    {
        if (phone == null || string.IsNullOrWhiteSpace(phone.Phone))
        {
            throw new Exception("The number is not valid");
        }
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/yearcards/checkinphone/")
        {
            Content = JsonContent.Create(phone) // Add DTO as JSON content
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            // Read response as a string and parse to bool
            var responseString = await response.Content.ReadAsStringAsync();
            if (bool.TryParse(responseString, out bool isValid))
            {
                return isValid;
            }
            else
            {
                throw new Exception("Unexpected response format.");
            }
        }
        else
        {
            throw new Exception("Number was invalid");
        }
    }

    public async Task<bool> CheckinWithEmailAsync(EmailCheckInRequest email)
    {
        if (email == null || string.IsNullOrWhiteSpace(email.Email))
        {
            throw new Exception("The Email is not valid");
        }
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/yearcards/checkinemail/")
        {
            Content = JsonContent.Create(email) // Add DTO as JSON content
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            // Read response as a string and parse to bool
            var responseString = await response.Content.ReadAsStringAsync();
            if (bool.TryParse(responseString, out bool isValid))
            {
                return isValid;
            }
            else
            {
                throw new Exception("Unexpected response format.");
            }
        }
        else
        {
            throw new Exception("Email was invalid");
        }
    }

    public async Task<bool> CheckinWithUsernameAsync(UsernameCheckInRequest username)
    {
        if (username == null || string.IsNullOrWhiteSpace(username.UserName))
        {
            throw new Exception("The username is not valid");
        }
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/yearcards/checkinusername/")
        {
            Content = JsonContent.Create(username) // Add DTO as JSON content
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            // Read response as a string and parse to bool
            var responseString = await response.Content.ReadAsStringAsync();
            if (bool.TryParse(responseString, out bool isValid))
            {
                return isValid;
            }
            else
            {
                throw new Exception("Unexpected response format.");
            }
        }
        else
        {
            throw new Exception("username was invalid");
        }
    }

    public async Task<YearcardDTO> GetYearcardAsync(Guid? id)
    {
        if (id == null)
        {
            throw new Exception("The card is not valid");
        }
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/yearcards/{id}");

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            // Read response as a string and parse to bool
            var yearcard = await response.Content.ReadFromJsonAsync<YearcardDTO>();

            if (yearcard != null)
            {
                return yearcard;
            }
            else
            {
                throw new Exception("Found no Yearcard");
            }
        }
        else
        {
            throw new Exception("Number was invalid");
        }
    }

    public async Task<bool> DeleteYearcardAsync(Guid? id)
    {
        if (id == null)
        {
            throw new Exception("The card is not valid");
        }
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/yearcards/{id}");

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            throw new Exception("Delete Failed!");
        }
    } 
    
    public async Task<bool> RejectYearcardAsync(Guid? id){
        if(id == null){
            throw new Exception("The card is not valid");
        }
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/yearcards/reject/{id}");

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);

          if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            throw new Exception($"Reject Failed!{response.ToString()}");
        }
    } 
}
