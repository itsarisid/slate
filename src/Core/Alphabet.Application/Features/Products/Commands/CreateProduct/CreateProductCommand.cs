using Alphabet.Application.Features.Products;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Products.Commands.CreateProduct;

/// <summary>
/// Creates a new product.
/// </summary>
public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Currency) : IRequest<Result<ProductResponseDto>>;
