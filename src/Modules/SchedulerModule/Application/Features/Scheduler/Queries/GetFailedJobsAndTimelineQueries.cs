using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Scheduler.Queries;

public sealed record GetFailedJobsQuery(int Threshold = 3) : IRequest<IReadOnlyList<JobDto>>;

public sealed record GetExecutionTimelineQuery(int Hours = 24) : IRequest<IReadOnlyList<TimelinePointDto>>;

public sealed record ExportJobConfigurationsQuery : IRequest<string>;

public sealed class GetFailedJobsQueryHandler(IJobRepository jobRepository)
    : IRequestHandler<GetFailedJobsQuery, IReadOnlyList<JobDto>>
{
    public async Task<IReadOnlyList<JobDto>> Handle(GetFailedJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await jobRepository.GetFailedJobsAsync(request.Threshold, cancellationToken);
        return jobs.Select(job => job.ToDto(job.IsPaused ? "Paused" : job.IsEnabled ? "Active" : "Disabled")).ToArray();
    }
}

public sealed class GetExecutionTimelineQueryHandler(IJobExecutionRepository executionRepository)
    : IRequestHandler<GetExecutionTimelineQuery, IReadOnlyList<TimelinePointDto>>
{
    public async Task<IReadOnlyList<TimelinePointDto>> Handle(GetExecutionTimelineQuery request, CancellationToken cancellationToken)
    {
        var points = await executionRepository.GetTimelineAsync(DateTimeOffset.UtcNow.AddHours(-request.Hours), cancellationToken);
        return points.Select(point => new TimelinePointDto(point.Bucket, point.SuccessCount, point.FailedCount, point.RunningCount)).ToArray();
    }
}

public sealed class ExportJobConfigurationsQueryHandler(IJobRepository jobRepository)
    : IRequestHandler<ExportJobConfigurationsQuery, string>
{
    public async Task<string> Handle(ExportJobConfigurationsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await jobRepository.GetAllAsync(cancellationToken);
        var exported = jobs
            .Where(job => !job.IsDeleted)
            .Select(job => job.ToDto(job.IsPaused ? "Paused" : job.IsEnabled ? "Active" : "Disabled"))
            .ToArray();

        return System.Text.Json.JsonSerializer.Serialize(exported, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        });
    }
}
