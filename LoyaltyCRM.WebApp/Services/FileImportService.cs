using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LoyaltyCRM.DTOs.Dtos.FileImport;
using Microsoft.AspNetCore.Components.Forms;

public class FileImportService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _auth;

    public FileImportService(HttpClient httpClient, AuthService auth)
    {
        _httpClient = httpClient;
        _auth = auth;
    }

    public async Task<ImportPreviewResponse> PreviewAsync(IBrowserFile file)
    {
        using var content = new MultipartFormDataContent();
        using var fileStream = file.OpenReadStream(file.Size);
        content.Add(new StreamContent(fileStream), "file", file.Name);

        var request = new HttpRequestMessage(HttpMethod.Post, "api/file/preview")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ImportPreviewResponse>() ?? new ImportPreviewResponse();
    }

    public async Task<ImportResultDto> ImportAsync(IBrowserFile file, Dictionary<string, string> mapping, string? startDate = null)
    {
        using var content = new MultipartFormDataContent();
        using var fileStream = file.OpenReadStream(file.Size);
        content.Add(new StreamContent(fileStream), "File", file.Name);
        content.Add(new StringContent(JsonSerializer.Serialize(mapping)), "ColumnMappingJson");

        if (!string.IsNullOrWhiteSpace(startDate))
        {
            content.Add(new StringContent(startDate), "StartDate");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "api/file/import")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _auth.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ImportResultDto>() ?? new ImportResultDto();
    }
}
