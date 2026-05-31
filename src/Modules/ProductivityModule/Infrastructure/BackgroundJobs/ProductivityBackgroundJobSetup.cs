using Alphabet.Infrastructure.Options;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Registers recurring background jobs for the productivity module.
/// </summary>
public static class ProductivityBackgroundJobSetup
{
    /// <summary>
    /// Configure.
    /// </summary>
    public static void Configure(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var schedulerSettings = scope.ServiceProvider.GetRequiredService<IOptions<SchedulerSettings>>().Value;
        if (!schedulerSettings.Provider.Equals("Hangfire", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<RecurringTaskGeneratorJob>(
            "productivity:recurring-task-generator",
            job => job.ExecuteAsync(),
            Cron.Hourly);
        recurringJobManager.AddOrUpdate<TrashCleanupJob>(
            "productivity:trash-cleanup",
            job => job.ExecuteAsync(),
            Cron.Daily);
        recurringJobManager.AddOrUpdate<ProductivityReportJob>(
            "productivity:weekly-report",
            job => job.ExecuteAsync(),
            Cron.Weekly);
        recurringJobManager.AddOrUpdate<CalendarSyncJob>(
            "productivity:calendar-sync",
            job => job.ExecuteAsync(),
            Cron.Hourly);
    }
}
