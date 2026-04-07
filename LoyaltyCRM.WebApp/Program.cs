using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using papacrm_web;
using System.Globalization;
using Microsoft.JSInterop;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Read ApiBaseUrl from configuration
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

// Build host to get JSInterop
var host = builder.Build();
var js = host.Services.GetRequiredService<IJSRuntime>();
var result = await js.InvokeAsync<string>("BlazorCulture.get") ?? "da";
if (!string.IsNullOrWhiteSpace(result))
{
    var culture = new CultureInfo(result);
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}

// Register HttpClient with AcceptLanguageHandler
builder.Services.AddScoped(sp =>
{
    var handler = new AcceptLanguageHandler(result)
    {
        InnerHandler = new HttpClientHandler()
    };
    return new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
});
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<AuthService>();
// builder.Services.AddScoped<SetupService>();
builder.Services.AddScoped<YearcardService>();
builder.Services.AddLocalization();

await builder.Build().RunAsync();
