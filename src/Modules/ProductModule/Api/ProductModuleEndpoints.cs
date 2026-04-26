using Alphabet.Application.Features.Products;
using Alphabet.Application.Features.Products.Commands.CreateProduct;
using Alphabet.Application.Features.Products.Queries.GetProductById;
using Alphabet.Contracts.Products;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Alphabet.Modules.ProductModule.Api;

/// <summary>
/// Maps product endpoints for the product module.
/// </summary>
public static class ProductModuleEndpoints
{
    public static IEndpointRouteBuilder MapProductModule(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("api/v{version:apiVersion}/products")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Product Module");

        group.MapPost(
                "/",
                async Task<Results<Created<ProductResponseDto>, BadRequest<ProblemDetails>>> (
                    CreateProductRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new CreateProductCommand(request.Name, request.Description, request.Price, request.Currency),
                        cancellationToken);

                    if (result.IsFailure || result.Value is null)
                    {
                        return TypedResults.BadRequest(new ProblemDetails
                        {
                            Title = "Product creation failed",
                            Detail = result.Error
                        });
                    }

                    return TypedResults.Created($"/api/v1/products/{result.Value.Id}", result.Value);
                })
            .RequireAuthorization("CatalogWrite")
            .Produces<ProductResponseDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .WithName("CreateProduct")
            .WithSummary("Creates a new product.");

        group.MapGet(
                "/{id:guid}",
                async Task<Results<Ok<ProductResponseDto>, NotFound<ProblemDetails>>> (
                    Guid id,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetProductByIdQuery(id), cancellationToken);

                    if (result.IsFailure || result.Value is null)
                    {
                        return TypedResults.NotFound(new ProblemDetails
                        {
                            Title = "Product not found",
                            Detail = result.Error
                        });
                    }

                    return TypedResults.Ok(result.Value);
                })
            .AllowAnonymous()
            .Produces<ProductResponseDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .WithName("GetProductById")
            .WithSummary("Gets a product by identifier.");

        return endpoints;
    }
}
