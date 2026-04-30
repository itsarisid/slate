using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Scheduler.JobHandlers;

/// <summary>
/// Executes file-operation scheduler jobs.
/// </summary>
public sealed class FileOperationJobHandler(IOptions<SchedulerSettings> schedulerOptions) : IJobHandler
{
    public Task<string> ExecuteAsync(Job job, JsonElement parameters, CancellationToken cancellationToken)
    {
        var config = JsonSerializer.Deserialize<JobConfigurationDto>(job.JobConfiguration)
            ?? throw new InvalidOperationException("Job configuration is invalid.");

        if (string.IsNullOrWhiteSpace(config.SourcePath))
        {
            throw new InvalidOperationException("Source path is required.");
        }

        var allowedRoots = schedulerOptions.Value.Jobs.AllowedFileRoots;
        EnsurePathAllowed(config.SourcePath, allowedRoots);
        if (!string.IsNullOrWhiteSpace(config.DestinationPath))
        {
            EnsurePathAllowed(config.DestinationPath, allowedRoots);
        }

        var builder = new StringBuilder();
        var files = ResolveFiles(config.SourcePath).ToArray();
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch ((config.Operation ?? "DeleteOldFiles").Trim())
            {
                case "Archive":
                case "Move":
                    var destinationFolder = config.DestinationPath ?? throw new InvalidOperationException("Destination path is required.");
                    Directory.CreateDirectory(destinationFolder);
                    var destinationFile = Path.Combine(destinationFolder, Path.GetFileName(file));
                    File.Move(file, destinationFile, overwrite: true);
                    builder.AppendLine($"Moved {file} to {destinationFile}");
                    break;
                case "Compress":
                    var archivePath = config.ArchivePath ?? Path.Combine(Path.GetDirectoryName(file) ?? string.Empty, $"{Path.GetFileNameWithoutExtension(file)}.zip");
                    using (var zip = ZipFile.Open(archivePath, ZipArchiveMode.Update))
                    {
                        zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                    }

                    builder.AppendLine($"Compressed {file} into {archivePath}");
                    break;
                default:
                    File.Delete(file);
                    builder.AppendLine($"Deleted {file}");
                    break;
            }
        }

        return Task.FromResult(builder.ToString().Trim());
    }

    private static IEnumerable<string> ResolveFiles(string searchPattern)
    {
        var directory = Path.GetDirectoryName(searchPattern);
        var pattern = Path.GetFileName(searchPattern);

        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return [];
        }

        return Directory.GetFiles(directory, string.IsNullOrWhiteSpace(pattern) ? "*" : pattern, SearchOption.TopDirectoryOnly);
    }

    private static void EnsurePathAllowed(string path, string[] allowedRoots)
    {
        var fullPath = Path.GetFullPath(path);
        if (!allowedRoots.Any(root => fullPath.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Path '{path}' is outside the allowed scheduler file roots.");
        }
    }
}
