namespace Alphabet.Contracts.Products;

/// <summary>
/// Represents the API request payload for creating a product.
/// </summary>
public sealed record CreateProductRequest(string Name, string Description, decimal Price, string Currency);
