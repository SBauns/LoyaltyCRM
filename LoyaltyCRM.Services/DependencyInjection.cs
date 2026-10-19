using System.Diagnostics.CodeAnalysis;
using LoyaltyCRM.Application.Services;
using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Services.Repositories;
using LoyaltyCRM.Services.Repositories.Interfaces;
using LoyaltyCRM.Services.Services;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyCRM.Services;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        
        //Add Repos
        services.AddScoped<ICustomerRepo, CustomerRepo>();
        services.AddScoped<IYearcardRepo, YearcardRepo>();
        services.AddScoped<ISettingsRepo, SettingsRepo>();
        services.AddSingleton<IAppSettingsProvider, AppSettingsProvider>();
        services.AddHostedService<YearcardCleanupService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IYearcardService, YearcardService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IFileReaderService, FileReaderService>();
        services.AddScoped<IFileImportService, FileImportService>();
        services.AddScoped<ITermsService, TermsService>();
        services.AddHttpClient<ITransactionalMailService, TransactionalMailService>();
        services.AddHttpClient<IAudienceSyncService, AudienceSyncService>();



        //Add extra Services
        // services.AddHttpClient<IMailService, MailchimpService>();

        return services;
    }
}