using System.Text.Json;
using Alphabet.Domain.Entities;

namespace Alphabet.Infrastructure.Scheduler;

/// <summary>
/// Resolves dynamic scheduler variables inside JSON payloads.
/// </summary>
internal static class SchedulerVariableResolver
{
    public static JsonElement ResolveParameters(JsonElement parameters, Job job)
    {
        var json = parameters.GetRawText()
            .Replace("{{Today}}", DateTime.UtcNow.ToString("yyyy-MM-dd"), StringComparison.Ordinal)
            .Replace("{{Now}}", DateTime.UtcNow.ToString("O"), StringComparison.Ordinal)
            .Replace("{{Job.Id}}", job.Id.ToString(), StringComparison.Ordinal)
            .Replace("{{Job.Name}}", job.Name, StringComparison.Ordinal)
            .Replace("{{User.Email}}", job.CreatedBy, StringComparison.Ordinal)
            .Replace("{{Environment.MachineName}}", Environment.MachineName, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
