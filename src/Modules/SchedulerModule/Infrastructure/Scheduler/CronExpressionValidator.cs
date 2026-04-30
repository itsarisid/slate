using System.Text.RegularExpressions;
using Alphabet.Application.Common.Interfaces.Scheduler;

namespace Alphabet.Infrastructure.Scheduler;

/// <summary>
/// Lightweight cron validator for 5-part UNIX cron expressions.
/// </summary>
public sealed class CronExpressionValidator : ICronExpressionValidator
{
    private static readonly Regex PartRegex = new("^[0-9*/,\\-]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public bool IsValid(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        var parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 5 && parts.All(part => PartRegex.IsMatch(part));
    }
}
