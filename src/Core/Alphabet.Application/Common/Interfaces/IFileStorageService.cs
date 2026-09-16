namespace Alphabet.Application.Common.Interfaces;

/// <summary>
/// Stores and retrieves application files.
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(string path, CancellationToken cancellationToken);

    Task DeleteAsync(string path, CancellationToken cancellationToken);
}
