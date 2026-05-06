using Alphabet.Application.Common.Interfaces;

namespace Alphabet.Infrastructure.Repositories.Privilege;

/// <summary>
/// Provides privilege-focused cache operations.
/// </summary>
public sealed class PrivilegeCacheRepository(ICacheService cacheService)
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        => cacheService.GetAsync<T>(key, cancellationToken);

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
        => cacheService.SetAsync(key, value, ttl, cancellationToken);

    public Task RemoveAsync(string key, CancellationToken cancellationToken)
        => cacheService.RemoveAsync(key, cancellationToken);
}
