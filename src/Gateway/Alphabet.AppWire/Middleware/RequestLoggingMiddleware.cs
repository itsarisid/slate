namespace Alphabet.AppWire.Middleware;

/// <summary>
/// Logs request and response information for security tracing.
/// </summary>
public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        logger.LogInformation("Incoming {Method} {Path} from {Ip}", context.Request.Method, context.Request.Path, context.Connection.RemoteIpAddress);
        await next(context);
        logger.LogInformation("Completed {Method} {Path} with {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);
    }
}
