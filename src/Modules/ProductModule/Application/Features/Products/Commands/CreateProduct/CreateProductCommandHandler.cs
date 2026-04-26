using Alphabet.Application.Features.Products;
using Alphabet.Application.Results;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.ValueObjects;
using MediatR;

namespace Alphabet.Application.Features.Products.Commands.CreateProduct;

/// <summary>
/// Handles product creation requests.
/// </summary>
public sealed class CreateProductCommandHandler(
    IRepository<Product> productRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCommand, Result<ProductResponseDto>>
{
    /// <summary>
    /// Handles the create product command.
    /// </summary>
    public async Task<Result<ProductResponseDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Description, new Money(request.Price, request.Currency));
        product.Activate();

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
