using Alphabet.Common.Models;

namespace Alphabet.Modules.ProductModule.Api.Resource;

public static class ApiResource
{
    public static EndpointDetails CreateProduct => new()
    {
        Endpoint = "/",
        Name = "CreateProduct",
        Summary = "Creates a new product.",
        Description = "Creates a catalog product, persists it, and returns the created product representation. This endpoint requires the CatalogWrite policy."
    };

    public static EndpointDetails GetProductById => new()
    {
        Endpoint = "/{id:guid}",
        Name = "GetProductById",
        Summary = "Gets a product by identifier.",
        Description = "Returns the product details for the specified product id when the product exists."
    };
}
