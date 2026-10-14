namespace LoyaltyCRM.Api.Extensions;

public static class HttpRequestExtensions
{
    public static async Task<string> ReadBodyAsStringAsync(this HttpRequest request, CancellationToken cancellationToken)
    {
        using StreamReader reader = new StreamReader(request.Body);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}