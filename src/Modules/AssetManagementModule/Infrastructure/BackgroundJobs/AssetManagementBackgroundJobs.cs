using Alphabet.Application.Common.Interfaces.AssetManagement;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Infrastructure.Options;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Services.AssetManagement;

/// <summary>
/// Registers recurring background jobs for the asset management module.
/// </summary>
public static class AssetManagementBackgroundJobSetup
{
    /// <summary>
    /// Configures recurring asset management jobs.
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
        recurringJobManager.AddOrUpdate<WorkflowEscalationJob>(
            "asset-management:workflow-escalation",
            job => job.ExecuteAsync(),
            Cron.Hourly);

        recurringJobManager.AddOrUpdate<MaintenanceReminderJob>(
            "asset-management:maintenance-reminders",
            job => job.ExecuteAsync(),
            Cron.Daily);
    }
}

/// <summary>
/// Escalates overdue workflow steps.
/// </summary>
public sealed class WorkflowEscalationJob(
    IAssetRepository assetRepository,
    IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync()
    {
        var overdue = await assetRepository.GetOverdueWorkflowInstancesAsync(DateTimeOffset.UtcNow, CancellationToken.None);
        foreach (var instance in overdue)
        {
            instance.MarkEscalated();
            assetRepository.UpdateWorkflowInstance(instance);
        }

        if (overdue.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }
    }
}

/// <summary>
/// Sends maintenance reminders for due asset service records.
/// </summary>
public sealed class MaintenanceReminderJob(
    IAssetRepository assetRepository,
    IAssetNotificationService notificationService)
{
    public async Task ExecuteAsync()
    {
        var dueMaintenance = await assetRepository.GetDueMaintenanceAsync(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14), CancellationToken.None);
        foreach (var item in dueMaintenance)
        {
            var asset = await assetRepository.GetAssetByIdAsync(item.AssetId, CancellationToken.None);
            if (asset is not null)
            {
                await notificationService.NotifyMaintenanceDueAsync(asset, item, CancellationToken.None);
            }
        }
    }
}
