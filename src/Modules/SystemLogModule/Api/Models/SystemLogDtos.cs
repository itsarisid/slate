namespace Alphabet.Modules.SystemLogModule.Api.Models;

/// <summary>
/// Describes an available system log file.
/// </summary>
public sealed record SystemLogFileDto(string FileName, long SizeBytes, DateTime LastModifiedUtc);

/// <summary>
/// Contains the most recent entries from a system log file.
/// </summary>
public sealed record SystemLogContentDto(string FileName, long SizeBytes, DateTime LastModifiedUtc, string[] Lines);
