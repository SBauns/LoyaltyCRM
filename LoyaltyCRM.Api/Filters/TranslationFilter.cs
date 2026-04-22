using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LoyaltyCRM.Api.Filters;

public class TranslationFilter : IAsyncResultFilter
{
    private readonly ILogger<TranslationFilter> _logger;

    public TranslationFilter(ILogger<TranslationFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value != null)
        {
            var lang = GetLanguage(context.HttpContext);

            // Convert object → dictionary (loosely typed)
            var dict = objectResult.Value
                .GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(objectResult.Value));

            if (dict.TryGetValue("Code", out var codeObj) && codeObj is string code)
            {
                var translated = TranslationService.TranslateAndTrack(code, lang, _logger);

                // Inject Message field
                dict["Message"] = translated;

                // Replace result with updated object
                objectResult.Value = dict;
            }
        }

        await next();
    }

    private string GetLanguage(HttpContext context)
    {
        var acceptLang = context.Request.Headers["Accept-Language"].ToString();
        if (!string.IsNullOrEmpty(acceptLang))
        {
            return acceptLang.Split(',')[0].Split('-')[0];
        }
        return "en";
    }
}