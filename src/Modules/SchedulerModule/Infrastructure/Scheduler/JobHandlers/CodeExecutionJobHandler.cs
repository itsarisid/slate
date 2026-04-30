using System.Text.Json;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Alphabet.Infrastructure.Scheduler.JobHandlers;

/// <summary>
/// Executes scheduler jobs that map to registered code handlers.
/// </summary>
public sealed class CodeExecutionJobHandler(IServiceProvider serviceProvider) : IJobHandler
{
    public async Task<string> ExecuteAsync(Job job, JsonElement parameters, CancellationToken cancellationToken)
    {
        var config = JsonSerializer.Deserialize<JobConfigurationDto>(job.JobConfiguration)
            ?? throw new InvalidOperationException("Job configuration is invalid.");

        if (string.IsNullOrWhiteSpace(config.HandlerType))
        {
            throw new InvalidOperationException("Handler type is required.");
        }

        var handlerType = Type.GetType(config.HandlerType, throwOnError: false);
        if (handlerType is null)
        {
            throw new InvalidOperationException($"Could not resolve handler type '{config.HandlerType}'.");
        }

        var handler = serviceProvider.GetService(handlerType) as IJobHandler;
        if (handler is null)
        {
            throw new InvalidOperationException($"Handler type '{config.HandlerType}' is not registered as IJobHandler.");
        }

        var resolvedParameters = config.Parameters is null
            ? JsonDocument.Parse("{}").RootElement.Clone()
            : JsonSerializer.SerializeToElement(config.Parameters);

        return await handler.ExecuteAsync(job, SchedulerVariableResolver.ResolveParameters(resolvedParameters, job), cancellationToken);
    }
}
