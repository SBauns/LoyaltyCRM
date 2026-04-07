using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;  
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyCRM.Infrastructure.Security
{
    public static class SecurityInjection
    {
        public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
