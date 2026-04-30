using System.Text.Json;
using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Infrastructure.Scheduler.JobHandlers;
using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.Scheduler;

/// <summary>
/// Executes scheduled jobs and persists execution logs.
/// </summary>
public sealed class JobExecutor(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    IJobExecutionService executionService,
    HttpCallJobHandler httpCallJobHandler,
    StoredProcedureJobHandler storedProcedureJobHandler,
    CodeExecutionJobHandler codeExecutionJobHandler,
    FileOperationJobHandler fileOperationJobHandler,
    ILogger<JobExecutor> logger)
{
    public Task ExecuteScheduledAsync(Guid jobId)
        => ExecuteInternalAsync(jobId, null, null, null, CancellationToken.None);

    public Task ExecuteQueuedAsync(Guid jobId, Guid executionId, Guid? triggeredBy, Guid? retryParentId)
        => ExecuteInternalAsync(jobId, executionId, triggeredBy, retryParentId, CancellationToken.None);

    private async Task ExecuteInternalAsync(Guid jobId, Guid? executionId, Guid? triggeredBy, Guid? retryParentId, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job is null || job.IsDeleted || !job.IsEnabled || job.IsPaused)
        {
            return;
        }

        var execution = executionId.HasValue
            ? await executionService.GetExecutionAsync(executionId.Value, cancellationToken)
            : null;

        if (execution is null)
        {
            execution = await executionService.CreateAndStartExecutionAsync(jobId, triggeredBy, retryParentId, cancellationToken);
        }
        else
        {
            execution.Start();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var retryPolicy = JsonSerializer.Deserialize<RetryPolicyDto>(job.RetryPolicy)
            ?? new RetryPolicyDto(0, 0, RetryBackoffType.Fixed, null, null);

        var attempts = Math.Max(1, retryPolicy.MaxRetryAttempts + 1);
        string? output = null;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                output = await ExecuteByTypeAsync(job, cancellationToken);
                await executionService.MarkSuccessAsync(execution.Id, output, cancellationToken);
                job.RecordExecution(ExecutionStatus.Success);
                jobRepository.Update(job);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }
            catch (Exception exception)
            {
                lastException = exception;
                logger.LogError(exception, "Scheduler job {JobId} failed on attempt {Attempt}.", jobId, attempt);

                if (attempt < attempts)
                {
                    await Task.Delay(GetDelay(retryPolicy, attempt), cancellationToken);
                }
            }
        }

        await executionService.MarkFailureAsync(execution.Id, output, lastException?.ToString(), attempts - 1, cancellationToken);
        job.RecordExecution(ExecutionStatus.Failed);
        jobRepository.Update(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> ExecuteByTypeAsync(Job job, CancellationToken cancellationToken)
    {
        var jobConfiguration = JsonSerializer.Deserialize<JobConfigurationDto>(job.JobConfiguration)
            ?? throw new InvalidOperationException("Job configuration is invalid.");

        var parameters = jobConfiguration.Parameters is null
            ? JsonDocument.Parse("{}").RootElement.Clone()
            : JsonSerializer.SerializeToElement(jobConfiguration.Parameters);

        parameters = SchedulerVariableResolver.ResolveParameters(parameters, job);

        return job.JobType switch
        {
            JobType.HttpCall => await httpCallJobHandler.ExecuteAsync(job, parameters, cancellationToken),
            JobType.StoredProcedure => await storedProcedureJobHandler.ExecuteAsync(job, parameters, cancellationToken),
            JobType.CodeExecution => await codeExecutionJobHandler.ExecuteAsync(job, parameters, cancellationToken),
            JobType.FileOperation => await fileOperationJobHandler.ExecuteAsync(job, parameters, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported job type '{job.JobType}'.")
        };
    }

    private static TimeSpan GetDelay(RetryPolicyDto retryPolicy, int attempt)
    {
        var baseDelay = Math.Max(1, retryPolicy.RetryDelaySeconds);
        return retryPolicy.RetryBackoffType switch
        {
            RetryBackoffType.Linear => TimeSpan.FromSeconds(baseDelay * attempt),
            RetryBackoffType.Exponential => TimeSpan.FromSeconds(baseDelay * Math.Pow(2, attempt - 1)),
            _ => TimeSpan.FromSeconds(baseDelay)
        };
    }
}
