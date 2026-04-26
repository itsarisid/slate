using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Alphabet.Infrastructure.Health;

/// <summary>
/// Performs a lightweight distributed cache health check.
/// </summary>
public sealed class RedisHealthCheck(IDistributedCache distributedCache) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        const string probeKey = "health:probe";

        try
        {
            await distributedCache.SetStringAsync(probeKey, "ok", cancellationToken);
            var value = await distributedCache.GetStringAsync(probeKey, cancellationToken);

            return value == "ok"
                ? HealthCheckResult.Healthy("Redis cache is reachable.")
                : HealthCheckResult.Degraded("Redis cache responded unexpectedly.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Redis cache is unavailable.", exception);
        }
    }
}
