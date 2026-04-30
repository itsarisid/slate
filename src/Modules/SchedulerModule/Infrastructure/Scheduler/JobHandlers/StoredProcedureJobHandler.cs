using System.Data;
using System.Text.Json;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Infrastructure.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Scheduler.JobHandlers;

/// <summary>
/// Executes stored-procedure scheduler jobs.
/// </summary>
public sealed class StoredProcedureJobHandler(IOptions<DatabaseSettings> databaseSettings) : IJobHandler
{
    public async Task<string> ExecuteAsync(Job job, JsonElement parameters, CancellationToken cancellationToken)
    {
        var config = JsonSerializer.Deserialize<JobConfigurationDto>(job.JobConfiguration)
            ?? throw new InvalidOperationException("Job configuration is invalid.");

        if (string.IsNullOrWhiteSpace(config.StoredProcedureName))
        {
            throw new InvalidOperationException("Stored procedure name is required.");
        }

        await using var connection = new SqlConnection(databaseSettings.Value.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = config.StoredProcedureName;
        command.CommandTimeout = config.CommandTimeoutSeconds ?? 300;

        if (config.Parameters is not null)
        {
            foreach (var parameter in config.Parameters)
            {
                command.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }
        }

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return $"Stored procedure '{config.StoredProcedureName}' executed successfully. Rows affected: {affected}.";
    }
}
