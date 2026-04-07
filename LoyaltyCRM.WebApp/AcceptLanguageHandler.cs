using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class AcceptLanguageHandler : DelegatingHandler
{
    private readonly string _language;

    public AcceptLanguageHandler(string language)
    {
        _language = language;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_language))
        {
            request.Headers.Remove("Accept-Language");
            request.Headers.Add("Accept-Language", _language);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
