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
/// Creates a new scheduled job.
/// </summary>
public sealed record CreateJobCommand(
    string Name,
    string Description,
    JobType JobType,
    ScheduleType ScheduleType,
    string? ScheduleExpression,
    int? IntervalSeconds,
    DateTimeOffset? RunAt,
    JobConfigurationDto JobConfiguration,
    RetryPolicyDto RetryPolicy,
    int? TimeoutSeconds,
    string Timezone,
    bool IsEnabled,
    IReadOnlyList<string>? Tags,
    string? CreatedBy) : IRequest<Result<JobDto>>;

public sealed class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobCommandValidator(ICronExpressionValidator cronExpressionValidator)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Timezone).NotEmpty();
        RuleFor(x => x.IntervalSeconds)
            .Must(value => value is null || value >= 300)
            .WithMessage("Interval jobs must run at least every 300 seconds.");
        RuleFor(x => x.IntervalSeconds)
            .Must(value => value is null || value % 60 == 0)
            .WithMessage("Interval seconds must be in whole-minute increments.");
        RuleFor(x => x.ScheduleExpression)
            .Must(cronExpressionValidator.IsValid)
            .When(x => x.ScheduleType == ScheduleType.Cron)
            .WithMessage("Cron expression is invalid.");
        RuleFor(x => x.RunAt)
            .NotNull()
            .When(x => x.ScheduleType == ScheduleType.OneTime);
        RuleFor(x => x.JobConfiguration.Url)
            .Must(value => string.IsNullOrWhiteSpace(value) || Uri.TryCreate(value, UriKind.Absolute, out _))
            .WithMessage("A valid URL is required for HTTP call jobs.")
            .When(x => x.JobType == JobType.HttpCall);
        RuleFor(x => x.JobConfiguration.Method)
            .Must(value => value is null || new[] { "GET", "POST", "PUT", "DELETE", "PATCH" }.Contains(value.ToUpperInvariant()))
            .WithMessage("HTTP method must be GET, POST, PUT, DELETE, or PATCH.")
            .When(x => x.JobType == JobType.HttpCall);
        RuleFor(x => x.JobConfiguration.HandlerType)
            .NotEmpty()
            .When(x => x.JobType == JobType.CodeExecution);
        RuleFor(x => x.JobConfiguration.SourcePath)
            .NotEmpty()
            .When(x => x.JobType == JobType.FileOperation);
    }
}

public sealed class CreateJobCommandHandler(
    IJobRepository jobRepository,
    IRepository<JobHistory> jobHistoryRepository,
    IUnitOfWork unitOfWork,
    ISchedulerService schedulerService,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateJobCommand, Result<JobDto>>
{
    public async Task<Result<JobDto>> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        var performedBy = request.CreatedBy ?? currentUserService.Email ?? "system@alphabet.local";

        using var jobConfiguration = JsonDocument.Parse(JsonSerializer.Serialize(request.JobConfiguration));
        using var retryPolicy = JsonDocument.Parse(JsonSerializer.Serialize(request.RetryPolicy));

        var job = Job.Create(
            request.Name,
            request.Description,
            request.JobType,
            request.ScheduleType,
            request.ScheduleExpression,
            request.IntervalSeconds,
            request.RunAt,
            jobConfiguration,
            retryPolicy,
            request.Timezone,
            request.IsEnabled,
            performedBy,
            request.Tags ?? []);

        await jobRepository.AddAsync(job, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.IsEnabled)
        {
            var schedulerJobId = await schedulerService.ScheduleJobAsync(job, cancellationToken);
            job.SetSchedulerJobId(schedulerJobId, performedBy);
        }

        await jobHistoryRepository.AddAsync(
            JobHistory.Create(job.Id, JobHistoryAction.Created, JsonSerializer.Serialize(request), performedBy, currentUserService.IpAddress),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var status = await schedulerService.GetJobStatusAsync(job, cancellationToken);
        return job.ToDto(status);
    }
}
