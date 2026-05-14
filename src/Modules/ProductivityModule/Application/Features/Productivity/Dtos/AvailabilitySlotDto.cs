namespace Alphabet.Application.Features.Productivity.Dtos;

/// <summary>
/// Represents a suggested availability slot.
/// </summary>
public sealed record AvailabilitySlotDto(
    DateTimeOffset Start,
    DateTimeOffset End,
    IReadOnlyList<Guid> UserIds);
