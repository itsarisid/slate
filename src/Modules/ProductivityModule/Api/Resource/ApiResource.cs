using Alphabet.Common.Models;

namespace Alphabet.Modules.ProductivityModule.Api.Resource;

public static class ApiResource
{
    // ── Todos ──────────────────────────────────────────────────────────────

    public static EndpointDetails CreateTodo => new()
    {
        Endpoint = "/",
        Name = "CreateTodo",
        Summary = "Creates a todo.",
        Description = "Creates a new todo with optional due date, recurrence metadata, tags, category, and assignment."
    };

    public static EndpointDetails GetTodos => new()
    {
        Endpoint = "/",
        Name = "GetTodos",
        Summary = "Gets todos with advanced filtering.",
        Description = "Returns paginated todos filtered by status, priority, category, tag, due-date range, assignment, and text search."
    };

    public static EndpointDetails GetTodoById => new()
    {
        Endpoint = "/{todoId:guid}",
        Name = "GetTodoById",
        Summary = "Gets a todo by id.",
        Description = "Returns a single todo item including its recurrence and completion metadata."
    };

    public static EndpointDetails UpdateTodo => new()
    {
        Endpoint = "/{todoId:guid}",
        Name = "UpdateTodo",
        Summary = "Updates a todo.",
        Description = "Updates the todo details, due date, assignment, and recurrence information."
    };

    public static EndpointDetails CompleteTodo => new()
    {
        Endpoint = "/{todoId:guid}/complete",
        Name = "CompleteTodo",
        Summary = "Marks a todo as complete.",
        Description = "Completes the todo and closes any linked reminder records."
    };

    public static EndpointDetails UncompleteTodo => new()
    {
        Endpoint = "/{todoId:guid}/uncomplete",
        Name = "UncompleteTodo",
        Summary = "Reopens a completed todo.",
        Description = "Moves a completed todo back to the pending state."
    };

    public static EndpointDetails DeleteOrRestoreTodo => new()
    {
        Endpoint = "/{todoId:guid}",
        Name = "DeleteOrRestoreTodo",
        Summary = "Soft deletes or restores a todo.",
        Description = "Soft deletes a todo by default. When the restore query flag is true, the endpoint restores the item from trash."
    };

    public static EndpointDetails ConvertTodoToTask => new()
    {
        Endpoint = "/{todoId:guid}/convert-to-task",
        Name = "ConvertTodoToTask",
        Summary = "Converts a todo to a task.",
        Description = "Creates a linked task from the todo while preserving the source todo for traceability."
    };

    public static EndpointDetails CreateReminderFromTodo => new()
    {
        Endpoint = "/{todoId:guid}/create-reminder",
        Name = "CreateReminderFromTodo",
        Summary = "Creates a reminder from a todo.",
        Description = "Creates a linked reminder record for the selected todo."
    };

    // ── Reminders ────────────────────────────────────────────────────────

    public static EndpointDetails CreateReminder => new()
    {
        Endpoint = "/",
        Name = "CreateReminder",
        Summary = "Creates a reminder.",
        Description = "Creates a reminder with scheduling, recurrence, snooze, and delivery-channel settings."
    };

    public static EndpointDetails GetReminders => new()
    {
        Endpoint = "/",
        Name = "GetReminders",
        Summary = "Gets reminders.",
        Description = "Returns reminders filtered by date range, reminder type, and delivery status."
    };

    public static EndpointDetails SnoozeReminder => new()
    {
        Endpoint = "/{reminderId:guid}/snooze",
        Name = "SnoozeReminder",
        Summary = "Snoozes a reminder.",
        Description = "Pushes the reminder forward by the specified number of minutes."
    };

    public static EndpointDetails DismissReminder => new()
    {
        Endpoint = "/{reminderId:guid}/dismiss",
        Name = "DismissReminder",
        Summary = "Dismisses a reminder.",
        Description = "Marks the reminder as dismissed and suppresses further delivery."
    };

    public static EndpointDetails TestReminder => new()
    {
        Endpoint = "/{reminderId:guid}/test",
        Name = "TestReminder",
        Summary = "Triggers a reminder immediately for testing.",
        Description = "Marks the reminder as triggered and dispatches its notification channels immediately."
    };

    // ── Notes ─────────────────────────────────────────────────────────────

    public static EndpointDetails CreateNote => new()
    {
        Endpoint = "/",
        Name = "CreateNote",
        Summary = "Creates a note.",
        Description = "Creates a note with formatting, tags, notebook organization, and collaboration metadata."
    };

    public static EndpointDetails GetNotes => new()
    {
        Endpoint = "/",
        Name = "GetNotes",
        Summary = "Gets notes.",
        Description = "Returns paginated notes filtered by category, tag, notebook, pinned/favorite flags, and search text."
    };

    public static EndpointDetails SearchNotes => new()
    {
        Endpoint = "/search",
        Name = "SearchNotes",
        Summary = "Searches notes.",
        Description = "Searches notes by title and content and returns paginated relevance-style results."
    };

    public static EndpointDetails UpdateNote => new()
    {
        Endpoint = "/{noteId:guid}",
        Name = "UpdateNote",
        Summary = "Updates a note.",
        Description = "Updates the note content and stores a version snapshot for restore and audit scenarios."
    };

    public static EndpointDetails ShareNote => new()
    {
        Endpoint = "/{noteId:guid}/share",
        Name = "ShareNote",
        Summary = "Shares a note.",
        Description = "Adds a collaborator to the note. Permission is accepted for client compatibility and documented intent."
    };

    public static EndpointDetails GetNoteVersions => new()
    {
        Endpoint = "/{noteId:guid}/versions",
        Name = "GetNoteVersions",
        Summary = "Gets note versions.",
        Description = "Returns note version history so clients can show change history or restore workflows."
    };

    public static EndpointDetails ExportNote => new()
    {
        Endpoint = "/{noteId:guid}/export",
        Name = "ExportNote",
        Summary = "Exports a note.",
        Description = "Exports note content in a simple file form. Markdown is returned by default, while HTML/PDF/DOCX requests use a text fallback in this module baseline."
    };

    // ── Tasks ─────────────────────────────────────────────────────────────

    public static EndpointDetails CreateTask => new()
    {
        Endpoint = "/",
        Name = "CreateTask",
        Summary = "Creates a task.",
        Description = "Creates an advanced task with ownership, assignee/reviewer, checklist items, dependency ids, and estimation metadata."
    };

    public static EndpointDetails GetTaskBoard => new()
    {
        Endpoint = "/board",
        Name = "GetTaskBoard",
        Summary = "Gets the task board.",
        Description = "Returns tasks grouped by status so clients can render a Kanban board."
    };

    public static EndpointDetails UpdateTaskStatus => new()
    {
        Endpoint = "/{taskId:guid}/status",
        Name = "UpdateTaskStatus",
        Summary = "Updates task status.",
        Description = "Moves a task between statuses and optionally appends a status-change comment."
    };

    public static EndpointDetails AddTimeEntry => new()
    {
        Endpoint = "/{taskId:guid}/time-entries",
        Name = "AddTimeEntry",
        Summary = "Adds a task time entry.",
        Description = "Records tracked time and rolls the duration into the task's actual-hours total."
    };

    public static EndpointDetails GetTaskDependencyGraph => new()
    {
        Endpoint = "/{taskId:guid}/dependencies/graph",
        Name = "GetTaskDependencyGraph",
        Summary = "Gets task dependencies.",
        Description = "Returns the current task dependency graph as a flat list of dependency ids."
    };

    // ── Events ────────────────────────────────────────────────────────────

    public static EndpointDetails CreateEvent => new()
    {
        Endpoint = "/",
        Name = "CreateEvent",
        Summary = "Creates a calendar event.",
        Description = "Creates an event with attendee, reminder, recurrence, and meeting-link support."
    };

    public static EndpointDetails GetCalendarView => new()
    {
        Endpoint = "/calendar",
        Name = "GetCalendarView",
        Summary = "Gets a calendar view.",
        Description = "Returns event data for month, week, day, or agenda-style calendar screens."
    };

    public static EndpointDetails CheckAvailability => new()
    {
        Endpoint = "/availability",
        Name = "CheckAvailability",
        Summary = "Checks attendee availability.",
        Description = "Returns available slots for the supplied users and date range."
    };

    public static EndpointDetails RespondToEvent => new()
    {
        Endpoint = "/{eventId:guid}/respond",
        Name = "RespondToEvent",
        Summary = "Responds to an invitation.",
        Description = "Records Accepted, Declined, or Tentative responses for the current user."
    };

    public static EndpointDetails ExportCalendarIcal => new()
    {
        Endpoint = "/export/ical",
        Name = "ExportCalendarIcal",
        Summary = "Exports calendar data to iCal.",
        Description = "Exports a baseline iCal feed for the next month of events."
    };

    public static EndpointDetails SuggestMeetingTimes => new()
    {
        Endpoint = "/suggest-times",
        Name = "SuggestMeetingTimes",
        Summary = "Suggests meeting times.",
        Description = "Suggests likely meeting times based on the requested date range and desired meeting duration."
    };

    // ── Cross-Entity ──────────────────────────────────────────────────────

    public static EndpointDetails ConvertNoteToTodo => new()
    {
        Endpoint = "/{noteId:guid}/convert-to-todo",
        Name = "ConvertNoteToTodo",
        Summary = "Converts a note to a todo.",
        Description = "Creates a todo from the selected note while keeping the original note intact."
    };

    public static EndpointDetails CreateNotebook => new()
    {
        Endpoint = "/notebooks",
        Name = "CreateNotebook",
        Summary = "Creates a notebook.",
        Description = "Creates a notebook and optionally nests it under a parent notebook."
    };

    public static EndpointDetails GlobalProductivitySearch => new()
    {
        Endpoint = "/search",
        Name = "GlobalProductivitySearch",
        Summary = "Performs global productivity search.",
        Description = "Searches across todos, notes, tasks, and events using a shared search experience."
    };

    public static EndpointDetails GetProductivityDashboardToday => new()
    {
        Endpoint = "/dashboard/today",
        Name = "GetProductivityDashboardToday",
        Summary = "Gets the productivity dashboard.",
        Description = "Returns today's overdue work, due work, events, reminders, recent notes, and summary metrics."
    };

    public static EndpointDetails CreateSmartList => new()
    {
        Endpoint = "/smart-lists",
        Name = "CreateSmartList",
        Summary = "Creates a smart list.",
        Description = "Saves reusable filter criteria for a specific productivity entity type."
    };

    // ── Reports ───────────────────────────────────────────────────────────

    public static EndpointDetails GetProductivityReport => new()
    {
        Endpoint = "/productivity",
        Name = "GetProductivityReport",
        Summary = "Gets a productivity report.",
        Description = "Returns completion, category, and activity metrics for the requested reporting window."
    };

    // ── Templates ─────────────────────────────────────────────────────────

    public static EndpointDetails CreateTemplate => new()
    {
        Endpoint = "/",
        Name = "CreateTemplate",
        Summary = "Creates a reusable productivity template.",
        Description = "Saves a serialized todo, note, task, or event payload for later instantiation."
    };

    public static EndpointDetails InstantiateTemplate => new()
    {
        Endpoint = "/{templateId:guid}/instantiate",
        Name = "InstantiateTemplate",
        Summary = "Instantiates a template.",
        Description = "Returns the saved serialized template payload so clients can prefill creation forms."
    };

}
