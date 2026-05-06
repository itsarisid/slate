namespace Alphabet.Application.Features.Privilege.Dtos;

/// <summary>
/// Represents analytics data for privilege usage and assignments.
/// </summary>
public sealed record PrivilegeAnalyticsDto(
    IReadOnlyCollection<PrivilegeUsageMetricDto> MostUsedPrivileges,
    IReadOnlyCollection<string> UnusedPrivileges,
    IReadOnlyCollection<PrivilegeAssignmentTrendDto> PrivilegeAssignmentTrend);

/// <summary>
/// Represents privilege usage metrics.
/// </summary>
public sealed record PrivilegeUsageMetricDto(string Privilege, int UsageCount, int UniqueUsers);

/// <summary>
/// Represents assignment counts by day.
/// </summary>
public sealed record PrivilegeAssignmentTrendDto(DateOnly Date, int Assignments);
