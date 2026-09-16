using Alphabet.Application.Common.Interfaces;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Provides system UTC time.
/// </summary>
public sealed class DateTimeService : IDateTime
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
