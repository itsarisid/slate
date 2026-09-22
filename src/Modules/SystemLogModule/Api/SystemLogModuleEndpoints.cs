using Alphabet.Common.Extensions;
using Alphabet.Modules.SystemLogModule.Api.Models;
using Alphabet.Modules.SystemLogModule.Api.Resource;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Alphabet.Modules.SystemLogModule.Api;

/// <summary>
/// Maps read-only system log endpoints.
/// </summary>
public static class SystemLogModuleEndpoints
{
    private const int DefaultLineCount = 200;
    private const int MaximumLineCount = 1_000;

    /// <summary>
    /// Registers the system log module endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapSystemLogModule(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("api/v{version:apiVersion}/system/logs")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("System Log Module")
            .RequireAuthorization("AdminOnly");

        group.MapGet(ApiResource.ListSystemLogs.Endpoint, (IWebHostEnvironment environment) =>
            Results.Ok(GetLogFiles(GetLogDirectory(environment))))
            .Produces<SystemLogFileDto[]>(StatusCodes.Status200OK)
            .WithDocumentation(ApiResource.ListSystemLogs);

        group.MapGet(ApiResource.GetSystemLog.Endpoint, async Task<IResult> (
            string fileName,
            int? lines,
            IWebHostEnvironment environment,
            CancellationToken cancellationToken) =>
        {
            if (!IsValidLogFileName(fileName))
            {
                return Results.NotFound();
            }

            var logDirectory = GetLogDirectory(environment);
            var filePath = Path.GetFullPath(Path.Combine(logDirectory, fileName));
            if (!filePath.StartsWith(logDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(filePath))
            {
                return Results.NotFound();
            }

            var requestedLineCount = Math.Clamp(lines ?? DefaultLineCount, 1, MaximumLineCount);
            var entries = await ReadTailAsync(filePath, requestedLineCount, cancellationToken);
            var fileInfo = new FileInfo(filePath);

            return Results.Ok(new SystemLogContentDto(fileInfo.Name, fileInfo.Length, fileInfo.LastWriteTimeUtc, entries));
        })
        .Produces<SystemLogContentDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithDocumentation(ApiResource.GetSystemLog);

        return endpoints;
    }

    private static SystemLogFileDto[] GetLogFiles(string logDirectory)
    {
        if (!Directory.Exists(logDirectory))
        {
            return [];
        }

        return new DirectoryInfo(logDirectory)
            .EnumerateFiles("*.log", SearchOption.TopDirectoryOnly)
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Select(file => new SystemLogFileDto(file.Name, file.Length, file.LastWriteTimeUtc))
            .ToArray();
    }

    private static string GetLogDirectory(IWebHostEnvironment environment) =>
        Path.GetFullPath(Path.Combine(environment.ContentRootPath, "logs"));

    private static bool IsValidLogFileName(string fileName) =>
        string.Equals(fileName, Path.GetFileName(fileName), StringComparison.Ordinal) &&
        string.Equals(Path.GetExtension(fileName), ".log", StringComparison.OrdinalIgnoreCase);

    private static async Task<string[]> ReadTailAsync(string filePath, int lineCount, CancellationToken cancellationToken)
    {
        var entries = new Queue<string>(lineCount);
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (entries.Count == lineCount)
            {
                entries.Dequeue();
            }

            entries.Enqueue(line);
        }

        return entries.ToArray();
    }
}
