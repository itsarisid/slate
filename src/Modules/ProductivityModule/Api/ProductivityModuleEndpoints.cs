using System.Text;
using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Features.Productivity.CrossEntity.Commands;
using Alphabet.Application.Features.Productivity.CrossEntity.Queries;
using Alphabet.Application.Features.Productivity.Events.Commands;
using Alphabet.Application.Features.Productivity.Events.Queries;
using Alphabet.Application.Features.Productivity.Notes.Commands;
using Alphabet.Application.Features.Productivity.Notes.Queries;
using Alphabet.Application.Features.Productivity.Reminders.Commands;
using Alphabet.Application.Features.Productivity.Reminders.Queries;
using Alphabet.Application.Features.Productivity.Reports.Queries;
using Alphabet.Application.Features.Productivity.SmartLists.Commands;
using Alphabet.Application.Features.Productivity.Tasks.Commands;
using Alphabet.Application.Features.Productivity.Tasks.Queries;
using Alphabet.Application.Features.Productivity.Templates.Commands;
using Alphabet.Application.Features.Productivity.Todos.Commands;
using Alphabet.Application.Features.Productivity.Todos.Queries;
using Alphabet.Modules.ProductivityModule.Api.Hubs;
using Alphabet.Modules.ProductivityModule.Api.Models;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Alphabet.Modules.ProductivityModule.Api;

/// <summary>
/// Maps productivity module endpoints.
/// </summary>
public static class ProductivityModuleEndpoints
{
    /// <summary>
    /// Registers the productivity module endpoints and hub.
    /// </summary>
    public static IEndpointRouteBuilder MapProductivityModule(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        MapTodos(endpoints, versionSet);
        MapReminders(endpoints, versionSet);
        MapNotes(endpoints, versionSet);
        MapTasks(endpoints, versionSet);
        MapEvents(endpoints, versionSet);
        MapCrossEntity(endpoints, versionSet);
        MapReports(endpoints, versionSet);
        MapTemplates(endpoints, versionSet);
        endpoints.MapHub<ProductivityHub>("/hubs/productivity").RequireAuthorization();
        return endpoints;
    }
    /// <summary>
    /// Map todos.
    /// </summary>

    private static void MapTodos(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/todos")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Productivity Module - Todos")
            .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateTodoRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateTodoCommand(
                request.Title,
                request.Description,
                request.Priority,
                request.DueDate,
                request.ReminderMinutesBefore,
                request.Category,
                request.Tags,
                request.IsRecurring,
                request.RecurrencePattern,
                request.AssignedTo), ct);

            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Todo creation failed", result.Error))
                : TypedResults.Created($"/api/v1/todos/{result.Value.Id}", result.Value);
        })
        .Accepts<CreateTodoRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateTodo")
        .WithSummary("Creates a todo.")
        .WithDescription("Creates a new todo with optional due date, recurrence metadata, tags, category, and assignment.");

        group.MapGet("/", async Task<IResult> (
            [AsParameters] TodoQueryParameters query,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTodosQuery(query.Status, query.Priority, query.Category, query.Tag, query.DueDateFrom, query.DueDateTo, query.AssignedTo, query.Search, query.SortBy, query.SortDirection, query.Page, query.PageSize), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Todo query failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetTodos")
        .WithSummary("Gets todos with advanced filtering.")
        .WithDescription("Returns paginated todos filtered by status, priority, category, tag, due-date range, assignment, and text search.");

        group.MapGet("/{todoId:guid}", async Task<IResult> (
            Guid todoId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTodoByIdQuery(todoId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.NotFound(CreateProblem("Todo not found", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .WithName("GetTodoById")
        .WithSummary("Gets a todo by id.")
        .WithDescription("Returns a single todo item including its recurrence and completion metadata.");

        group.MapPut("/{todoId:guid}", async Task<IResult> (
            Guid todoId,
            [FromBody] UpdateTodoRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateTodoCommand(todoId, request.Title, request.Description, request.Priority, request.DueDate, request.Category, request.AssignedTo, request.ReminderMinutesBefore, request.IsRecurring, request.RecurrencePattern), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Todo update failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Accepts<UpdateTodoRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("UpdateTodo")
        .WithSummary("Updates a todo.")
        .WithDescription("Updates the todo details, due date, assignment, and recurrence information.");

        group.MapPost("/{todoId:guid}/complete", async Task<IResult> (
            Guid todoId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CompleteTodoCommand(todoId, true), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Todo completion failed", result.Error)) : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CompleteTodo")
        .WithSummary("Marks a todo as complete.")
        .WithDescription("Completes the todo and closes any linked reminder records.");

        group.MapPost("/{todoId:guid}/uncomplete", async Task<IResult> (
            Guid todoId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CompleteTodoCommand(todoId, false), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Todo uncomplete failed", result.Error)) : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("UncompleteTodo")
        .WithSummary("Reopens a completed todo.")
        .WithDescription("Moves a completed todo back to the pending state.");

        group.MapDelete("/{todoId:guid}", async Task<IResult> (
            Guid todoId,
            [FromQuery] bool restore,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteTodoCommand(todoId, restore), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Todo delete failed", result.Error)) : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("DeleteOrRestoreTodo")
        .WithSummary("Soft deletes or restores a todo.")
        .WithDescription("Soft deletes a todo by default. When the restore query flag is true, the endpoint restores the item from trash.");

        group.MapPost("/{todoId:guid}/convert-to-task", async Task<IResult> (
            Guid todoId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ConvertTodoToTaskCommand(todoId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Todo conversion failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("ConvertTodoToTask")
        .WithSummary("Converts a todo to a task.")
        .WithDescription("Creates a linked task from the todo while preserving the source todo for traceability.");

        group.MapPost("/{todoId:guid}/create-reminder", async Task<IResult> (
            Guid todoId,
            [FromBody] CreateReminderFromEntityRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateReminderFromEntityCommand("Todo", todoId, request.Title, request.Description, request.ReminderTime), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Reminder creation failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Accepts<CreateReminderFromEntityRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateReminderFromTodo")
        .WithSummary("Creates a reminder from a todo.")
        .WithDescription("Creates a linked reminder record for the selected todo.");
    }
    /// <summary>
    /// Map reminders.
    /// </summary>

    private static void MapReminders(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/reminders")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Productivity Module - Reminders")
            .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateReminderRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateReminderCommand(request.Title, request.Description, request.ReminderTime, request.ReminderType, request.RepeatInterval, request.RepeatCount, request.EndDate, request.SoundEnabled, request.VibrationEnabled, request.SnoozeEnabled, request.SnoozeMinutes, request.LinkedEntityType, request.LinkedEntityId, request.NotificationMethods, request.RecurrencePattern), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Reminder creation failed", result.Error))
                : TypedResults.Created($"/api/v1/reminders/{result.Value.Id}", result.Value);
        })
        .Accepts<CreateReminderRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateReminder")
        .WithSummary("Creates a reminder.")
        .WithDescription("Creates a reminder with scheduling, recurrence, snooze, and delivery-channel settings.");

        group.MapGet("/", async Task<IResult> (
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to,
            [FromQuery] Alphabet.Domain.Enums.ReminderType? type,
            [FromQuery] Alphabet.Domain.Enums.ReminderStatus? status,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRemindersQuery(from, to, type, status), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Reminder query failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("GetReminders")
        .WithSummary("Gets reminders.")
        .WithDescription("Returns reminders filtered by date range, reminder type, and delivery status.");

        group.MapPost("/{reminderId:guid}/snooze", async Task<IResult> (
            Guid reminderId,
            [FromBody] SnoozeReminderRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new SnoozeReminderCommand(reminderId, request.Minutes), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Reminder snooze failed", result.Error)) : TypedResults.Ok();
        })
        .Accepts<SnoozeReminderRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("SnoozeReminder")
        .WithSummary("Snoozes a reminder.")
        .WithDescription("Pushes the reminder forward by the specified number of minutes.");

        group.MapPost("/{reminderId:guid}/dismiss", async Task<IResult> (
            Guid reminderId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DismissReminderCommand(reminderId, false), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Reminder dismiss failed", result.Error)) : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("DismissReminder")
        .WithSummary("Dismisses a reminder.")
        .WithDescription("Marks the reminder as dismissed and suppresses further delivery.");

        group.MapPost("/{reminderId:guid}/test", async Task<IResult> (
            Guid reminderId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DismissReminderCommand(reminderId, true), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Reminder test failed", result.Error)) : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("TestReminder")
        .WithSummary("Triggers a reminder immediately for testing.")
        .WithDescription("Marks the reminder as triggered and dispatches its notification channels immediately.");
    }
    /// <summary>
    /// Map notes.
    /// </summary>

    private static void MapNotes(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/notes")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Productivity Module - Notes")
            .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateNoteRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateNoteCommand(request.Title, request.Content, request.Format, request.Category, request.Tags, request.Color, request.IsPinned, request.IsFavorite, request.NotebookId, request.Collaborators), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Note creation failed", result.Error))
                : TypedResults.Created($"/api/v1/notes/{result.Value.Id}", result.Value);
        })
        .Accepts<CreateNoteRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateNote")
        .WithSummary("Creates a note.")
        .WithDescription("Creates a note with formatting, tags, notebook organization, and collaboration metadata.");

        group.MapGet("/", async Task<IResult> (
            [FromQuery] string? category,
            [FromQuery] string? tag,
            [FromQuery] Guid? notebookId,
            [FromQuery] bool? isPinned,
            [FromQuery] bool? isFavorite,
            [FromQuery] string? search,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new SearchNotesQuery(category, tag, notebookId, isPinned, isFavorite, search, page, pageSize), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Note query failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetNotes")
        .WithSummary("Gets notes.")
        .WithDescription("Returns paginated notes filtered by category, tag, notebook, pinned/favorite flags, and search text.");

        group.MapGet("/search", async Task<IResult> (
            [FromQuery(Name = "q")] string q,
            [FromQuery(Name = "in")] string? searchIn,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new SearchNotesQuery(null, null, null, null, null, q, 1, 20), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Note search failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("SearchNotes")
        .WithSummary("Searches notes.")
        .WithDescription("Searches notes by title and content and returns paginated relevance-style results.");

        group.MapPut("/{noteId:guid}", async Task<IResult> (
            Guid noteId,
            [FromBody] UpdateNoteRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateNoteCommand(noteId, request.Title, request.Content, request.Format, request.Category, request.Color, request.IsPinned, request.IsFavorite, request.NotebookId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Note update failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Accepts<UpdateNoteRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("UpdateNote")
        .WithSummary("Updates a note.")
        .WithDescription("Updates the note content and stores a version snapshot for restore and audit scenarios.");

        group.MapPost("/{noteId:guid}/share", async Task<IResult> (
            Guid noteId,
            [FromBody] ShareNoteRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ShareNoteCommand(noteId, request.Email, request.Permission), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Note share failed", result.Error)) : TypedResults.Ok();
        })
        .Accepts<ShareNoteRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("ShareNote")
        .WithSummary("Shares a note.")
        .WithDescription("Adds a collaborator to the note. Permission is accepted for client compatibility and documented intent.");

        group.MapGet("/{noteId:guid}/versions", async Task<IResult> (
            Guid noteId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetNoteVersionsQuery(noteId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.NotFound(CreateProblem("Note versions not found", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .WithName("GetNoteVersions")
        .WithSummary("Gets note versions.")
        .WithDescription("Returns note version history so clients can show change history or restore workflows.");

        group.MapGet("/{noteId:guid}/export", async Task<IResult> (
            Guid noteId,
            [FromQuery] string format,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetNoteByIdQuery(noteId), ct);
            if (result.IsFailure || result.Value is null)
            {
                return TypedResults.NotFound(CreateProblem("Note was not found", result.Error));
            }

            var note = result.Value;
            var normalizedFormat = string.IsNullOrWhiteSpace(format) ? "markdown" : format.Trim().ToLowerInvariant();
            var extension = normalizedFormat switch
            {
                "html" => "html",
                "pdf" => "txt",
                "docx" => "txt",
                _ => "md"
            };

            return Results.File(Encoding.UTF8.GetBytes(note.Content), "text/plain", $"{note.Title}.{extension}");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("ExportNote")
        .WithSummary("Exports a note.")
        .WithDescription("Exports note content in a simple file form. Markdown is returned by default, while HTML/PDF/DOCX requests use a text fallback in this module baseline.");
    }
    /// <summary>
    /// Map tasks.
    /// </summary>

    private static void MapTasks(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/tasks")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Productivity Module - Tasks")
            .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateTaskRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateTaskCommand(request.Title, request.Description, request.Priority, request.Status, request.DueDate, request.EstimatedHours, request.AssigneeId, request.ReviewerId, request.ParentTaskId, request.ProjectId, request.Dependencies, request.Checklist), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Task creation failed", result.Error))
                : TypedResults.Created($"/api/v1/tasks/{result.Value.Id}", result.Value);
        })
        .Accepts<CreateTaskRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateTask")
        .WithSummary("Creates a task.")
        .WithDescription("Creates an advanced task with ownership, assignee/reviewer, checklist items, dependency ids, and estimation metadata.");

        group.MapGet("/board", async Task<IResult> (
            [FromQuery] Guid? projectId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTaskBoardQuery(projectId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Task board failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetTaskBoard")
        .WithSummary("Gets the task board.")
        .WithDescription("Returns tasks grouped by status so clients can render a Kanban board.");

        group.MapPatch("/{taskId:guid}/status", async Task<IResult> (
            Guid taskId,
            [FromBody] UpdateTaskStatusRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateTaskStatusCommand(taskId, request.Status, request.Comment), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Task status update failed", result.Error)) : TypedResults.Ok();
        })
        .Accepts<UpdateTaskStatusRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("UpdateTaskStatus")
        .WithSummary("Updates task status.")
        .WithDescription("Moves a task between statuses and optionally appends a status-change comment.");

        group.MapPost("/{taskId:guid}/time-entries", async Task<IResult> (
            Guid taskId,
            [FromBody] AddTimeEntryRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AddTimeEntryCommand(taskId, request.StartTime, request.EndTime, request.Description), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Time entry failed", result.Error)) : TypedResults.Ok();
        })
        .Accepts<AddTimeEntryRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AddTimeEntry")
        .WithSummary("Adds a task time entry.")
        .WithDescription("Records tracked time and rolls the duration into the task's actual-hours total.");

        group.MapGet("/{taskId:guid}/dependencies/graph", async Task<IResult> (
            Guid taskId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTaskDependencyGraphQuery(taskId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Dependency graph failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("GetTaskDependencyGraph")
        .WithSummary("Gets task dependencies.")
        .WithDescription("Returns the current task dependency graph as a flat list of dependency ids.");
    }
    /// <summary>
    /// Map events.
    /// </summary>

    private static void MapEvents(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/events")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Productivity Module - Events")
            .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateEventRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateEventCommand(request.Title, request.Description, request.Location, request.IsAllDay, request.StartTime, request.EndTime, request.Timezone, request.Recurrence, request.Attendees, request.Visibility, request.Color, request.ReminderMinutesBefore, request.ConferenceLink), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Event creation failed", result.Error))
                : TypedResults.Created($"/api/v1/events/{result.Value.Id}", result.Value);
        })
        .Accepts<CreateEventRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateEvent")
        .WithSummary("Creates a calendar event.")
        .WithDescription("Creates an event with attendee, reminder, recurrence, and meeting-link support.");

        group.MapGet("/calendar", async Task<IResult> (
            [FromQuery] string? view,
            [FromQuery] DateTimeOffset? date,
            [FromQuery] DateTimeOffset? start,
            [FromQuery] DateTimeOffset? end,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCalendarViewQuery(view ?? "Month", date, start, end), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Calendar view failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetCalendarView")
        .WithSummary("Gets a calendar view.")
        .WithDescription("Returns event data for month, week, day, or agenda-style calendar screens.");

        group.MapPost("/availability", async Task<IResult> (
            [FromBody] CheckAvailabilityRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CheckAvailabilityQuery(request.UserIds, request.DateRange.Start, request.DateRange.End, request.DurationMinutes), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Availability check failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Accepts<CheckAvailabilityRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CheckAvailability")
        .WithSummary("Checks attendee availability.")
        .WithDescription("Returns available slots for the supplied users and date range.");

        group.MapPost("/{eventId:guid}/respond", async Task<IResult> (
            Guid eventId,
            [FromBody] RespondToEventRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RespondToEventCommand(eventId, request.Response), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Event response failed", result.Error)) : TypedResults.Ok();
        })
        .Accepts<RespondToEventRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("RespondToEvent")
        .WithSummary("Responds to an invitation.")
        .WithDescription("Records Accepted, Declined, or Tentative responses for the current user.");

        group.MapGet("/export/ical", async Task<IResult> (
            [FromServices] ISender sender,
            [FromServices] ICalendarExportService calendarExportService,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCalendarViewQuery("Agenda", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMonths(1)), ct);
            if (result.IsFailure || result.Value is null)
            {
                return TypedResults.BadRequest(CreateProblem("Calendar export failed", result.Error));
            }

            var content = await calendarExportService.ExportICalendarAsync(result.Value.Cast<Alphabet.Application.Features.Productivity.Dtos.CalendarEventDto>().ToArray(), ct);
            return Results.File(Encoding.UTF8.GetBytes(content), "text/calendar", "calendar.ics");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("ExportCalendarIcal")
        .WithSummary("Exports calendar data to iCal.")
        .WithDescription("Exports a baseline iCal feed for the next month of events.");

        group.MapPost("/suggest-times", async Task<IResult> (
            [FromBody] SuggestMeetingTimesRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var timezone = request.WorkingHours?.Timezone;
            var result = await sender.Send(new SuggestMeetingTimesCommand(request.Attendees, request.DurationMinutes, request.DateRange.Start, request.DateRange.End, timezone), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Meeting suggestions failed", result.Error))
                : TypedResults.Ok(new { suggestions = result.Value });
        })
        .Accepts<SuggestMeetingTimesRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("SuggestMeetingTimes")
        .WithSummary("Suggests meeting times.")
        .WithDescription("Suggests likely meeting times based on the requested date range and desired meeting duration.");
    }
    /// <summary>
    /// Map cross entity.
    /// </summary>

    private static void MapCrossEntity(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var notesGroup = endpoints.MapGroup("api/v{version:apiVersion}/notes")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Productivity Module - Notes")
            .RequireAuthorization();

        notesGroup.MapPost("/{noteId:guid}/convert-to-todo", async Task<IResult> (
            Guid noteId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ConvertNoteToTodoCommand(noteId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Note conversion failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("ConvertNoteToTodo")
        .WithSummary("Converts a note to a todo.")
        .WithDescription("Creates a todo from the selected note while keeping the original note intact.");

        notesGroup.MapPost("/notebooks", async Task<IResult> (
            [FromBody] CreateNotebookRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateNotebookCommand(request.Name, request.Description, request.Color, request.ParentNotebookId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(CreateProblem("Notebook creation failed", result.Error))
                : TypedResults.Created($"/api/v1/notebooks/{result.Value}", new { id = result.Value });
        })
        .Accepts<CreateNotebookRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateNotebook")
        .WithSummary("Creates a notebook.")
        .WithDescription("Creates a notebook and optionally nests it under a parent notebook.");

        var sharedGroup = endpoints.MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Productivity Module - Smart Views")
            .RequireAuthorization();

        sharedGroup.MapGet("/search", async Task<IResult> (
            [FromQuery(Name = "q")] string q,
            [FromQuery(Name = "types")] string? types,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GlobalSearchQuery(q, string.IsNullOrWhiteSpace(types) ? null : types.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Global search failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("GlobalProductivitySearch")
        .WithSummary("Performs global productivity search.")
        .WithDescription("Searches across todos, notes, tasks, and events using a shared search experience.");

        sharedGroup.MapGet("/dashboard/today", async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetDashboardTodayQuery(), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Dashboard query failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("GetProductivityDashboardToday")
        .WithSummary("Gets the productivity dashboard.")
        .WithDescription("Returns today's overdue work, due work, events, reminders, recent notes, and summary metrics.");

        sharedGroup.MapPost("/smart-lists", async Task<IResult> (
            [FromBody] CreateSmartListRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateSmartListCommand(request.Name, request.EntityType, request.Criteria, request.IsShared), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(CreateProblem("Smart list creation failed", result.Error))
                : TypedResults.Created($"/api/v1/smart-lists/{result.Value}", new { id = result.Value });
        })
        .Accepts<CreateSmartListRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateSmartList")
        .WithSummary("Creates a smart list.")
        .WithDescription("Saves reusable filter criteria for a specific productivity entity type.");
    }
    /// <summary>
    /// Map reports.
    /// </summary>

    private static void MapReports(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/reports")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Productivity Module - Reports")
            .RequireAuthorization();

        group.MapGet("/productivity", async Task<IResult> (
            [FromQuery] string? period,
            [FromQuery] DateTimeOffset? start,
            [FromQuery] DateTimeOffset? end,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetProductivityReportQuery(period ?? "week", start, end), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Productivity report failed", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("GetProductivityReport")
        .WithSummary("Gets a productivity report.")
        .WithDescription("Returns completion, category, and activity metrics for the requested reporting window.");
    }
    /// <summary>
    /// Map templates.
    /// </summary>

    private static void MapTemplates(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/templates")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Productivity Module - Templates")
            .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateTemplateRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateTemplateCommand(request.Name, request.EntityType, request.Description, request.Template), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(CreateProblem("Template creation failed", result.Error))
                : TypedResults.Created($"/api/v1/templates/{result.Value}", new { id = result.Value });
        })
        .Accepts<CreateTemplateRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateTemplate")
        .WithSummary("Creates a reusable productivity template.")
        .WithDescription("Saves a serialized todo, note, task, or event payload for later instantiation.");

        group.MapPost("/{templateId:guid}/instantiate", async Task<IResult> (
            Guid templateId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new InstantiateTemplateCommand(templateId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Template instantiation failed", result.Error))
                : TypedResults.Ok(result.Value.RootElement.Clone());
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("InstantiateTemplate")
        .WithSummary("Instantiates a template.")
        .WithDescription("Returns the saved serialized template payload so clients can prefill creation forms.");
    }
    /// <summary>
    /// Create problem.
    /// </summary>

    private static ProblemDetails CreateProblem(string title, string? detail)
        => new() { Title = title, Detail = detail };
    /// <summary>
    /// Todo query parameters.
    /// </summary>

    private sealed class TodoQueryParameters
    {
        public Alphabet.Domain.Enums.TodoStatus? Status { get; init; }
        public Alphabet.Domain.Enums.Priority? Priority { get; init; }
        public string? Category { get; init; }
        public string? Tag { get; init; }
        public DateTimeOffset? DueDateFrom { get; init; }
        public DateTimeOffset? DueDateTo { get; init; }
        public Guid? AssignedTo { get; init; }
        public string? Search { get; init; }
        public string? SortBy { get; init; }
        public string? SortDirection { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
