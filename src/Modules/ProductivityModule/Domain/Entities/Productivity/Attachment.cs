namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a file attachment associated with a productivity entity.
/// </summary>
public sealed class Attachment : BaseEntity
{
    public string EntityType { get; private set; } = string.Empty;

    public Guid EntityId { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public string StoragePath { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }

    public Guid UploadedByUserId { get; private set; }

    private Attachment()
    {
    }

    private Attachment(string entityType, Guid entityId, string fileName, string contentType, string storagePath, long sizeBytes, Guid uploadedByUserId)
    {
        EntityType = entityType.Trim();
        EntityId = entityId;
        FileName = fileName.Trim();
        ContentType = contentType.Trim();
        StoragePath = storagePath.Trim();
        SizeBytes = sizeBytes;
        UploadedByUserId = uploadedByUserId;
    }

    public static Attachment Create(string entityType, Guid entityId, string fileName, string contentType, string storagePath, long sizeBytes, Guid uploadedByUserId)
        => new(entityType, entityId, fileName, contentType, storagePath, sizeBytes, uploadedByUserId);
}
