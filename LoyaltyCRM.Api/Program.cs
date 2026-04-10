using LoyaltyCRM.Api.Middleware;
using LoyaltyCRM.Domain.Enums;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Infrastructure.Seeders;
using LoyaltyCRM.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LoyaltyCRM.Infrastructure.Database;
using LoyaltyCRM.Infrastructure.Security;
using LoyaltyCRM.Services;
using Microsoft.AspNetCore.Identity;
using LoyaltyCRM.Api.Settings;
using LoyaltyCRM.Api.Services.Interfaces;
using LoyaltyCRM.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSecurity(builder.Configuration);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false; // No digit required
    options.Password.RequiredLength = 1; // Minimum length of 8 characters
    options.Password.RequireNonAlphanumeric = false; // No special character required
    options.Password.RequireUppercase = false; // No uppercase letter required
    options.Password.RequireLowercase = false; // Require at least one lowercase letter

    options.User.RequireUniqueEmail = false;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ æøåÆØÅ"; // Allow letters, digits, and specific special characters
})
    .AddEntityFrameworkStores<LoyaltyContext>()
    .AddDefaultTokenProviders();

//Role Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OnlyBartendersOrAdmins", policy =>
        policy.RequireRole(nameof(Role.Bartender), nameof(Role.Papa)));
    options.AddPolicy("OnlyAdmins", policy =>
        policy.RequireRole(nameof(Role.Papa)));
});

//CORS POLICY
var corsOrigins = builder.Configuration["CORS_ORIGINS"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Use DefaultConnection by default, override with env variable in Docker
builder.Services.AddDatabase(builder.Configuration);

//Add Yearcard cleaner that will remove invalid yearcards
builder.Services.AddHostedService<YearcardCleanupService>();

//Adding Authentication and Authorization using https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-8.0

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new Exception("Secret key not found. Ensure JwtSettings__SecretKey is set in the user secrets or configuration.");
}

var keyBytes = Convert.FromBase64String(secretKey);
var securityKey = new SymmetricSecurityKey(keyBytes);


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false, // We do not validate token lifetime here to allow for custom handling in OnChallenge
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = securityKey
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            string message = "Unauthorized";

            if (context.AuthenticateFailure != null)
            {
                var ex = context.AuthenticateFailure;

                if (ex is SecurityTokenExpiredException)
                {
                    message = "Token expired";
                }
                else if (ex is SecurityTokenInvalidSignatureException)
                {
                    message = "Invalid token signature";
                }
                else if (ex is SecurityTokenInvalidIssuerException)
                {
                    message = "Invalid token issuer";
                }
                else if (ex is SecurityTokenInvalidAudienceException)
                {
                    message = "Invalid token audience";
                }
                else if (ex is SecurityTokenNoExpirationException)
                {
                    message = "Token has no expiration";
                }
                else if (ex is SecurityTokenInvalidLifetimeException)
                {
                    message = "Invalid token lifetime";
                }
                else
                {
                    // Fallback: use the exception message but sanitize if needed
                    message = ex.Message;
                }
            }

            return context.Response.WriteAsync($"{{\"error\": \"{message}\"}}");
        }
    };

});

builder.Services.AddAuthorization();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

TranslationService.HttpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
    // In development, do NOT redirect HTTP to HTTPS (to avoid CORS preflight redirect issues)
}
else
{
    // In production, enforce HTTPS
    // app.UseHttpsRedirection();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();
//Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LoyaltyContext>();

    // Retry logic for DB connection using EF Core
    var maxRetries = 5;
    var delay = TimeSpan.FromSeconds(5);
    var retries = 0;
    var dbReady = false;
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        context.Database.Migrate(); // Automatically apply pending migrations and create tables.   
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying migrations.");
    }

    while (retries < maxRetries && !dbReady)
    {
        logger.LogInformation("Attempts with connectionstring: {connection}.", context.Database.GetConnectionString());
        try
        {
            dbReady = await context.Database.CanConnectAsync();
            if (dbReady)
            {
                logger.LogInformation("Successfully connected to the database on attempt {Attempt}.", retries + 1);
            }
            else
            {
                logger.LogWarning("Database connection attempt {Attempt} failed.", retries + 1);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database connection attempt {Attempt} threw an exception.", retries + 1);
            dbReady = false;
        }

        if (!dbReady)
        {
            retries++;
            if (retries == maxRetries)
                throw new Exception("Could not connect to the database after multiple attempts.");
            await Task.Delay(delay);
        }
    }

    //Seed Roles and base users if not already there
    var services = scope.ServiceProvider;
    await DataSeeder.SeedUsersAsync(services, builder.Configuration, logger);

    // if(services.GetRequiredService<IHostEnvironment>().IsDevelopment())
    //     await DataSeeder.SeedTestData(services, context);
}

app.Run();