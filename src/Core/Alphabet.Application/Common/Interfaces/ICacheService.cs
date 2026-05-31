namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Provides distributed or in-memory caching behavior.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get async.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);
    /// <summary>
    /// Set async.
    /// </summary>

    Task SetAsync<T>(string key, T value, TimeSpan duration, CancellationToken cancellationToken);
    /// <summary>
    /// Remove async.
    /// </summary>

    Task RemoveAsync(string key, CancellationToken cancellationToken);
}
