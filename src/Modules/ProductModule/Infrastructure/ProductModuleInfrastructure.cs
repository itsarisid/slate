using Microsoft.Extensions.DependencyInjection;

namespace Alphabet.Modules.ProductModule.Infrastructure;

/// <summary>
/// Provides module-specific infrastructure registration hooks for the product module.
/// </summary>
public static class ProductModuleInfrastructure
{
    /// <summary>
    /// Registers product module infrastructure services.
    /// </summary>
    public static IServiceCollection AddProductModuleInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}
