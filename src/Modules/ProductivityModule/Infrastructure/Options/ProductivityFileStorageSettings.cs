namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Configures productivity attachment storage.
/// </summary>
public sealed class ProductivityFileStorageSettings
{
    public const string SectionName = "FileStorage";

    public string Provider { get; init; } = "Local";

    public string LocalPath { get; init; } = Path.Combine(AppContext.BaseDirectory, "attachments");

    public string? AzureConnectionString { get; init; }

    public string? AWSBucketName { get; init; }
}
