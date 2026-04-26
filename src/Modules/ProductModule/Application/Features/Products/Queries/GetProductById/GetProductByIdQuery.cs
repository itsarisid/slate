using Alphabet.Application.Features.Products;
using Alphabet.Application.Results;
using MediatR;

namespace Alphabet.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Gets a product by identifier.
/// </summary>
public sealed record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductResponseDto>>;
