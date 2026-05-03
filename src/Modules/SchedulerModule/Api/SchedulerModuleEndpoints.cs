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

        group.MapPost("/jobs", async Task<Results<Created<JobDto>, BadRequest<ProblemDetails>>> (
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
        .WithName("CreateSchedulerJob")
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
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetJobsQuery(pageNumber ?? 1, pageSize ?? 20, jobType, status, tag, search, sortBy, sortDirection), ct);
            return TypedResults.Ok(result);
        })
        .Produces<PagedResponseDto<JobDto>>(StatusCodes.Status200OK)
        .WithName("GetSchedulerJobs")
        .WithSummary("Gets scheduler jobs.")
        .WithDescription("Returns a paged list of jobs with optional filtering, search, sorting, and job-type or state restrictions.");

        group.MapGet("/jobs/{jobId:guid}", async Task<Results<Ok<JobDto>, NotFound<ProblemDetails>>> (
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
        .WithName("GetSchedulerJobById")
        .WithSummary("Gets a scheduler job by id.")
        .WithDescription("Returns the full scheduler job definition, current state, retry policy, and schedule details for the requested job.");

        group.MapPut("/jobs/{jobId:guid}", async Task<Results<Ok<JobDto>, BadRequest<ProblemDetails>>> (
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
        .WithName("UpdateSchedulerJob")
        .WithSummary("Updates a scheduler job.")
        .WithDescription("Updates an existing scheduler job. If the schedule or enablement changes, the underlying scheduler registration is updated as well.");

        group.MapDelete("/jobs/{jobId:guid}", async Task<Results<NoContent, BadRequest<ProblemDetails>>> (
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
        .WithName("DeleteSchedulerJob")
        .WithSummary("Deletes a scheduler job.")
        .WithDescription("Soft deletes a scheduler job by default while preserving history. Set hardDelete to true to remove the persisted record.");

        group.MapPatch("/jobs/{jobId:guid}/reschedule", async Task<Results<Ok<JobDto>, BadRequest<ProblemDetails>>> (
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
        .WithName("RescheduleSchedulerJob")
        .WithSummary("Reschedules a scheduler job.")
        .WithDescription("Changes the schedule type and timing configuration for an existing job and reapplies the schedule in the active scheduler provider.");

        group.MapPost("/jobs/{jobId:guid}/pause", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithName("PauseSchedulerJob")
        .WithSummary("Pauses a scheduler job.")
        .WithDescription("Stops future scheduled runs for the selected job without deleting its definition or execution history.");

        group.MapPost("/jobs/{jobId:guid}/resume", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithName("ResumeSchedulerJob")
        .WithSummary("Resumes a scheduler job.")
        .WithDescription("Restores a paused scheduler job and re-registers its active schedule in the configured scheduler provider.");

        group.MapPost("/jobs/{jobId:guid}/trigger", async Task<Results<Accepted<Guid>, BadRequest<ProblemDetails>>> (
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
        .WithName("TriggerSchedulerJob")
        .WithSummary("Triggers a job immediately.")
        .WithDescription("Queues the selected job for immediate execution and returns the new execution id for tracking.");

        group.MapGet("/jobs/{jobId:guid}/executions", async Task<Ok<PagedResponseDto<JobExecutionDto>>> (
            Guid jobId,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromQuery] Domain.Enums.ExecutionStatus? status,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetJobExecutionsQuery(jobId, pageNumber ?? 1, pageSize ?? 20, status, fromDate, toDate), ct);
            return TypedResults.Ok(result);
        })
        .Produces<PagedResponseDto<JobExecutionDto>>(StatusCodes.Status200OK)
        .WithName("GetSchedulerJobExecutions")
        .WithSummary("Gets executions for a job.")
        .WithDescription("Returns paged execution history for the selected job with optional status and date-range filtering.");

        group.MapGet("/executions/{executionId:guid}", async Task<Results<Ok<JobExecutionDto>, NotFound<ProblemDetails>>> (
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
        .WithName("GetSchedulerExecutionDetails")
        .WithSummary("Gets execution details.")
        .WithDescription("Returns the detailed execution record for a specific scheduler run, including output, duration, retries, and final status.");

        group.MapPost("/executions/{executionId:guid}/cancel", async Task<Results<Ok, Conflict<ProblemDetails>>> (
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
        .WithName("CancelSchedulerExecution")
        .WithSummary("Cancels a running execution.")
        .WithDescription("Attempts to cancel a still-running scheduler execution. A conflict is returned when the execution can no longer be cancelled.");

        group.MapPost("/executions/{executionId:guid}/retry", async Task<Results<Accepted<Guid>, BadRequest<ProblemDetails>>> (
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
        .WithName("RetrySchedulerExecution")
        .WithSummary("Retries a failed execution.")
        .WithDescription("Queues a new execution for the same job using the selected failed execution as the retry parent.");

        group.MapPost("/jobs/{jobId:guid}/exclusions", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithName("AddSchedulerJobExclusions")
        .WithSummary("Adds exclusion rules to a job.")
        .WithDescription("Stores excluded dates, days of week, and optional blocked time windows that should prevent the job from running.");

        group.MapPost("/jobs/{jobId:guid}/dependencies", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithName("AddSchedulerJobDependencies")
        .WithSummary("Adds dependency rules to a job.")
        .WithDescription("Defines prerequisite jobs and dependency conditions that should be satisfied before the selected job can execute.");

        group.MapPost("/workflows", async Task<Results<Ok<string>, BadRequest<ProblemDetails>>> (
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
        .WithName("CreateSchedulerWorkflow")
        .WithSummary("Creates a scheduler workflow definition.")
        .WithDescription("Creates a chained workflow definition made up of multiple jobs and dependency relationships.");

        group.MapGet("/dashboard/stats", async Task<Ok<DashboardStatsDto>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetDashboardStatsQuery(), ct);
            return TypedResults.Ok(result);
        })
        .Produces<DashboardStatsDto>(StatusCodes.Status200OK)
        .WithName("GetSchedulerDashboardStats")
        .WithSummary("Gets dashboard statistics.")
        .WithDescription("Returns scheduler health and activity metrics including totals, success rate, and upcoming executions.");

        group.MapGet("/jobs/failed", async Task<Ok<IReadOnlyList<JobDto>>> (
            [FromQuery] int? threshold,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetFailedJobsQuery(threshold ?? 3), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<JobDto>>(StatusCodes.Status200OK)
        .WithName("GetSchedulerFailedJobs")
        .WithSummary("Gets jobs with repeated failures.")
        .WithDescription("Returns jobs whose consecutive failure count meets or exceeds the supplied threshold.");

        group.MapGet("/executions/timeline", async Task<Ok<IReadOnlyList<TimelinePointDto>>> (
            [FromQuery] int? hours,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetExecutionTimelineQuery(hours ?? 24), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<TimelinePointDto>>(StatusCodes.Status200OK)
        .WithName("GetSchedulerExecutionTimeline")
        .WithSummary("Gets execution timeline data.")
        .WithDescription("Returns time-bucketed execution counts for success, failure, and running states to power monitoring charts.");
    }

    private static void MapAdmin(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/scheduler/admin")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Scheduler Module")
            .RequireAuthorization("AdminOnly");

        group.MapPost("/pause-all", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithName("PauseAllSchedulerJobs")
        .WithSummary("Pauses all jobs.")
        .WithDescription("Pauses every active scheduler job in the system. This endpoint is intended for administrative maintenance windows.");

        group.MapPost("/resume-all", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithName("ResumeAllSchedulerJobs")
        .WithSummary("Resumes all jobs.")
        .WithDescription("Resumes every persisted scheduler job and re-registers active schedules in the configured scheduler provider.");

        group.MapDelete("/clear-logs", async Task<Results<Ok<int>, BadRequest<ProblemDetails>>> (
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
        .WithName("ClearSchedulerExecutionLogs")
        .WithSummary("Clears old execution logs.")
        .WithDescription("Deletes persisted execution records older than the requested number of days and returns the number of deleted items.");

        group.MapGet("/export", async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var payload = await sender.Send(new ExportJobConfigurationsQuery(), ct);
            return Results.File(System.Text.Encoding.UTF8.GetBytes(payload), "application/json", "scheduler-jobs-export.json");
        })
        .Produces(StatusCodes.Status200OK, contentType: "application/json")
        .WithName("ExportSchedulerJobs")
        .WithSummary("Exports job configurations.")
        .WithDescription("Exports the current scheduler job definitions as a JSON file for backup, migration, or review.");

        group.MapPost("/import", async Task<Results<Ok<int>, BadRequest<ProblemDetails>>> (
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
        .WithName("ImportSchedulerJobs")
        .WithSummary("Imports job configurations.")
        .WithDescription("Creates scheduler jobs from the supplied JSON payload and returns the number of successfully imported jobs.");
    }
}
