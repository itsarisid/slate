namespace Alphabet.Application.Features.Privilege.Dtos;

/// <summary>
/// Represents a privilege definition for API consumers.
/// </summary>
public sealed record PrivilegeDto(
    Guid Id,
    string Name,
    string DisplayName,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    string? ResourceType,
    IReadOnlyCollection<string> Actions,
    bool IsGlobal,
    bool IsDeprecated,
    IReadOnlyCollection<string> DependsOn,
    IReadOnlyDictionary<string, string?> Attributes,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset UpdatedAt);
