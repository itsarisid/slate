using Alphabet.Common.Models;

namespace Alphabet.Modules.SchedulerModule.Api.Resource;

public static class ApiResource
{
    // ── MapJobs endpoints ──────────────────────────────────────────────

    public static EndpointDetails CreateSchedulerJob => new()
    {
        Endpoint = "/jobs",
        Name = "CreateSchedulerJob",
        Summary = "Creates a scheduler job.",
        Description = "Creates and schedules a new job for HTTP calls, stored procedures, code execution, or file operations."
    };

    public static EndpointDetails GetSchedulerJobs => new()
    {
        Endpoint = "/jobs",
        Name = "GetSchedulerJobs",
        Summary = "Gets scheduler jobs.",
        Description = "Returns a paged list of jobs with optional filtering, search, sorting, and job-type or state restrictions."
    };

    public static EndpointDetails GetSchedulerJobById => new()
    {
        Endpoint = "/jobs/{jobId:guid}",
        Name = "GetSchedulerJobById",
        Summary = "Gets a scheduler job by id.",
        Description = "Returns the full scheduler job definition, current state, retry policy, and schedule details for the requested job."
    };

    public static EndpointDetails UpdateSchedulerJob => new()
    {
        Endpoint = "/jobs/{jobId:guid}",
        Name = "UpdateSchedulerJob",
        Summary = "Updates a scheduler job.",
        Description = "Updates an existing scheduler job. If the schedule or enablement changes, the underlying scheduler registration is updated as well."
    };

    public static EndpointDetails DeleteSchedulerJob => new()
    {
        Endpoint = "/jobs/{jobId:guid}",
        Name = "DeleteSchedulerJob",
        Summary = "Deletes a scheduler job.",
        Description = "Soft deletes a scheduler job by default while preserving history. Set hardDelete to true to remove the persisted record."
    };

    public static EndpointDetails RescheduleSchedulerJob => new()
    {
        Endpoint = "/jobs/{jobId:guid}/reschedule",
        Name = "RescheduleSchedulerJob",
        Summary = "Reschedules a scheduler job.",
        Description = "Changes the schedule type and timing configuration for an existing job and reapplies the schedule in the active scheduler provider."
    };

    public static EndpointDetails PauseSchedulerJob => new()
    {
        Endpoint = "/jobs/{jobId:guid}/pause",
        Name = "PauseSchedulerJob",
        Summary = "Pauses a scheduler job.",
        Description = "Stops future scheduled runs for the selected job without deleting its definition or execution history."
    };

    public static EndpointDetails ResumeSchedulerJob => new()
    {
        Endpoint = "/jobs/{jobId:guid}/resume",
        Name = "ResumeSchedulerJob",
        Summary = "Resumes a scheduler job.",
        Description = "Restores a paused scheduler job and re-registers its active schedule in the configured scheduler provider."
    };

    public static EndpointDetails TriggerSchedulerJob => new()
    {
        Endpoint = "/jobs/{jobId:guid}/trigger",
        Name = "TriggerSchedulerJob",
        Summary = "Triggers a job immediately.",
        Description = "Queues the selected job for immediate execution and returns the new execution id for tracking."
    };

    public static EndpointDetails GetSchedulerJobExecutions => new()
    {
        Endpoint = "/jobs/{jobId:guid}/executions",
        Name = "GetSchedulerJobExecutions",
        Summary = "Gets executions for a job.",
        Description = "Returns paged execution history for the selected job with optional status and date-range filtering."
    };

    public static EndpointDetails GetSchedulerExecutionDetails => new()
    {
        Endpoint = "/executions/{executionId:guid}",
        Name = "GetSchedulerExecutionDetails",
        Summary = "Gets execution details.",
        Description = "Returns the detailed execution record for a specific scheduler run, including output, duration, retries, and final status."
    };

    public static EndpointDetails CancelSchedulerExecution => new()
    {
        Endpoint = "/executions/{executionId:guid}/cancel",
        Name = "CancelSchedulerExecution",
        Summary = "Cancels a running execution.",
        Description = "Attempts to cancel a still-running scheduler execution. A conflict is returned when the execution can no longer be cancelled."
    };

    public static EndpointDetails RetrySchedulerExecution => new()
    {
        Endpoint = "/executions/{executionId:guid}/retry",
        Name = "RetrySchedulerExecution",
        Summary = "Retries a failed execution.",
        Description = "Queues a new execution for the same job using the selected failed execution as the retry parent."
    };

    public static EndpointDetails AddSchedulerJobExclusions => new()
    {
        Endpoint = "/jobs/{jobId:guid}/exclusions",
        Name = "AddSchedulerJobExclusions",
        Summary = "Adds exclusion rules to a job.",
        Description = "Stores excluded dates, days of week, and optional blocked time windows that should prevent the job from running."
    };

    public static EndpointDetails AddSchedulerJobDependencies => new()
    {
        Endpoint = "/jobs/{jobId:guid}/dependencies",
        Name = "AddSchedulerJobDependencies",
        Summary = "Adds dependency rules to a job.",
        Description = "Defines prerequisite jobs and dependency conditions that should be satisfied before the selected job can execute."
    };

    public static EndpointDetails CreateSchedulerWorkflow => new()
    {
        Endpoint = "/workflows",
        Name = "CreateSchedulerWorkflow",
        Summary = "Creates a scheduler workflow definition.",
        Description = "Creates a chained workflow definition made up of multiple jobs and dependency relationships."
    };

    public static EndpointDetails GetSchedulerDashboardStats => new()
    {
        Endpoint = "/dashboard/stats",
        Name = "GetSchedulerDashboardStats",
        Summary = "Gets dashboard statistics.",
        Description = "Returns scheduler health and activity metrics including totals, success rate, and upcoming executions."
    };

    public static EndpointDetails GetSchedulerFailedJobs => new()
    {
        Endpoint = "/jobs/failed",
        Name = "GetSchedulerFailedJobs",
        Summary = "Gets jobs with repeated failures.",
        Description = "Returns jobs whose consecutive failure count meets or exceeds the supplied threshold."
    };

    public static EndpointDetails GetSchedulerExecutionTimeline => new()
    {
        Endpoint = "/executions/timeline",
        Name = "GetSchedulerExecutionTimeline",
        Summary = "Gets execution timeline data.",
        Description = "Returns time-bucketed execution counts for success, failure, and running states to power monitoring charts."
    };

    // ── MapAdmin endpoints ─────────────────────────────────────────────

    public static EndpointDetails PauseAllSchedulerJobs => new()
    {
        Endpoint = "/pause-all",
        Name = "PauseAllSchedulerJobs",
        Summary = "Pauses all jobs.",
        Description = "Pauses every active scheduler job in the system. This endpoint is intended for administrative maintenance windows."
    };

    public static EndpointDetails ResumeAllSchedulerJobs => new()
    {
        Endpoint = "/resume-all",
        Name = "ResumeAllSchedulerJobs",
        Summary = "Resumes all jobs.",
        Description = "Resumes every persisted scheduler job and re-registers active schedules in the configured scheduler provider."
    };

    public static EndpointDetails ClearSchedulerExecutionLogs => new()
    {
        Endpoint = "/clear-logs",
        Name = "ClearSchedulerExecutionLogs",
        Summary = "Clears old execution logs.",
        Description = "Deletes persisted execution records older than the requested number of days and returns the number of deleted items."
    };

    public static EndpointDetails ExportSchedulerJobs => new()
    {
        Endpoint = "/export",
        Name = "ExportSchedulerJobs",
        Summary = "Exports job configurations.",
        Description = "Exports the current scheduler job definitions as a JSON file for backup, migration, or review."
    };

    public static EndpointDetails ImportSchedulerJobs => new()
    {
        Endpoint = "/import",
        Name = "ImportSchedulerJobs",
        Summary = "Imports job configurations.",
        Description = "Creates scheduler jobs from the supplied JSON payload and returns the number of successfully imported jobs."
    };

}
