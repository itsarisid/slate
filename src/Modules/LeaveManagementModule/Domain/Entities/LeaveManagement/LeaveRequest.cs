using System.ComponentModel.DataAnnotations.Schema;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Exceptions;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents an employee leave request and its lifecycle.
/// </summary>
public sealed class LeaveRequest : BaseEntity
{
    private LeaveRequest()
    {
    }

    private LeaveRequest(
        Guid leaveTypeId,
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        LeavePartialDays partialDays,
        decimal totalDays,
        string reason,
        IReadOnlyCollection<string> attachmentIds,
        string? contactNumber,
        string? alternateArrangements,
        bool applyToAllDays,
        bool isHalfDay)
    {
        if (endDate < startDate)
        {
            throw new DomainException("End date cannot be before start date.");
        }

        if (totalDays <= 0)
        {
            throw new DomainException("Leave request must include at least one leave day.");
        }

        LeaveTypeId = leaveTypeId;
        UserId = userId;
        StartDate = startDate;
        EndDate = endDate;
        PartialDaysJson = LeaveManagementJson.Serialize(partialDays);
        TotalDays = totalDays;
        Reason = reason.Trim();
        Status = LeaveRequestStatus.Pending;
        CurrentApprovalLevel = 1;
        AppliedAt = DateTimeOffset.UtcNow;
        AttachmentIdsJson = LeaveManagementJson.Serialize(attachmentIds);
        ContactNumber = string.IsNullOrWhiteSpace(contactNumber) ? null : contactNumber.Trim();
        AlternateArrangements = string.IsNullOrWhiteSpace(alternateArrangements) ? null : alternateArrangements.Trim();
        ApplyToAllDays = applyToAllDays;
        IsHalfDay = isHalfDay;
    }

    public Guid LeaveTypeId { get; private set; }

    public Guid UserId { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public string PartialDaysJson { get; private set; } = "{}";

    public decimal TotalDays { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public LeaveRequestStatus Status { get; private set; }

    public int CurrentApprovalLevel { get; private set; }

    public DateTimeOffset AppliedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public string? CancelledReason { get; private set; }

    public string AttachmentIdsJson { get; private set; } = "[]";

    public string? ContactNumber { get; private set; }

    public string? AlternateArrangements { get; private set; }

    public bool ApplyToAllDays { get; private set; }

    public bool IsHalfDay { get; private set; }

    [NotMapped]
    public LeavePartialDays PartialDays => LeaveManagementJson.Deserialize<LeavePartialDays>(PartialDaysJson)
        ?? new LeavePartialDays(LeaveDayPart.Full, LeaveDayPart.Full);

    [NotMapped]
    public IReadOnlyList<string> AttachmentIds => LeaveManagementJson.DeserializeList<string>(AttachmentIdsJson);

    /// <summary>
    /// Creates a leave request.
    /// </summary>
    public static LeaveRequest Create(
        Guid leaveTypeId,
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        LeavePartialDays partialDays,
        decimal totalDays,
        string reason,
        IReadOnlyCollection<string> attachmentIds,
        string? contactNumber,
        string? alternateArrangements,
        bool applyToAllDays,
        bool isHalfDay)
    {
        return new LeaveRequest(leaveTypeId, userId, startDate, endDate, partialDays, totalDays, reason, attachmentIds, contactNumber, alternateArrangements, applyToAllDays, isHalfDay);
    }

    /// <summary>
    /// Updates a pending leave request.
    /// </summary>
    public void Modify(DateOnly startDate, DateOnly endDate, LeavePartialDays partialDays, decimal totalDays, string reason, IReadOnlyCollection<string> attachmentIds, string? contactNumber, string? alternateArrangements)
    {
        if (Status is not LeaveRequestStatus.Pending and not LeaveRequestStatus.ChangesRequested)
        {
            throw new DomainException("Only pending or change-requested leave requests can be modified.");
        }

        StartDate = startDate;
        EndDate = endDate;
        PartialDaysJson = LeaveManagementJson.Serialize(partialDays);
        TotalDays = totalDays;
        Reason = reason.Trim();
        AttachmentIdsJson = LeaveManagementJson.Serialize(attachmentIds);
        ContactNumber = string.IsNullOrWhiteSpace(contactNumber) ? null : contactNumber.Trim();
        AlternateArrangements = string.IsNullOrWhiteSpace(alternateArrangements) ? null : alternateArrangements.Trim();
        Status = LeaveRequestStatus.Pending;
        CurrentApprovalLevel = 1;
        Touch();
    }

    /// <summary>
    /// Moves the request to the next approval level.
    /// </summary>
    public void MoveToApprovalLevel(int level)
    {
        CurrentApprovalLevel = level;
        Status = LeaveRequestStatus.Pending;
        Touch();
    }

    /// <summary>
    /// Marks the request as fully approved.
    /// </summary>
    public void Approve()
    {
        Status = LeaveRequestStatus.Approved;
        Touch();
    }

    /// <summary>
    /// Rejects the request.
    /// </summary>
    public void Reject()
    {
        Status = LeaveRequestStatus.Rejected;
        Touch();
    }

    /// <summary>
    /// Requests changes from the employee.
    /// </summary>
    public void RequestChanges()
    {
        Status = LeaveRequestStatus.ChangesRequested;
        Touch();
    }

    /// <summary>
    /// Cancels the request.
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status is LeaveRequestStatus.Rejected or LeaveRequestStatus.Cancelled)
        {
            throw new DomainException("This leave request cannot be cancelled.");
        }

        Status = LeaveRequestStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
        CancelledReason = reason.Trim();
        Touch();
    }

    /// <summary>
    /// Withdraws the request from active approval.
    /// </summary>
    public void Withdraw()
    {
        if (Status != LeaveRequestStatus.Pending)
        {
            throw new DomainException("Only pending leave requests can be withdrawn.");
        }

        Status = LeaveRequestStatus.Withdrawn;
        Touch();
    }
}
