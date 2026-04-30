using System.Text.Json;
using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Interfaces;

/// <summary>
/// Defines the execution contract for code-based scheduler jobs.
/// </summary>
public interface IJobHandler
{
    /// <summary>
    /// Executes the custom job handler.
    /// </summary>
    Task<string> ExecuteAsync(Job job, JsonElement parameters, CancellationToken cancellationToken);
}
