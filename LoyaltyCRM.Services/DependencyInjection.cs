using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Services.Repositories;
using LoyaltyCRM.Services.Repositories.Interfaces;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyCRM.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        //Add Repos
        services.AddScoped<ICustomerRepo, CustomerRepo>();
        services.AddScoped<IYearcardRepo, YearcardRepo>();

        services.AddHostedService<YearcardCleanupService>();

        //Add extra Services
        services.AddHttpClient<IMailService, MailchimpService>();

        return services;
    }
}