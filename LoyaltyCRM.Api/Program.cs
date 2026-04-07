using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using PapasCRM_API.Context;
using PapasCRM_API.Requests;
using PapasCRM_API.Entities;
using PapasCRM_API.Enums;
using PapasCRM_API.Mappers;
using PapasCRM_API.Middleware;
using PapasCRM_API.Repositories;
using PapasCRM_API.Repositories.Interfaces;
using PapasCRM_API.Seeders;
using PapasCRM_API.Services.Interfaces;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using PapasCRM_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();


// Configure Identity
builder.Services.AddIdentity<ApplicationUserEntity, IdentityRole>(options =>
{
    options.Password.RequireDigit = false; // No digit required
    options.Password.RequiredLength = 1; // Minimum length of 8 characters
    options.Password.RequireNonAlphanumeric = false; // No special character required
    options.Password.RequireUppercase = false; // No uppercase letter required
    options.Password.RequireLowercase = false; // Require at least one lowercase letter

    options.User.RequireUniqueEmail = false;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ æøåÆØÅ"; // Allow letters, digits, and specific special characters
})
    .AddEntityFrameworkStores<BarContext>()
    .AddDefaultTokenProviders();

//Role Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OnlyBartendersOrAdmins", policy =>
        policy.RequireRole(Role.Bartender.ToString(), Role.Papa.ToString()));
    options.AddPolicy("OnlyAdmins", policy =>
        policy.RequireRole(Role.Papa.ToString()));
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

// Use DefaultConnection by default, override with env variable in Docker
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BarContext>(options =>
    options.UseSqlServer(connectionString));

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
            // Skip the default logic (which sets 401 and adds headers)
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            string message = "Unauthorized";

            if (context.AuthenticateFailure is SecurityTokenExpiredException)
            {
                message = "Token expired";
            }

            return context.Response.WriteAsync($"{{\"error\": \"{message}\"}}");
        }
    };
});

builder.Services.AddAuthorization();


//Add Repos
builder.Services.AddScoped<ICustomerRepo, CustomerRepo>();
builder.Services.AddScoped<IYearcardRepo, YearcardRepo>();

//Add extra Services
builder.Services.AddHttpClient<IMailService, MailchimpService>();
builder.Services.AddSingleton<IBarMapper, BarMapper>();

var app = builder.Build();

TranslationService.HttpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // In development, do NOT redirect HTTP to HTTPS (to avoid CORS preflight redirect issues)
}
else
{
    // In production, enforce HTTPS
    // app.UseHttpsRedirection();
}

app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();
//Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BarContext>();

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
    await DataSeeder.SeedUsersAsync(services, builder, logger);

    // if(services.GetRequiredService<IHostEnvironment>().IsDevelopment())
    //     await DataSeeder.SeedTestData(services, context);
}

app.Run();