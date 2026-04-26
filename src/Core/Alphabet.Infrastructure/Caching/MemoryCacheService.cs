using System.Text.Json;
using Alphabet.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Alphabet.Infrastructure.Caching;

/// <summary>
/// Provides in-memory caching.
/// </summary>
public sealed class MemoryCacheService(IMemoryCache memoryCache) : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!memoryCache.TryGetValue<string>(key, out var serialized) || serialized is null)
        {
            return Task.FromResult<T?>(default);
        }

        return Task.FromResult(JsonSerializer.Deserialize<T>(serialized));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan duration, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        memoryCache.Set(key, JsonSerializer.Serialize(value), duration);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        memoryCache.Remove(key);
        return Task.CompletedTask;
    }
}
