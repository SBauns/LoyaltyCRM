using System;
using System.Diagnostics.CodeAnalysis;
using LoyaltyCRM.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LoyaltyCRM.Api.Database
{
    public class LoyaltyDbContextFactory : IDesignTimeDbContextFactory<LoyaltyContext>
    {
        public LoyaltyContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<LoyaltyDbContextFactory>(optional: true) // now valid
                .Build();

            var connectionString = config.GetConnectionString("DesignConnection");

            var builder = new DbContextOptionsBuilder<LoyaltyContext>();
            builder.UseSqlServer(connectionString);

            return new LoyaltyContext(builder.Options);
        }
    }
}

