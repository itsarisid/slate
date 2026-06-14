using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alphabet.Infrastructure.Extensions;

/// <summary>
/// Canonical Core infrastructure registration entrypoint.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddInfrastructure(configuration);
    }
}
