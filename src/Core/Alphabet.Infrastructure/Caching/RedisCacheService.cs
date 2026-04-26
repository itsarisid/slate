using System.Text.Json;
using Alphabet.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Alphabet.Infrastructure.Caching;

/// <summary>
/// Provides Redis-backed distributed caching.
/// </summary>
public sealed class RedisCacheService(IDistributedCache distributedCache) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var serialized = await distributedCache.GetStringAsync(key, cancellationToken);
        return serialized is null ? default : JsonSerializer.Deserialize<T>(serialized);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan duration, CancellationToken cancellationToken)
    {
        return distributedCache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            },
            cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        return distributedCache.RemoveAsync(key, cancellationToken);
    }
}
