using Alphabet.Application.Common.Interfaces.LeaveManagement;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.LeaveManagement;
using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.BackgroundJobs;

/// <summary>
/// Executes recurring leave management maintenance jobs.
/// </summary>
public sealed class LeaveAccrualJob(
    ILeaveRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<LeaveAccrualJob> logger)
{
    /// <summary>
    /// Evaluates active accrual rules. Balance updates are intentionally centralized for future policy-specific extensions.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var rules = await repository.GetAccrualRulesAsync(cancellationToken);
        logger.LogInformation("Evaluated {RuleCount} leave accrual rule(s).", rules.Count);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Escalates overdue approval workflow steps.
/// </summary>
public sealed class LeaveApprovalEscalationJob(
    ILeaveRepository repository,
    ILeaveNotificationService notificationService,
    IUnitOfWork unitOfWork,
    ILogger<LeaveApprovalEscalationJob> logger)
{
    /// <summary>
    /// Marks overdue workflow steps as escalated and notifies configured approvers.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var overdue = await repository.GetOverdueStepsAsync(DateTimeOffset.UtcNow, cancellationToken);
        foreach (var step in overdue)
        {
            step.Escalate();
            repository.UpdateWorkflowStep(step);
            await notificationService.NotifyApprovalOverdueAsync(step, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Escalated {StepCount} overdue leave approval step(s).", overdue.Count);
    }
}
