using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;

namespace Alphabet.Infrastructure.Scheduler.JobHandlers;

/// <summary>
/// Executes HTTP-call scheduler jobs.
/// </summary>
public sealed class HttpCallJobHandler(IHttpClientFactory httpClientFactory) : IJobHandler
{
    public async Task<string> ExecuteAsync(Job job, JsonElement parameters, CancellationToken cancellationToken)
    {
        var config = JsonSerializer.Deserialize<JobConfigurationDto>(job.JobConfiguration)
            ?? throw new InvalidOperationException("Job configuration is invalid.");

        if (string.IsNullOrWhiteSpace(config.Url) || string.IsNullOrWhiteSpace(config.Method))
        {
            throw new InvalidOperationException("HTTP job configuration requires url and method.");
        }

        using var request = new HttpRequestMessage(new HttpMethod(config.Method), config.Url);
        if (!string.IsNullOrWhiteSpace(config.Body))
        {
            request.Content = new StringContent(config.Body, Encoding.UTF8, "application/json");
        }

        if (config.Headers is not null)
        {
            foreach (var header in config.Headers)
            {
                if (string.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    request.Content ??= new StringContent(string.Empty);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(header.Value);
                }
                else
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        var client = httpClientFactory.CreateClient("Alphabet.Scheduler.Http");
        client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds ?? 30);
        var response = await client.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        return responseBody;
    }
}
