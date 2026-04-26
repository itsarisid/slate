using MediatR;
using Microsoft.Extensions.Logging;

namespace Alphabet.Application.Common.Behaviors;

/// <summary>
/// Logs request execution boundaries.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Logs the request before and after execution.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling request {RequestName} with payload {@Request}", requestName, request);

        var response = await next();

        logger.LogInformation("Completed request {RequestName}", requestName);
        return response;
    }
}
