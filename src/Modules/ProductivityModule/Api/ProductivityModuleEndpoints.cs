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
using Alphabet.Common.Extensions;
using Alphabet.Modules.ProductivityModule.Api.Hubs;
using Alphabet.Modules.ProductivityModule.Api.Models;
using Alphabet.Modules.ProductivityModule.Api.Resource;
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

        group.MapPost(ApiResource.CreateTodo.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CreateTodo);

        group.MapGet(ApiResource.GetTodos.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetTodos);

        group.MapGet(ApiResource.GetTodoById.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetTodoById);

        group.MapPut(ApiResource.UpdateTodo.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.UpdateTodo);

        group.MapPost(ApiResource.CompleteTodo.Endpoint, async Task<IResult> (
            Guid todoId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CompleteTodoCommand(todoId, true), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Todo completion failed", result.Error)) : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.CompleteTodo);

        group.MapPost(ApiResource.UncompleteTodo.Endpoint, async Task<IResult> (
            Guid todoId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CompleteTodoCommand(todoId, false), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Todo uncomplete failed", result.Error)) : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.UncompleteTodo);

        group.MapDelete(ApiResource.DeleteOrRestoreTodo.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.DeleteOrRestoreTodo);

        group.MapPost(ApiResource.ConvertTodoToTask.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.ConvertTodoToTask);

        group.MapPost(ApiResource.CreateReminderFromTodo.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CreateReminderFromTodo);
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

        group.MapPost(ApiResource.CreateReminder.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CreateReminder);

        group.MapGet(ApiResource.GetReminders.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetReminders);

        group.MapPost(ApiResource.SnoozeReminder.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.SnoozeReminder);

        group.MapPost(ApiResource.DismissReminder.Endpoint, async Task<IResult> (
            Guid reminderId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DismissReminderCommand(reminderId, false), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Reminder dismiss failed", result.Error)) : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.DismissReminder);

        group.MapPost(ApiResource.TestReminder.Endpoint, async Task<IResult> (
            Guid reminderId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DismissReminderCommand(reminderId, true), ct);
            return result.IsFailure ? TypedResults.BadRequest(CreateProblem("Reminder test failed", result.Error)) : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.TestReminder);
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

        group.MapPost(ApiResource.CreateNote.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CreateNote);

        group.MapGet(ApiResource.GetNotes.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetNotes);

        group.MapGet(ApiResource.SearchNotes.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.SearchNotes);

        group.MapPut(ApiResource.UpdateNote.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.UpdateNote);

        group.MapPost(ApiResource.ShareNote.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.ShareNote);

        group.MapGet(ApiResource.GetNoteVersions.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetNoteVersions);

        group.MapGet(ApiResource.ExportNote.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.ExportNote);
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

        group.MapPost(ApiResource.CreateTask.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CreateTask);

        group.MapGet(ApiResource.GetTaskBoard.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetTaskBoard);

        group.MapPatch(ApiResource.UpdateTaskStatus.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.UpdateTaskStatus);

        group.MapPost(ApiResource.AddTimeEntry.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.AddTimeEntry);

        group.MapGet(ApiResource.GetTaskDependencyGraph.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetTaskDependencyGraph);
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

        group.MapPost(ApiResource.CreateEvent.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CreateEvent);

        group.MapGet(ApiResource.GetCalendarView.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetCalendarView);

        group.MapPost(ApiResource.CheckAvailability.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CheckAvailability);

        group.MapPost(ApiResource.RespondToEvent.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.RespondToEvent);

        group.MapGet(ApiResource.ExportCalendarIcal.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.ExportCalendarIcal);

        group.MapPost(ApiResource.SuggestMeetingTimes.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.SuggestMeetingTimes);
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

        notesGroup.MapPost(ApiResource.ConvertNoteToTodo.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.ConvertNoteToTodo);

        notesGroup.MapPost(ApiResource.CreateNotebook.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CreateNotebook);

        var sharedGroup = endpoints.MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Productivity Module - Smart Views")
            .RequireAuthorization();

        sharedGroup.MapGet(ApiResource.GlobalProductivitySearch.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GlobalProductivitySearch);

        sharedGroup.MapGet(ApiResource.GetProductivityDashboardToday.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetProductivityDashboardToday);

        sharedGroup.MapPost(ApiResource.CreateSmartList.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CreateSmartList);
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

        group.MapGet(ApiResource.GetProductivityReport.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetProductivityReport);
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

        group.MapPost(ApiResource.CreateTemplate.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CreateTemplate);

        group.MapPost(ApiResource.InstantiateTemplate.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.InstantiateTemplate);
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
