using Alphabet.Domain.Enums;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents assignment history for an asset.
/// </summary>
public sealed class AssetAssignment : BaseEntity
{
    private AssetAssignment()
    {
    }

    private AssetAssignment(
        Guid assetId,
        Guid assignedToUserId,
        Guid assignedByUserId,
        DateOnly? expectedReturnDate,
        AssetAssignmentType assignmentType,
        AssetCondition conditionAtAssignment,
        string? purpose,
        string? notes)
    {
        AssetId = assetId;
        AssignedToUserId = assignedToUserId;
        AssignedByUserId = assignedByUserId;
        AssignedAt = DateTimeOffset.UtcNow;
        ExpectedReturnDate = expectedReturnDate;
        AssignmentType = assignmentType;
        ConditionAtAssignment = conditionAtAssignment;
        Purpose = string.IsNullOrWhiteSpace(purpose) ? null : purpose.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        IsActive = true;
    }

    public Guid AssetId { get; private set; }

    public Guid AssignedToUserId { get; private set; }

    public Guid AssignedByUserId { get; private set; }

    public DateTimeOffset AssignedAt { get; private set; }

    public DateOnly? ExpectedReturnDate { get; private set; }

    public DateOnly? ActualReturnDate { get; private set; }

    public AssetAssignmentType AssignmentType { get; private set; }

    public AssetCondition ConditionAtAssignment { get; private set; }

    public AssetCondition? ConditionOnReturn { get; private set; }

    public string? Purpose { get; private set; }

    public string? Notes { get; private set; }

    public string? DamageNotes { get; private set; }

    public string MissingItemsJson { get; private set; } = "[]";

    public Guid? ReturnedByUserId { get; private set; }

    public Guid? ReceivedByUserId { get; private set; }

    public bool IsActive { get; private set; }

    /// <summary>
    /// Creates a new assignment history record.
    /// </summary>
    public static AssetAssignment Create(
        Guid assetId,
        Guid assignedToUserId,
        Guid assignedByUserId,
        DateOnly? expectedReturnDate,
        AssetAssignmentType assignmentType,
        AssetCondition conditionAtAssignment,
        string? purpose,
        string? notes)
    {
        return new AssetAssignment(assetId, assignedToUserId, assignedByUserId, expectedReturnDate, assignmentType, conditionAtAssignment, purpose, notes);
    }

    /// <summary>
    /// Marks the assignment as returned.
    /// </summary>
    public void MarkReturned(
        Guid returnedByUserId,
        Guid receivedByUserId,
        AssetCondition conditionOnReturn,
        string? damageNotes,
        IReadOnlyCollection<string>? missingItems,
        DateOnly actualReturnDate)
    {
        ReturnedByUserId = returnedByUserId;
        ReceivedByUserId = receivedByUserId;
        ConditionOnReturn = conditionOnReturn;
        DamageNotes = string.IsNullOrWhiteSpace(damageNotes) ? null : damageNotes.Trim();
        MissingItemsJson = AssetManagementJson.Serialize(missingItems ?? Array.Empty<string>());
        ActualReturnDate = actualReturnDate;
        IsActive = false;
        Touch();
    }
}
