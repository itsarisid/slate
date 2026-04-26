using Alphabet.Application.Features.Products;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using MediatR;

namespace Alphabet.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Handles product-by-id queries.
/// </summary>
public sealed class GetProductByIdQueryHandler(IRepository<Product> productRepository)
    : IRequestHandler<GetProductByIdQuery, Result<ProductResponseDto>>
{
    /// <summary>
    /// Handles the get-product-by-id query.
    /// </summary>
    public async Task<Result<ProductResponseDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result<ProductResponseDto>.Failure($"Product '{request.Id}' was not found.");
        }

        return Result<ProductResponseDto>.Success(
            new ProductResponseDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price.Amount,
                product.Price.Currency,
                product.Status.ToString(),
                product.CreatedAt));
    }
}
