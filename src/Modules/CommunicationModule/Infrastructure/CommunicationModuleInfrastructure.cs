using Microsoft.Extensions.DependencyInjection;

namespace Alphabet.Modules.CommunicationModule.Infrastructure;

/// <summary>
/// Provides module-specific infrastructure registration hooks for the communication module.
/// </summary>
public static class CommunicationModuleInfrastructure
{
    /// <summary>
    /// Registers communication module infrastructure services.
    /// </summary>
    public static IServiceCollection AddCommunicationModuleInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}
