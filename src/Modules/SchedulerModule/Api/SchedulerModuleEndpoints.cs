using Alphabet.Modules.SchedulerModule.Api.Resource;
using Alphabet.Common.Extensions;
using Alphabet.Application.Features.Scheduler.Commands;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Application.Features.Scheduler.Queries;
using Alphabet.Domain.Enums;
using Alphabet.Modules.SchedulerModule.Api.Models;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Alphabet.Modules.SchedulerModule.Api;

/// <summary>
/// Maps the scheduler module API surface.
/// </summary>
public static class SchedulerModuleEndpoints
{
    /// <summary>
    /// Registers scheduler endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapSchedulerModule(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        MapJobs(endpoints, versionSet);
        MapAdmin(endpoints, versionSet);
        return endpoints;
    }
    /// <summary>
    /// Map jobs.
    /// </summary>

    private static void MapJobs(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/scheduler")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Scheduler Module")
            .RequireAuthorization();

        group.MapPost(ApiResource.CreateSchedulerJob.Endpoint, async Task<Results<Created<JobDto>, BadRequest<ProblemDetails>>> (
            [FromBody] CreateSchedulerJobRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new CreateJobCommand(
                    request.Name,
                    request.Description,
                    request.JobType,
                    request.ScheduleType,
                    request.ScheduleExpression,
                    request.IntervalSeconds,
                    request.RunAt,
                    request.JobConfiguration,
                    request.RetryPolicy,
                    request.TimeoutSeconds,
                    request.Timezone,
                    request.IsEnabled,
                    request.Tags,
                    request.CreatedBy),
                ct);

            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Create job failed", Detail = result.Error })
                : TypedResults.Created($"/api/v1/scheduler/jobs/{result.Value.Id}", result.Value);
        })
        .Accepts<CreateSchedulerJobRequest>("application/json")
        .Produces<JobDto>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.CreateSchedulerJob);

        group.MapGet(ApiResource.GetSchedulerJobs.Endpoint, async Task<Ok<PagedResponseDto<JobDto>>> (
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromQuery] JobType? jobType,
            [FromQuery] string? status,
            [FromQuery] string? tag,
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetJobsQuery(pageNumber ?? 1, pageSize ?? 20, jobType, status, tag, search, sortBy, sortDirection), ct);
            return TypedResults.Ok(result);
        })
        .Produces<PagedResponseDto<JobDto>>(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetSchedulerJobs);

        group.MapGet(ApiResource.GetSchedulerJobById.Endpoint, async Task<Results<Ok<JobDto>, NotFound<ProblemDetails>>> (
            Guid jobId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetJobByIdQuery(jobId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.NotFound(new ProblemDetails { Title = "Job not found", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Produces<JobDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .WithDocumentation(ApiResource.GetSchedulerJobById);

        group.MapPut(ApiResource.UpdateSchedulerJob.Endpoint, async Task<Results<Ok<JobDto>, BadRequest<ProblemDetails>>> (
            Guid jobId,
            [FromBody] UpdateSchedulerJobRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new UpdateJobCommand(
                    jobId,
                    request.Name,
                    request.Description,
                    request.JobType,
                    request.ScheduleType,
                    request.ScheduleExpression,
                    request.IntervalSeconds,
                    request.RunAt,
                    request.JobConfiguration,
                    request.RetryPolicy,
                    request.TimeoutSeconds,
                    request.Timezone,
                    request.IsEnabled,
                    request.Tags),
                ct);

            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Update job failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Accepts<UpdateSchedulerJobRequest>("application/json")
        .Produces<JobDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.UpdateSchedulerJob);

        group.MapDelete(ApiResource.DeleteSchedulerJob.Endpoint, async Task<Results<NoContent, BadRequest<ProblemDetails>>> (
            Guid jobId,
            [FromQuery] bool? hardDelete,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteJobCommand(jobId, hardDelete ?? false), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Delete job failed", Detail = result.Error })
                : TypedResults.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.DeleteSchedulerJob);

        group.MapPatch(ApiResource.RescheduleSchedulerJob.Endpoint, async Task<Results<Ok<JobDto>, BadRequest<ProblemDetails>>> (
            Guid jobId,
            [FromBody] RescheduleSchedulerJobRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RescheduleJobCommand(jobId, request.ScheduleType, request.ScheduleExpression, request.IntervalSeconds, request.RunAt, request.EffectiveFrom), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Reschedule job failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Accepts<RescheduleSchedulerJobRequest>("application/json")
        .Produces<JobDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.RescheduleSchedulerJob);

        group.MapPost(ApiResource.PauseSchedulerJob.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid jobId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new PauseJobCommand(jobId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Pause job failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.PauseSchedulerJob);

        group.MapPost(ApiResource.ResumeSchedulerJob.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid jobId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ResumeJobCommand(jobId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Resume job failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.ResumeSchedulerJob);

        group.MapPost(ApiResource.TriggerSchedulerJob.Endpoint, async Task<Results<Accepted<Guid>, BadRequest<ProblemDetails>>> (
            Guid jobId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new TriggerJobCommand(jobId), ct);
            return result.IsFailure || result.Value == Guid.Empty
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Trigger job failed", Detail = result.Error })
                : TypedResults.Accepted($"/api/v1/scheduler/executions/{result.Value}", result.Value);
        })
        .Produces<Guid>(StatusCodes.Status202Accepted)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.TriggerSchedulerJob);

        group.MapGet(ApiResource.GetSchedulerJobExecutions.Endpoint, async Task<Ok<PagedResponseDto<JobExecutionDto>>> (
            Guid jobId,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromQuery] ExecutionStatus? status,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetJobExecutionsQuery(jobId, pageNumber ?? 1, pageSize ?? 20, status, fromDate, toDate), ct);
            return TypedResults.Ok(result);
        })
        .Produces<PagedResponseDto<JobExecutionDto>>(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetSchedulerJobExecutions);

        group.MapGet(ApiResource.GetSchedulerExecutionDetails.Endpoint, async Task<Results<Ok<JobExecutionDto>, NotFound<ProblemDetails>>> (
            Guid executionId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetExecutionDetailsQuery(executionId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.NotFound(new ProblemDetails { Title = "Execution not found", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Produces<JobExecutionDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .WithDocumentation(ApiResource.GetSchedulerExecutionDetails);

        group.MapPost(ApiResource.CancelSchedulerExecution.Endpoint, async Task<Results<Ok, Conflict<ProblemDetails>>> (
            Guid executionId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CancelExecutionCommand(executionId), ct);
            return result.IsFailure
                ? TypedResults.Conflict(new ProblemDetails { Title = "Cancel execution failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
        .WithDocumentation(ApiResource.CancelSchedulerExecution);

        group.MapPost(ApiResource.RetrySchedulerExecution.Endpoint, async Task<Results<Accepted<Guid>, BadRequest<ProblemDetails>>> (
            Guid executionId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RetryExecutionCommand(executionId), ct);
            return result.IsFailure || result.Value == Guid.Empty
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Retry execution failed", Detail = result.Error })
                : TypedResults.Accepted($"/api/v1/scheduler/executions/{result.Value}", result.Value);
        })
        .Produces<Guid>(StatusCodes.Status202Accepted)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.RetrySchedulerExecution);

        group.MapPost(ApiResource.AddSchedulerJobExclusions.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid jobId,
            [FromBody] AddJobExclusionRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AddJobExclusionCommand(jobId, request.ExcludedDates, request.ExcludedDaysOfWeek, request.TimeRange?.Start, request.TimeRange?.End), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Add exclusions failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<AddJobExclusionRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.AddSchedulerJobExclusions);

        group.MapPost(ApiResource.AddSchedulerJobDependencies.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid jobId,
            [FromBody] AddJobDependencyRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AddJobDependencyCommand(jobId, request.DependsOnJobIds, request.Condition), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Add dependencies failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<AddJobDependencyRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.AddSchedulerJobDependencies);

        group.MapPost(ApiResource.CreateSchedulerWorkflow.Endpoint, async Task<Results<Ok<string>, BadRequest<ProblemDetails>>> (
            [FromBody] CreateWorkflowRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateWorkflowCommand(request.Name, request.Jobs.Select(x => new CreateWorkflowJobItem(x.JobId, x.DependsOn, x.OnFailure)).ToArray()), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Create workflow failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Accepts<CreateWorkflowRequest>("application/json")
        .Produces<string>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.CreateSchedulerWorkflow);

        group.MapGet(ApiResource.GetSchedulerDashboardStats.Endpoint, async Task<Ok<DashboardStatsDto>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetDashboardStatsQuery(), ct);
            return TypedResults.Ok(result);
        })
        .Produces<DashboardStatsDto>(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetSchedulerDashboardStats);

        group.MapGet(ApiResource.GetSchedulerFailedJobs.Endpoint, async Task<Ok<IReadOnlyList<JobDto>>> (
            [FromQuery] int? threshold,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetFailedJobsQuery(threshold ?? 3), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<JobDto>>(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetSchedulerFailedJobs);

        group.MapGet(ApiResource.GetSchedulerExecutionTimeline.Endpoint, async Task<Ok<IReadOnlyList<TimelinePointDto>>> (
            [FromQuery] int? hours,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetExecutionTimelineQuery(hours ?? 24), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<TimelinePointDto>>(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetSchedulerExecutionTimeline);
    }
    /// <summary>
    /// Map admin.
    /// </summary>

    private static void MapAdmin(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/scheduler/admin")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Scheduler Module")
            .RequireAuthorization("AdminOnly");

        group.MapPost(ApiResource.PauseAllSchedulerJobs.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new PauseAllJobsCommand(), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Pause all jobs failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.PauseAllSchedulerJobs);

        group.MapPost(ApiResource.ResumeAllSchedulerJobs.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ResumeAllJobsCommand(), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Resume all jobs failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.ResumeAllSchedulerJobs);

        group.MapDelete(ApiResource.ClearSchedulerExecutionLogs.Endpoint, async Task<Results<Ok<int>, BadRequest<ProblemDetails>>> (
            [FromBody] ClearExecutionLogsRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ClearOldExecutionLogsCommand(request.OlderThanDays), ct);
            return result.IsFailure || result.Value < 0
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Clear logs failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Accepts<ClearExecutionLogsRequest>("application/json")
        .Produces<int>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.ClearSchedulerExecutionLogs);

        group.MapGet(ApiResource.ExportSchedulerJobs.Endpoint, async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var payload = await sender.Send(new ExportJobConfigurationsQuery(), ct);
            return Results.File(System.Text.Encoding.UTF8.GetBytes(payload), "application/json", "scheduler-jobs-export.json");
        })
        .Produces(StatusCodes.Status200OK, contentType: "application/json")
        .WithDocumentation(ApiResource.ExportSchedulerJobs);

        group.MapPost(ApiResource.ImportSchedulerJobs.Endpoint, async Task<Results<Ok<int>, BadRequest<ProblemDetails>>> (
            [FromBody] ImportSchedulerJobsRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ImportJobConfigurationsCommand(request.JsonPayload), ct);
            return result.IsFailure || result.Value < 0
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Import jobs failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Accepts<ImportSchedulerJobsRequest>("application/json")
        .Produces<int>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.ImportSchedulerJobs);
    }
}
