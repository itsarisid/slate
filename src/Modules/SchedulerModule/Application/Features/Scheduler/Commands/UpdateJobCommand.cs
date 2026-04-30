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
/// Updates an existing job definition.
/// </summary>
public sealed record UpdateJobCommand(
    Guid JobId,
    string? Name,
    string? Description,
    JobType? JobType,
    ScheduleType? ScheduleType,
    string? ScheduleExpression,
    int? IntervalSeconds,
    DateTimeOffset? RunAt,
    JobConfigurationDto? JobConfiguration,
    RetryPolicyDto? RetryPolicy,
    int? TimeoutSeconds,
    string? Timezone,
    bool? IsEnabled,
    IReadOnlyList<string>? Tags) : IRequest<Result<JobDto>>;

public sealed class UpdateJobCommandValidator : AbstractValidator<UpdateJobCommand>
{
    public UpdateJobCommandValidator(ICronExpressionValidator cronExpressionValidator)
    {
        RuleFor(x => x.JobId).NotEmpty();
        RuleFor(x => x.ScheduleExpression)
            .Must(cronExpressionValidator.IsValid)
            .When(x => x.ScheduleType == Domain.Enums.ScheduleType.Cron && !string.IsNullOrWhiteSpace(x.ScheduleExpression))
            .WithMessage("Cron expression is invalid.");
        RuleFor(x => x.IntervalSeconds)
            .Must(value => value is null || value >= 300)
            .WithMessage("Interval jobs must run at least every 300 seconds.");
        RuleFor(x => x.IntervalSeconds)
            .Must(value => value is null || value % 60 == 0)
            .WithMessage("Interval seconds must be in whole-minute increments.");
    }
}

public sealed class UpdateJobCommandHandler(
    IJobRepository jobRepository,
    IRepository<JobHistory> jobHistoryRepository,
    IUnitOfWork unitOfWork,
    ISchedulerService schedulerService,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateJobCommand, Result<JobDto>>
{
    public async Task<Result<JobDto>> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null || job.IsDeleted)
        {
            return Result<JobDto>.Failure("Job was not found.");
        }

        var performedBy = currentUserService.Email ?? "system@alphabet.local";

        job.UpdateDetails(
            request.Name,
            request.Description,
            request.JobConfiguration is null ? null : JsonSerializer.Serialize(request.JobConfiguration),
            request.RetryPolicy is null ? null : JsonSerializer.Serialize(request.RetryPolicy),
            request.Timezone,
            request.IsEnabled,
            request.Tags,
            performedBy);

        if (request.ScheduleType.HasValue)
        {
            job.UpdateSchedule(
                request.ScheduleType.Value,
                request.ScheduleExpression,
                request.IntervalSeconds,
                request.RunAt,
                performedBy);
        }

        jobRepository.Update(job);
        await jobHistoryRepository.AddAsync(
            JobHistory.Create(job.Id, JobHistoryAction.Updated, JsonSerializer.Serialize(request), performedBy, currentUserService.IpAddress),
            cancellationToken);

        if (job.IsEnabled && !job.IsPaused && !job.IsDeleted)
        {
            var schedulerJobId = await schedulerService.UpdateScheduleAsync(job, cancellationToken);
            job.SetSchedulerJobId(schedulerJobId, performedBy);
        }
        else
        {
            await schedulerService.DeleteJobAsync(job, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var status = await schedulerService.GetJobStatusAsync(job, cancellationToken);
        return job.ToDto(status);
    }
}
