using Alphabet.Application.Common.Exceptions;
using Alphabet.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Alphabet.AppWire.Middleware;

/// <summary>
/// Converts unhandled exceptions into RFC 7807 problem details responses.
/// </summary>
public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    /// <summary>
    /// Executes the middleware pipeline.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception occurred while processing {Path}", context.Request.Path);
            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, type, errors) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
                validationException.Errors),
            DomainException => (
                StatusCodes.Status422UnprocessableEntity,
                "Domain rule violation",
                "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.21",
                (IDictionary<string, string[]>)new Dictionary<string, string[]>()),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Server error",
                "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
                (IDictionary<string, string[]>)new Dictionary<string, string[]>())
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        if (errors.Count != 0)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
