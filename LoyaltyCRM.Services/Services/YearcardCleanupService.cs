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
    private readonly ITransactionalMailService _transactionalMailService;
    private readonly IAudienceSyncService _audienceSyncService;

    private Timer? _timer;

    // Set your timezone here (e.g., "Central European Standard Time")
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

    [ExcludeFromCodeCoverage]
    public YearcardCleanupService(
        ILogger<YearcardCleanupService> logger,
        IServiceScopeFactory scopeFactory,
        IAppSettingsProvider settings,
        ITransactionalMailService transactionalMailService,
        IAudienceSyncService audienceSyncService
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "ILogger cannot be null.");
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory), "IServiceScopeFactory cannot be null.");
        _settings = settings ?? throw new ArgumentNullException(nameof(settings), "IAppSettingsProvider cannot be null.");
        _transactionalMailService = transactionalMailService ?? throw new ArgumentNullException(nameof(transactionalMailService), "ITransactionalMailService cannot be null.");
        _audienceSyncService = audienceSyncService ?? throw new ArgumentNullException(nameof(audienceSyncService), "IAudienceSyncService cannot be null.");
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
        // _timer = new Timer(CleanupExpiredYearcards, null, initialDelay, TimeSpan.FromSeconds(60)); // Run every Minute Uncomment and comment above when checking for activity in development

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
        }
        else
        {
            foreach (var user in expiredUsers)
            {
                var result = await userManager.DeleteAsync(user!);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Deleted expired user {UserId}.", user!.Id);
                    await TrySyncDeletedUserAsync(user!);
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

        await WarnUsersWithExpiredYearcardDiscountsAsync(yearcards);
    }

    public async Task WarnUsersWithExpiredYearcardDiscountsAsync(IEnumerable<Yearcard>? yearcards = null)
    {
        _logger.LogInformation("Checking for expired yearcards eligible for discount reminders...");

        using var scope = _scopeFactory.CreateScope();

        var yearcardRepo = scope.ServiceProvider.GetRequiredService<IYearcardRepo>();
        var cards = yearcards?.ToList() ?? await yearcardRepo.GetYearcards();

        var usersToWarn = cards
            .Where(yearcard => yearcard.User != null && IsExpiredButEligibleForDiscount(yearcard, _settings.Current.DiscountGracePeriodInDays))
            .Select(yearcard => yearcard.User!)
            .DistinctBy(user => user.Id)
            .ToList();

        if (!usersToWarn.Any())
        {
            _logger.LogInformation("No users were eligible for a discount reminder.");
            return;
        }

        foreach (var user in usersToWarn)
        {
            await SendDiscountReminderAsync(user, _transactionalMailService);
        }
    }

    private static bool IsExpiredButEligibleForDiscount(Yearcard yearcard, int discountGracePeriodDays)
    {
        var hasCurrentValidity = yearcard.ValidityIntervals
            .Any(interval => interval.EndDate.Value >= DateTime.Now);

        if (hasCurrentValidity)
        {
            return false;
        }

        return yearcard.ValidityIntervals
            .Any(interval => interval.EndDate.Value.AddDays(discountGracePeriodDays) >= DateTime.Now);
    }

    private async Task SendDiscountReminderAsync(ApplicationUser user, ITransactionalMailService mailService)
    {
        if (user == null || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrEmpty(_settings.Current.SenderDomain))
        {
            return;
        }

        var templateName = _settings.Current.DiscountMailTemplate;
        if (string.IsNullOrWhiteSpace(templateName))
        {
            _logger.LogWarning("Discount mail template is not configured, so no reminder email is sent for user {UserId}.", user.Id);
            return;
        }

        var variables = new Dictionary<string, string>
        {
            ["USER_NAME"] = user.UserName ?? string.Empty,
            ["DISCOUNT_GRACE_PERIOD_DAYS"] = _settings.Current.DiscountGracePeriodInDays.ToString(),
        };

        try
        {
            await mailService.SendTemplateEmailAsync(
                templateName,
                user.Email,
                $"no-reply@{_settings.Current.SenderDomain}", //TODO Set Settings Domain
                variables
            );

            _logger.LogInformation("Sent discount reminder email to user {UserId}.", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send discount reminder email to user {UserId}.", user.Id);
        }
    }

    private async Task TrySyncDeletedUserAsync(ApplicationUser user)
    {
        if (user == null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        try
        {
            await _audienceSyncService.DeleteUserAsync(user.Email);
            _logger.LogInformation("Synced deleted user {UserId} with audience service.", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sync deletion of expired user {UserId} to audience service.", user.Id);
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
