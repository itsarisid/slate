using System.Text.Json;
using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Scheduler.Commands;

/// <summary>
/// Reschedules an existing job.
/// </summary>
public sealed record RescheduleJobCommand(
    Guid JobId,
    ScheduleType ScheduleType,
    string? ScheduleExpression,
    int? IntervalSeconds,
    DateTimeOffset? RunAt,
    DateTimeOffset? EffectiveFrom) : IRequest<Result<JobDto>>;

public sealed class RescheduleJobCommandValidator : AbstractValidator<RescheduleJobCommand>
{
    public RescheduleJobCommandValidator(ICronExpressionValidator cronExpressionValidator)
    {
        RuleFor(x => x.JobId).NotEmpty();
        RuleFor(x => x.ScheduleExpression)
            .Must(cronExpressionValidator.IsValid)
            .When(x => x.ScheduleType == Domain.Enums.ScheduleType.Cron)
            .WithMessage("Cron expression is invalid.");
        RuleFor(x => x.IntervalSeconds)
            .Must(value => value is null || value >= 300)
            .WithMessage("Interval jobs must run at least every 300 seconds.");
    }
}

public sealed class RescheduleJobCommandHandler(
    IJobRepository jobRepository,
    IRepository<JobHistory> jobHistoryRepository,
    IUnitOfWork unitOfWork,
    ISchedulerService schedulerService,
    ICurrentUserService currentUserService)
    : IRequestHandler<RescheduleJobCommand, Result<JobDto>>
{
    public async Task<Result<JobDto>> Handle(RescheduleJobCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            return Result<JobDto>.Failure("Job was not found.");
        }

        var performedBy = currentUserService.Email ?? "system@alphabet.local";
        job.UpdateSchedule(request.ScheduleType, request.ScheduleExpression, request.IntervalSeconds, request.RunAt, performedBy);
        var schedulerJobId = await schedulerService.UpdateScheduleAsync(job, cancellationToken);
        job.SetSchedulerJobId(schedulerJobId, performedBy);
        jobRepository.Update(job);

        await jobHistoryRepository.AddAsync(
            JobHistory.Create(job.Id, JobHistoryAction.Rescheduled, JsonSerializer.Serialize(request), performedBy, currentUserService.IpAddress),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var status = await schedulerService.GetJobStatusAsync(job, cancellationToken);
        return job.ToDto(status);
    }
}
