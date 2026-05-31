namespace Alphabet.Application.Common.Interfaces.Productivity;

/// <summary>
/// Stores productivity attachments outside the database.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Save async.
    /// </summary>
    Task<string> SaveAsync(string fileName, string contentType, byte[] content, CancellationToken cancellationToken);
}
