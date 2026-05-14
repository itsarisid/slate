namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents productivity summary metrics.
/// </summary>
public sealed record ProductivityStatsDto(
    int CompletedToday,
    int PendingTotal,
    int TasksInProgress);
