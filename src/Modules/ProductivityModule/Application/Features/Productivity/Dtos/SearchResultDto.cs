namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents a global search hit across productivity entities.
/// </summary>
public sealed record SearchResultDto(
    Guid Id,
    string EntityType,
    string Title,
    string Summary,
    decimal Score,
    DateTimeOffset UpdatedAt);
