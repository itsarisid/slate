using Alphabet.Application.Features.Scheduler.Commands;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Application.Features.Scheduler.Queries;
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

    private static void MapJobs(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/scheduler")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Scheduler Module")
            .RequireAuthorization();

        group.MapPost("/jobs", async Task<Results<Created<JobDto>, BadRequest<ProblemDetails>>> (CreateSchedulerJobRequest request, ISender sender, CancellationToken ct) =>
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
        .Produces<JobDto>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithSummary("Creates a scheduler job.")
        .WithDescription("Creates and schedules a new job for HTTP calls, stored procedures, code execution, or file operations.");

        group.MapGet("/jobs", async Task<Ok<PagedResponseDto<JobDto>>> (
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromQuery] Domain.Enums.JobType? jobType,
            [FromQuery] string? status,
            [FromQuery] string? tag,
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetJobsQuery(pageNumber ?? 1, pageSize ?? 20, jobType, status, tag, search, sortBy, sortDirection), ct);
            return TypedResults.Ok(result);
        })
        .Produces<PagedResponseDto<JobDto>>(StatusCodes.Status200OK)
        .WithSummary("Gets scheduler jobs.")
        .WithDescription("Returns a paged list of jobs with optional filtering, search, and sorting.");

        group.MapGet("/jobs/{jobId:guid}", async Task<Results<Ok<JobDto>, NotFound<ProblemDetails>>> (Guid jobId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetJobByIdQuery(jobId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.NotFound(new ProblemDetails { Title = "Job not found", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Produces<JobDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .WithSummary("Gets a scheduler job by id.");

        group.MapPut("/jobs/{jobId:guid}", async Task<Results<Ok<JobDto>, BadRequest<ProblemDetails>>> (Guid jobId, UpdateSchedulerJobRequest request, ISender sender, CancellationToken ct) =>
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
        .Produces<JobDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithSummary("Updates a scheduler job.");

        group.MapDelete("/jobs/{jobId:guid}", async Task<Results<NoContent, BadRequest<ProblemDetails>>> (Guid jobId, [FromQuery] bool? hardDelete, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteJobCommand(jobId, hardDelete ?? false), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Delete job failed", Detail = result.Error })
                : TypedResults.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithSummary("Deletes a scheduler job.");

        group.MapPatch("/jobs/{jobId:guid}/reschedule", async Task<Results<Ok<JobDto>, BadRequest<ProblemDetails>>> (Guid jobId, RescheduleSchedulerJobRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new RescheduleJobCommand(jobId, request.ScheduleType, request.ScheduleExpression, request.IntervalSeconds, request.RunAt, request.EffectiveFrom), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Reschedule job failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Produces<JobDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithSummary("Reschedules a scheduler job.");

        group.MapPost("/jobs/{jobId:guid}/pause", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid jobId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new PauseJobCommand(jobId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Pause job failed", Detail = result.Error })
                : TypedResults.Ok();
        }).WithSummary("Pauses a scheduler job.");

        group.MapPost("/jobs/{jobId:guid}/resume", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid jobId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ResumeJobCommand(jobId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Resume job failed", Detail = result.Error })
                : TypedResults.Ok();
        }).WithSummary("Resumes a scheduler job.");

        group.MapPost("/jobs/{jobId:guid}/trigger", async Task<Results<Accepted<Guid>, BadRequest<ProblemDetails>>> (Guid jobId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new TriggerJobCommand(jobId), ct);
            return result.IsFailure || result.Value == Guid.Empty
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Trigger job failed", Detail = result.Error })
                : TypedResults.Accepted($"/api/v1/scheduler/executions/{result.Value}", result.Value);
        }).WithSummary("Triggers a job immediately.");

        group.MapGet("/jobs/{jobId:guid}/executions", async Task<Ok<PagedResponseDto<JobExecutionDto>>> (
            Guid jobId,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromQuery] Domain.Enums.ExecutionStatus? status,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetJobExecutionsQuery(jobId, pageNumber ?? 1, pageSize ?? 20, status, fromDate, toDate), ct);
            return TypedResults.Ok(result);
        }).WithSummary("Gets executions for a job.");

        group.MapGet("/executions/{executionId:guid}", async Task<Results<Ok<JobExecutionDto>, NotFound<ProblemDetails>>> (Guid executionId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetExecutionDetailsQuery(executionId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.NotFound(new ProblemDetails { Title = "Execution not found", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        }).WithSummary("Gets execution details.");

        group.MapPost("/executions/{executionId:guid}/cancel", async Task<Results<Ok, Conflict<ProblemDetails>>> (Guid executionId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CancelExecutionCommand(executionId), ct);
            return result.IsFailure
                ? TypedResults.Conflict(new ProblemDetails { Title = "Cancel execution failed", Detail = result.Error })
                : TypedResults.Ok();
        }).WithSummary("Cancels a running execution.");

        group.MapPost("/executions/{executionId:guid}/retry", async Task<Results<Accepted<Guid>, BadRequest<ProblemDetails>>> (Guid executionId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new RetryExecutionCommand(executionId), ct);
            return result.IsFailure || result.Value == Guid.Empty
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Retry execution failed", Detail = result.Error })
                : TypedResults.Accepted($"/api/v1/scheduler/executions/{result.Value}", result.Value);
        }).WithSummary("Retries a failed execution.");

        group.MapPost("/jobs/{jobId:guid}/exclusions", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid jobId, AddJobExclusionRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AddJobExclusionCommand(jobId, request.ExcludedDates, request.ExcludedDaysOfWeek, request.TimeRange?.Start, request.TimeRange?.End), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Add exclusions failed", Detail = result.Error })
                : TypedResults.Ok();
        }).WithSummary("Adds exclusion rules to a job.");

        group.MapPost("/jobs/{jobId:guid}/dependencies", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid jobId, AddJobDependencyRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AddJobDependencyCommand(jobId, request.DependsOnJobIds, request.Condition), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Add dependencies failed", Detail = result.Error })
                : TypedResults.Ok();
        }).WithSummary("Adds dependency rules to a job.");

        group.MapPost("/workflows", async Task<Results<Ok<string>, BadRequest<ProblemDetails>>> (CreateWorkflowRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateWorkflowCommand(request.Name, request.Jobs.Select(x => new CreateWorkflowJobItem(x.JobId, x.DependsOn, x.OnFailure)).ToArray()), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Create workflow failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        }).WithSummary("Creates a scheduler workflow definition.");

        group.MapGet("/dashboard/stats", async Task<Ok<DashboardStatsDto>> (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetDashboardStatsQuery(), ct);
            return TypedResults.Ok(result);
        }).WithSummary("Gets dashboard statistics.");

        group.MapGet("/jobs/failed", async Task<Ok<IReadOnlyList<JobDto>>> ([FromQuery] int? threshold, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetFailedJobsQuery(threshold ?? 3), ct);
            return TypedResults.Ok(result);
        }).WithSummary("Gets jobs with repeated failures.");

        group.MapGet("/executions/timeline", async Task<Ok<IReadOnlyList<TimelinePointDto>>> ([FromQuery] int? hours, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetExecutionTimelineQuery(hours ?? 24), ct);
            return TypedResults.Ok(result);
        }).WithSummary("Gets execution timeline data.");
    }

    private static void MapAdmin(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/scheduler/admin")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Scheduler Module")
            .RequireAuthorization("AdminOnly");

        group.MapPost("/pause-all", async Task<Results<Ok, BadRequest<ProblemDetails>>> (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new PauseAllJobsCommand(), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Pause all jobs failed", Detail = result.Error })
                : TypedResults.Ok();
        }).WithSummary("Pauses all jobs.");

        group.MapPost("/resume-all", async Task<Results<Ok, BadRequest<ProblemDetails>>> (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ResumeAllJobsCommand(), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Resume all jobs failed", Detail = result.Error })
                : TypedResults.Ok();
        }).WithSummary("Resumes all jobs.");

        group.MapDelete("/clear-logs", async Task<Results<Ok<int>, BadRequest<ProblemDetails>>> (ClearExecutionLogsRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ClearOldExecutionLogsCommand(request.OlderThanDays), ct);
            return result.IsFailure || result.Value < 0
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Clear logs failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        }).WithSummary("Clears old execution logs.");

        group.MapGet("/export", async Task<IResult> (ISender sender, CancellationToken ct) =>
        {
            var payload = await sender.Send(new ExportJobConfigurationsQuery(), ct);
            return Results.File(System.Text.Encoding.UTF8.GetBytes(payload), "application/json", "scheduler-jobs-export.json");
        }).WithSummary("Exports job configurations.");

        group.MapPost("/import", async Task<Results<Ok<int>, BadRequest<ProblemDetails>>> (ImportSchedulerJobsRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ImportJobConfigurationsCommand(request.JsonPayload), ct);
            return result.IsFailure || result.Value < 0
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Import jobs failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        }).WithSummary("Imports job configurations.");
    }
}
