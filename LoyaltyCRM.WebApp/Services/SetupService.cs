// using System.Collections.Generic;
// using System.Net.Http;
// using System.Net.Http.Json;
// using System.Text.Json;
// using System.Threading.Tasks;

// public class SetupService
// {
//     private readonly HttpClient _httpClient;

//     private bool databaseIsSetup = false;

//     public SetupService(HttpClient httpClient)
//     {
//         _httpClient = httpClient;
//     }

//     public async Task<(bool Success, string ErrorMessage)> IsDatabaseSetup()
//     {
//         if(databaseIsSetup)
//         {
//             return (true, string.Empty);
//         }
//         try
//         {
//             var response = await _httpClient.GetAsync("/api/setup/issetup");

//             if (response.IsSuccessStatusCode)
//             {
//                 var result = await response.Content.ReadFromJsonAsync<bool>();
//                 databaseIsSetup = result;
//                 return (result, string.Empty);
//             }
//             else
//             {
//                 var error = await response.Content.ReadAsStringAsync();
//                 return (false, $"Failed to check database setup: {error}");
//             }
//         }
//         catch (HttpRequestException ex)
//         {
//             return (false, $"Network error: {ex.Message}");
//         }
//         catch (Exception ex)
//         {
//             return (false, $"Unexpected error: {ex.Message}");
//         }
//     }

//     public async Task<(bool Success, string ErrorMessage)> InitializeDatabase(InitialRegisterDTO initialData)
//     {
//         try
//         {
//             var response = await _httpClient.PostAsJsonAsync("/api/setup/initialize", initialData);

//             if (response.IsSuccessStatusCode)
//             {
//                 return (true, string.Empty);
//             }
//             else
//             {
//                 var error = await response.Content.ReadAsStringAsync();
//                 return (false, $"Failed to initialize database: {error}");
//             }
//         }
//         catch (HttpRequestException ex)
//         {
//             return (false, $"Network error: {ex.Message}");
//         }
//         catch (Exception ex)
//         {
//             return (false, $"Unexpected error: {ex.Message}");
//         }
//     }
// }
