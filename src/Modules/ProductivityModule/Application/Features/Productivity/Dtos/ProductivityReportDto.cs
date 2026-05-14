namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents a productivity report summary.
/// </summary>
public sealed record ProductivityReportDto(
    int TasksCompleted,
    int TodosCompleted,
    string AverageResponseTime,
    string MostProductiveDay,
    IReadOnlyDictionary<string, int> CategoryBreakdown);
