using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Infrastructure.Factories;
using LoyaltyCRM.Services;
using LoyaltyCRM.Services.Repositories;
using LoyaltyCRM.Services.Repositories.Interfaces;
using LoyaltyCRM.Services.Services;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LoyaltyCRM.Tests.CleanUpTests;

public class YearcardCleanupServiceTests : WithInMemoryDatabase
{
    [Fact]
    public async Task CleanupExpiredYearcardsAsync_DeletesUsersWithExpiredYearcards()
    {
        var (context, connection) = CreateSqliteContext();
        var options = new DbContextOptionsBuilder<LoyaltyContext>()
            .UseSqlite(connection)
            .Options;

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var settingsMock = new Mock<IAppSettingsProvider>();
        settingsMock
            .Setup(s => s.Current)
            .Returns(new AppSettings());

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => new LoyaltyContext(options));
        services.AddScoped<IYearcardRepo, YearcardRepo>();
        services.AddSingleton(userManagerMock.Object);
        services.AddSingleton(settingsMock.Object);

        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var logger = serviceProvider.GetRequiredService<ILogger<YearcardCleanupService>>();

        var user = ApplicationUserFactory.Create();

        var expiredYearcard = YearcardFactory.Create(user);

        expiredYearcard.AddValidityInterval(ValidityFactory.CreateExpired());

        user.Yearcard = expiredYearcard;

        context.Users.Add(user);
        context.Yearcards.Add(expiredYearcard);
        await context.SaveChangesAsync();

        var cleanupService = new YearcardCleanupService(logger, scopeFactory, settingsMock.Object);

        await cleanupService.CleanupExpiredYearcardsAsync();

        userManagerMock.Verify(
            x => x.DeleteAsync(It.Is<ApplicationUser>(u => u.Id == user.Id)),
            Times.Once);
    }

    [Fact]
    public async Task CleanupExpiredYearcardsAsync_DoesNotDeleteUsersWithValidYearcards()
    {
        var (context, connection) = CreateSqliteContext();
        var options = new DbContextOptionsBuilder<LoyaltyContext>()
            .UseSqlite(connection)
            .Options;

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var settingsMock = new Mock<IAppSettingsProvider>();
        settingsMock
            .Setup(s => s.Current)
            .Returns(new AppSettings());

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => new LoyaltyContext(options));
        services.AddScoped<IYearcardRepo, YearcardRepo>();
        services.AddSingleton(userManagerMock.Object);
        services.AddSingleton(settingsMock.Object);

        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var logger = serviceProvider.GetRequiredService<ILogger<YearcardCleanupService>>();

        var user = ApplicationUserFactory.Create();

        var validYearcard = YearcardFactory.Create(user);

        validYearcard.AddValidityInterval(ValidityFactory.CreateValid());

        user.Yearcard = validYearcard;

        context.Users.Add(user);
        context.Yearcards.Add(validYearcard);
        await context.SaveChangesAsync();

        var cleanupService = new YearcardCleanupService(logger, scopeFactory, settingsMock.Object);

        await cleanupService.CleanupExpiredYearcardsAsync();

        userManagerMock.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            storeMock.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }
}
