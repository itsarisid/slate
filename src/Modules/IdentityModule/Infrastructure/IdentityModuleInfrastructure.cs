using Microsoft.Extensions.DependencyInjection;

namespace Alphabet.Modules.IdentityModule.Infrastructure;

/// <summary>
/// Provides module-specific infrastructure registration hooks for the identity module.
/// </summary>
public static class IdentityModuleInfrastructure
{
    /// <summary>
    /// Registers identity module infrastructure services.
    /// </summary>
    public static IServiceCollection AddIdentityModuleInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}
