namespace Alphabet.Application.Features.Products;

/// <summary>
/// Represents product data returned to API callers.
/// </summary>
public sealed record ProductResponseDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    string Status,
    DateTimeOffset CreatedAt);
