namespace Alphabet.Application.Features.Scheduler.Dtos;

/// <summary>
/// Stores the union of supported scheduler job configuration values.
/// </summary>
public sealed record JobConfigurationDto(
    string? Url,
    string? Method,
    IReadOnlyDictionary<string, string>? Headers,
    string? Body,
    int? TimeoutSeconds,
    IReadOnlyList<int>? RetryOnStatusCodes,
    IReadOnlyList<int>? SuccessStatusCodes,
    string? StoredProcedureName,
    IReadOnlyDictionary<string, string>? Parameters,
    int? CommandTimeoutSeconds,
    string? DatabaseConnectionStringName,
    string? HandlerType,
    string? MethodName,
    string? Operation,
    string? SourcePath,
    string? DestinationPath,
    int? DeleteAfterDays,
    int? OlderThanHours,
    bool? DeleteEmptyDirectories,
    string? ArchivePath,
    string? Compression);
