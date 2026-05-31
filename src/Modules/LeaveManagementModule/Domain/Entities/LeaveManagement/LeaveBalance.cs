using Alphabet.Domain.Exceptions;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a user's annual leave balance for a leave type.
/// </summary>
public sealed class LeaveBalance : BaseEntity
{
    private LeaveBalance()
    {
    }

    private LeaveBalance(Guid userId, Guid leaveTypeId, int year, decimal allocated, decimal remaining, decimal carryForward)
    {
        UserId = userId;
        LeaveTypeId = leaveTypeId;
        Year = year;
        Allocated = allocated;
        Remaining = remaining;
        CarryForward = carryForward;
    }

    public Guid UserId { get; private set; }

    public Guid LeaveTypeId { get; private set; }

    public int Year { get; private set; }

    public decimal Allocated { get; private set; }

    public decimal Taken { get; private set; }

    public decimal Pending { get; private set; }

    public decimal Approved { get; private set; }

    public decimal Remaining { get; private set; }

    public decimal CarryForward { get; private set; }

    public decimal TotalAvailable => Remaining + CarryForward;

    /// <summary>
    /// Creates a leave balance row.
    /// </summary>
    public static LeaveBalance Create(Guid userId, Guid leaveTypeId, int year, decimal allocated, decimal remaining, decimal carryForward = 0)
    {
        return new LeaveBalance(userId, leaveTypeId, year, allocated, remaining, carryForward);
    }

    /// <summary>
    /// Reserves pending leave days.
    /// </summary>
    public void Reserve(decimal days)
    {
        if (days <= 0)
        {
            throw new DomainException("Reserve days must be greater than zero.");
        }

        if (TotalAvailable < days)
        {
            throw new DomainException("Insufficient leave balance.");
        }

        Pending += days;
        Remaining -= Math.Min(Remaining, days);
        Touch();
    }

    /// <summary>
    /// Converts pending days to approved and taken days.
    /// </summary>
    public void Approve(decimal days)
    {
        Pending = Math.Max(0, Pending - days);
        Approved += days;
        Taken += days;
        Touch();
    }

    /// <summary>
    /// Releases pending days back into remaining balance.
    /// </summary>
    public void Release(decimal days)
    {
        Pending = Math.Max(0, Pending - days);
        Remaining += days;
        Touch();
    }

    /// <summary>
    /// Adjusts the remaining balance.
    /// </summary>
    public void Adjust(decimal adjustment)
    {
        var updated = Remaining + adjustment;
        if (updated < 0)
        {
            throw new DomainException("Leave balance cannot become negative.");
        }

        Remaining = updated;
        Touch();
    }

    /// <summary>
    /// Applies carry-forward days.
    /// </summary>
    public void ApplyCarryForward(decimal days)
    {
        CarryForward = days;
        Touch();
    }
}
