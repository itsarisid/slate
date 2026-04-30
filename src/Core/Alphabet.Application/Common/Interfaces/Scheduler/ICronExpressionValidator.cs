namespace Alphabet.Application.Common.Interfaces.Scheduler;

/// <summary>
/// Validates cron expressions used by scheduled jobs.
/// </summary>
public interface ICronExpressionValidator
{
    /// <summary>
    /// Returns true when the expression is considered valid.
    /// </summary>
    bool IsValid(string? expression);
}
