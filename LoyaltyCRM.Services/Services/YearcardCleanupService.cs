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
using LoyaltyCRM.Services;
using System.ComponentModel;

public class YearcardCleanupService : IHostedService, IDisposable, IYearcardCleanupService
{
    private readonly ILogger<YearcardCleanupService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAppSettingsProvider _settings;

    private Timer? _timer;

    // Set your timezone here (e.g., "Central European Standard Time")
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

    [ExcludeFromCodeCoverage]
    public YearcardCleanupService(
        ILogger<YearcardCleanupService> logger,
        IServiceScopeFactory scopeFactory,
        IAppSettingsProvider settings
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "ILogger cannot be null.");
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory), "IServiceScopeFactory cannot be null.");
        _settings = settings;
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

        // Combine today's date with the TimeOnly
        var nextRunTime = now.Date.Add(_settings.Current.TimeToCleanUpCards.ToTimeSpan());

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

        _logger.LogInformation($"Scheduled cleanup for {nextRunTime} (daily at {_settings.Current.TimeToCleanUpCards.Hour}:{_settings.Current.TimeToCleanUpCards.Minute} in local time).");
    }

    public async Task CleanupExpiredYearcardsAsync()
    {
        _logger.LogInformation("Checking for expired yearcards...");

        using var scope = _scopeFactory.CreateScope();

        var yearcardRepo = scope.ServiceProvider.GetRequiredService<IYearcardRepo>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var yearcards = await yearcardRepo.GetYearcards();

        var expiredUsers = yearcards
            .Where(yearcard => !yearcard.IsYearcardSetForDeletion(_settings.Current.TimeBeforeDeleteInvalidYearcard))
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
