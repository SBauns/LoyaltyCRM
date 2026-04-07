using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PapasCRM_API.Context;
using Microsoft.EntityFrameworkCore;
using PapasCRM_API.Services.Interfaces;
using PapasCRM_API.Entities;
using PapasCRM_API.Mappers;
using PapasCRM_API.Models; // Add this

public class YearcardCleanupService : IHostedService, IDisposable, IYearcardCleanupService
{
    private readonly ILogger<YearcardCleanupService> _logger;
    private readonly IBarMapper _mapper;
    private readonly IServiceScopeFactory _scopeFactory; // Add this
    private Timer? _timer;

     // Hardcoded variable for scheduled cleanup time (hour and minute)
    private readonly int _cleanupHour = 2; // Set this to the desired hour (24-hour format)
    private readonly int _cleanupMinute = 0; // Set this to the desired minute

    // Set your timezone here (e.g., "Central European Standard Time")
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

    public YearcardCleanupService(ILogger<YearcardCleanupService> logger, IServiceScopeFactory scopeFactory, IBarMapper mapper) // Update constructor
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "ILogger cannot be null.");
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory), "IServiceScopeFactory cannot be null.");
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper), "IBarMapper cannot be null.");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ScheduleTimer();
        return Task.CompletedTask;
    }

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
        // _timer = new Timer(CleanupExpiredYearcards, null, initialDelay, TimeSpan.FromSeconds(10)); // Run every Minute

        _logger.LogInformation($"Scheduled cleanup for {nextRunTime} (daily at {_cleanupHour}:{_cleanupMinute} in local time).");
    }

    private async void CleanupExpiredYearcards(object state)
    {
        _logger.LogInformation("Checking for expired yearcards...");

        using (var scope = _scopeFactory.CreateScope()) // Create a new scope
        {
            var context = scope.ServiceProvider.GetRequiredService<BarContext>(); // Get BarContext from the scope

            List<YearcardEntity> expiredYearcards = new List<YearcardEntity>();
            //Delete Yearcards one month after they invalidate
            List<YearcardEntity> yearcardsEntities = await context.Yearcards
                .Include(y => y.ValidityIntervals)
                .ToListAsync();

            foreach (YearcardEntity yearcardEntity in yearcardsEntities)
            {
                Yearcard yearcard = _mapper.EntityToYearcard(yearcardEntity);
                if (yearcard.IsYearcardValidForDiscount() == false)
                {
                    expiredYearcards.Add(yearcardEntity);
                }
                else
                {
                    _logger.LogInformation($"Yearcard {yearcardEntity.CardId} is still valid.");
                }
            }

            if (expiredYearcards.Any())
            {
                context.Yearcards.RemoveRange(expiredYearcards);
                await context.SaveChangesAsync();
                _logger.LogInformation($"Deleted expired yearcards.");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
