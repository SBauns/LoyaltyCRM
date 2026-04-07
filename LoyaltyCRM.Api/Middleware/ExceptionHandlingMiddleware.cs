using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LoyaltyCRM.Services.Services.TranslationService;

namespace LoyaltyCRM.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Continue to the next middleware or endpoint
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            _logger.LogError(exception, "An unhandled exception occurred while processing the request.");

            object response = _env.IsDevelopment()
                ? new
                {
                    message = exception.Message,
                    stackTrace = exception.StackTrace,
                    innerException = exception.InnerException?.Message
                }
                : new { message = Translate("Something went wrong.") };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}