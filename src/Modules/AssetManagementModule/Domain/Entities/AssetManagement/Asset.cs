using Alphabet.Domain.Enums;
using Alphabet.Domain.Exceptions;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents an asset in the inventory.
/// </summary>
public sealed class Asset : BaseEntity
{
    private Asset()
    {
    }

    private Asset(
        string assetTag,
        string name,
        string description,
        Guid categoryId,
        string? subcategory,
        string? manufacturer,
        string? model,
        string? serialNumber,
        DateOnly? purchaseDate,
        DateOnly? warrantyExpiry,
        decimal cost,
        string currency,
        AssetStatus status,
        AssetCondition condition,
        Guid locationId,
        Guid? supplierId,
        IReadOnlyDictionary<string, string>? customFields,
        IReadOnlyCollection<string>? images,
        IReadOnlyCollection<string>? documents,
        string? qrCodePayload,
        string? barcodePayload)
    {
        if (string.IsNullOrWhiteSpace(assetTag))
        {
            throw new DomainException("Asset tag is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Asset name is required.");
        }

        if (cost < 0)
        {
            throw new DomainException("Asset cost cannot be negative.");
        }

        AssetTag = assetTag.Trim().ToUpperInvariant();
        Name = name.Trim();
        Description = description.Trim();
        CategoryId = categoryId;
        Subcategory = Normalize(subcategory);
        Manufacturer = Normalize(manufacturer);
        Model = Normalize(model);
        SerialNumber = Normalize(serialNumber);
        PurchaseDate = purchaseDate;
        WarrantyExpiry = warrantyExpiry;
        Cost = decimal.Round(cost, 2, MidpointRounding.ToEven);
        Currency = currency.Trim().ToUpperInvariant();
        Status = status;
        Condition = condition;
        LocationId = locationId;
        SupplierId = supplierId;
        CustomFieldsJson = AssetManagementJson.Serialize(customFields ?? new Dictionary<string, string>());
        ImagesJson = AssetManagementJson.Serialize(images ?? Array.Empty<string>());
        DocumentsJson = AssetManagementJson.Serialize(documents ?? Array.Empty<string>());
        QrCodePayload = Normalize(qrCodePayload);
        BarcodePayload = Normalize(barcodePayload);
    }

    public string AssetTag { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Guid CategoryId { get; private set; }

    public string? Subcategory { get; private set; }

    public string? Manufacturer { get; private set; }

    public string? Model { get; private set; }

    public string? SerialNumber { get; private set; }

    public DateOnly? PurchaseDate { get; private set; }

    public DateOnly? WarrantyExpiry { get; private set; }

    public decimal Cost { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public AssetStatus Status { get; private set; }

    public AssetCondition Condition { get; private set; }

    public Guid LocationId { get; private set; }

    public Guid? SupplierId { get; private set; }

    public string CustomFieldsJson { get; private set; } = "{}";

    public string ImagesJson { get; private set; } = "[]";

    public string DocumentsJson { get; private set; } = "[]";

    public string? QrCodePayload { get; private set; }

    public string? BarcodePayload { get; private set; }

    public Guid? AssignedToUserId { get; private set; }

    public DateTimeOffset? AssignedAt { get; private set; }

    public DateOnly? ExpectedReturnDate { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? LastInventoryCheckAt { get; private set; }

    public IReadOnlyDictionary<string, string> CustomFields => AssetManagementJson.DeserializeDictionary(CustomFieldsJson);

    public IReadOnlyList<string> Images => AssetManagementJson.DeserializeList<string>(ImagesJson);

    public IReadOnlyList<string> Documents => AssetManagementJson.DeserializeList<string>(DocumentsJson);

    /// <summary>
    /// Creates a new asset.
    /// </summary>
    public static Asset Create(
        string assetTag,
        string name,
        string description,
        Guid categoryId,
        string? subcategory,
        string? manufacturer,
        string? model,
        string? serialNumber,
        DateOnly? purchaseDate,
        DateOnly? warrantyExpiry,
        decimal cost,
        string currency,
        AssetStatus status,
        AssetCondition condition,
        Guid locationId,
        Guid? supplierId,
        IReadOnlyDictionary<string, string>? customFields,
        IReadOnlyCollection<string>? images,
        IReadOnlyCollection<string>? documents,
        string? qrCodePayload,
        string? barcodePayload)
    {
        return new Asset(
            assetTag,
            name,
            description,
            categoryId,
            subcategory,
            manufacturer,
            model,
            serialNumber,
            purchaseDate,
            warrantyExpiry,
            cost,
            currency,
            status,
            condition,
            locationId,
            supplierId,
            customFields,
            images,
            documents,
            qrCodePayload,
            barcodePayload);
    }

    /// <summary>
    /// Updates the asset's descriptive information.
    /// </summary>
    public void UpdateDetails(
        string name,
        string description,
        Guid categoryId,
        string? subcategory,
        string? manufacturer,
        string? model,
        string? serialNumber,
        DateOnly? purchaseDate,
        DateOnly? warrantyExpiry,
        decimal cost,
        string currency,
        AssetCondition condition,
        Guid locationId,
        Guid? supplierId,
        IReadOnlyDictionary<string, string>? customFields,
        IReadOnlyCollection<string>? images,
        IReadOnlyCollection<string>? documents)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Asset name is required.");
        }

        if (cost < 0)
        {
            throw new DomainException("Asset cost cannot be negative.");
        }

        Name = name.Trim();
        Description = description.Trim();
        CategoryId = categoryId;
        Subcategory = Normalize(subcategory);
        Manufacturer = Normalize(manufacturer);
        Model = Normalize(model);
        SerialNumber = Normalize(serialNumber);
        PurchaseDate = purchaseDate;
        WarrantyExpiry = warrantyExpiry;
        Cost = decimal.Round(cost, 2, MidpointRounding.ToEven);
        Currency = currency.Trim().ToUpperInvariant();
        Condition = condition;
        LocationId = locationId;
        SupplierId = supplierId;
        CustomFieldsJson = AssetManagementJson.Serialize(customFields ?? new Dictionary<string, string>());
        ImagesJson = AssetManagementJson.Serialize(images ?? Array.Empty<string>());
        DocumentsJson = AssetManagementJson.Serialize(documents ?? Array.Empty<string>());
        Touch();
    }

    /// <summary>
    /// Assigns the asset to a user.
    /// </summary>
    public void AssignTo(Guid userId, DateOnly? expectedReturnDate)
    {
        if (AssignedToUserId.HasValue && Status == AssetStatus.Assigned)
        {
            throw new DomainException("Asset is already assigned.");
        }

        AssignedToUserId = userId;
        AssignedAt = DateTimeOffset.UtcNow;
        ExpectedReturnDate = expectedReturnDate;
        Status = AssetStatus.Assigned;
        Touch();
    }

    /// <summary>
    /// Returns the asset from the active assignment.
    /// </summary>
    public void ReturnFromAssignment(AssetCondition returnedCondition)
    {
        AssignedToUserId = null;
        AssignedAt = null;
        ExpectedReturnDate = null;
        Condition = returnedCondition;
        Status = AssetStatus.Available;
        Touch();
    }

    /// <summary>
    /// Transfers the asset to a different user.
    /// </summary>
    public void TransferTo(Guid userId, DateOnly? expectedReturnDate)
    {
        if (Status != AssetStatus.Assigned)
        {
            throw new DomainException("Only assigned assets can be transferred.");
        }

        AssignedToUserId = userId;
        AssignedAt = DateTimeOffset.UtcNow;
        ExpectedReturnDate = expectedReturnDate;
        Touch();
    }

    /// <summary>
    /// Moves the asset to a new location.
    /// </summary>
    public void Move(Guid newLocationId)
    {
        LocationId = newLocationId;
        Touch();
    }

    /// <summary>
    /// Marks the asset as under repair.
    /// </summary>
    public void MarkUnderRepair()
    {
        Status = AssetStatus.UnderRepair;
        Touch();
    }

    /// <summary>
    /// Marks the asset as available.
    /// </summary>
    public void MarkAvailable()
    {
        Status = AssignedToUserId.HasValue ? AssetStatus.Assigned : AssetStatus.Available;
        Touch();
    }

    /// <summary>
    /// Retires or disposes the asset.
    /// </summary>
    public void Retire(AssetStatus status)
    {
        if (status is not AssetStatus.Retired and not AssetStatus.Disposed)
        {
            throw new DomainException("Asset can only be retired or disposed through this operation.");
        }

        Status = status;
        IsDeleted = true;
        Touch();
    }

    /// <summary>
    /// Updates generated barcode payloads.
    /// </summary>
    public void SetCodes(string? qrCodePayload, string? barcodePayload)
    {
        QrCodePayload = Normalize(qrCodePayload);
        BarcodePayload = Normalize(barcodePayload);
        Touch();
    }

    /// <summary>
    /// Records a stock take timestamp.
    /// </summary>
    public void MarkInventoryChecked()
    {
        LastInventoryCheckAt = DateTimeOffset.UtcNow;
        Touch();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
