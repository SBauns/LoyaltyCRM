using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using LoyaltyCRM.Services.Services.Interfaces;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Services.Repositories.Interfaces;
using System.Diagnostics.CodeAnalysis;

public class YearcardCleanupService : IHostedService, IDisposable, IYearcardCleanupService
{
    private readonly ILogger<YearcardCleanupService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer? _timer;

    // Hardcoded variable for scheduled cleanup time (hour and minute)
    private readonly int _cleanupHour = 2; // Set this to the desired hour (24-hour format)
    private readonly int _cleanupMinute = 0; // Set this to the desired minute

    // Set your timezone here (e.g., "Central European Standard Time")
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

    [ExcludeFromCodeCoverage]
    public YearcardCleanupService(
        ILogger<YearcardCleanupService> logger,
        IServiceScopeFactory scopeFactory
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "ILogger cannot be null.");
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory), "IServiceScopeFactory cannot be null.");
    }

    [ExcludeFromCodeCoverage]
    public Task StartAsync(CancellationToken cancellationToken)
    {
        ScheduleTimer();
        return Task.CompletedTask;
    }

    [ExcludeFromCodeCoverage]
    private void ScheduleTimer()
    {
        // Calculate next run time based on local time zone
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);
        var nextRunTime = new DateTime(now.Year, now.Month, now.Day, _cleanupHour, _cleanupMinute, 0);

        // If it's already past the scheduled time today, schedule for tomorrow
        if (now > nextRunTime)
        {
            nextRunTime = nextRunTime.AddDays(1);
        }

        // Convert next run time to UTC for the timer
        var initialDelay = TimeZoneInfo.ConvertTimeToUtc(nextRunTime, _timeZone) - DateTime.UtcNow;
        _timer = new Timer(CleanupExpiredYearcards, null, initialDelay, TimeSpan.FromDays(1)); // Run every 24 hours

        // var initialDelay = TimeSpan.FromMinutes(0);
        // _timer = new Timer(CleanupExpiredYearcards, null, initialDelay, TimeSpan.FromSeconds(10)); // Run every Minute Uncomment and comment above when checking for activity in development

        _logger.LogInformation($"Scheduled cleanup for {nextRunTime} (daily at {_cleanupHour}:{_cleanupMinute} in local time).");
    }

    public async Task CleanupExpiredYearcardsAsync()
    {
        _logger.LogInformation("Checking for expired yearcards...");

        using var scope = _scopeFactory.CreateScope();

        var yearcardRepo = scope.ServiceProvider.GetRequiredService<IYearcardRepo>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var yearcards = await yearcardRepo.GetYearcards();

        var expiredUsers = yearcards
            .Where(yearcard => !yearcard.IsYearcardValidForDiscount())
            .Select(yearcard => yearcard.User)
            .Where(user => user != null)
            .DistinctBy(user => user.Id)
            .ToList();

        if (!expiredUsers.Any())
        {
            _logger.LogInformation("No expired yearcards were found.");
            return;
        }

        foreach (var user in expiredUsers)
        {
            var result = await userManager.DeleteAsync(user!);
            if (result.Succeeded)
            {
                _logger.LogInformation("Deleted expired user {UserId}.", user!.Id);
            }
            else
            {
                _logger.LogWarning(
                    "Could not delete expired user {UserId}: {Errors}",
                    user!.Id,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    [ExcludeFromCodeCoverage]
    private async void CleanupExpiredYearcards(object? state)
    {
        try
        {
            await CleanupExpiredYearcardsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while cleaning up expired yearcards.");
        }
    }

    [ExcludeFromCodeCoverage]
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    [ExcludeFromCodeCoverage]
    public void Dispose()
    {
        _timer?.Dispose();
    }
}
