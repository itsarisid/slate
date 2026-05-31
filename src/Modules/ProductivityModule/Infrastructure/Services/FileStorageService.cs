using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Alphabet.Infrastructure.Services;

/// <summary>
/// Stores attachments using the configured file provider.
/// </summary>
public sealed class FileStorageService(IOptions<ProductivityFileStorageSettings> storageOptions) : IFileStorageService
{
    private readonly ProductivityFileStorageSettings _settings = storageOptions.Value;
    /// <summary>
    /// Save async.
    /// </summary>

    public async Task<string> SaveAsync(string fileName, string contentType, byte[] content, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_settings.LocalPath);
        var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(_settings.LocalPath, safeName);
        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);
        return fullPath;
    }
}
