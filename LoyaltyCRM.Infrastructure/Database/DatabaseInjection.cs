using System.Diagnostics.CodeAnalysis;
using LoyaltyCRM.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyCRM.Infrastructure.Database
{
    [ExcludeFromCodeCoverage]
    public static class DatabaseInjection
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<LoyaltyContext>(options =>
                options.UseSqlServer(connectionString)
            );

            return services;
        }
    }
}