namespace Alphabet.Application.Features.Privilege.Dtos;

/// <summary>
/// Represents a privilege category node.
/// </summary>
public sealed record PrivilegeCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    int SortOrder,
    IReadOnlyCollection<PrivilegeCategoryDto> Children);
