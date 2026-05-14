namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents the smart productivity dashboard.
/// </summary>
public sealed record DashboardDto(
    IReadOnlyList<TodoDto> OverdueTodos,
    IReadOnlyList<TodoDto> TodayDueTodos,
    IReadOnlyList<CalendarEventDto> TodayEvents,
    IReadOnlyList<ReminderDto> PendingReminders,
    IReadOnlyList<NoteDto> RecentNotes,
    ProductivityStatsDto Stats);
