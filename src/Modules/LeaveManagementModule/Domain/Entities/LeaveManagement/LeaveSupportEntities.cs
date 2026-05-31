using Alphabet.Domain.Enums;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a public holiday that can affect leave-day calculation.
/// </summary>
public sealed class PublicHoliday : BaseEntity
{
    private PublicHoliday()
    {
    }

    private PublicHoliday(string name, DateOnly date, string country, string? state, bool isPaid, bool recurring)
    {
        Name = name.Trim();
        Date = date;
        Country = country.Trim().ToUpperInvariant();
        State = string.IsNullOrWhiteSpace(state) ? null : state.Trim().ToUpperInvariant();
        IsPaid = isPaid;
        Recurring = recurring;
    }

    public string Name { get; private set; } = string.Empty;

    public DateOnly Date { get; private set; }

    public string Country { get; private set; } = string.Empty;

    public string? State { get; private set; }

    public bool IsPaid { get; private set; }

    public bool Recurring { get; private set; }

    /// <summary>
    /// Creates a public holiday.
    /// </summary>
    public static PublicHoliday Create(string name, DateOnly date, string country, string? state, bool isPaid, bool recurring)
    {
        return new PublicHoliday(name, date, country, state, isPaid, recurring);
    }
}

/// <summary>
/// Represents a blackout period where leave is blocked or requires override.
/// </summary>
public sealed class BlackoutPeriod : BaseEntity
{
    private BlackoutPeriod()
    {
    }

    private BlackoutPeriod(DateOnly startDate, DateOnly endDate, string reason, IReadOnlyCollection<string> applicableTo, bool isActive)
    {
        StartDate = startDate;
        EndDate = endDate;
        Reason = reason.Trim();
        ApplicableToJson = LeaveManagementJson.Serialize(applicableTo);
        IsActive = isActive;
    }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public string ApplicableToJson { get; private set; } = "[]";

    public bool IsActive { get; private set; }

    /// <summary>
    /// Creates a blackout period.
    /// </summary>
    public static BlackoutPeriod Create(DateOnly startDate, DateOnly endDate, string reason, IReadOnlyCollection<string> applicableTo, bool isActive = true)
    {
        return new BlackoutPeriod(startDate, endDate, reason, applicableTo, isActive);
    }
}

/// <summary>
/// Represents accrual rules for a leave type.
/// </summary>
public sealed class AccrualRule : BaseEntity
{
    private AccrualRule()
    {
    }

    private AccrualRule(Guid leaveTypeId, LeaveAccrualMethod accrualMethod, decimal accrualRate, decimal maxAccrual, string tenureRulesJson, bool isActive)
    {
        LeaveTypeId = leaveTypeId;
        AccrualMethod = accrualMethod;
        AccrualRate = accrualRate;
        MaxAccrual = maxAccrual;
        TenureRulesJson = tenureRulesJson;
        IsActive = isActive;
    }

    public Guid LeaveTypeId { get; private set; }

    public LeaveAccrualMethod AccrualMethod { get; private set; }

    public decimal AccrualRate { get; private set; }

    public decimal MaxAccrual { get; private set; }

    public string TenureRulesJson { get; private set; } = "[]";

    public bool IsActive { get; private set; }

    /// <summary>
    /// Creates an accrual rule.
    /// </summary>
    public static AccrualRule Create(Guid leaveTypeId, LeaveAccrualMethod accrualMethod, decimal accrualRate, decimal maxAccrual, string tenureRulesJson, bool isActive = true)
    {
        return new AccrualRule(leaveTypeId, accrualMethod, accrualRate, maxAccrual, tenureRulesJson, isActive);
    }
}

/// <summary>
/// Represents an immutable audit log entry for leave operations.
/// </summary>
public sealed class LeaveActivityLog : BaseEntity
{
    private LeaveActivityLog()
    {
    }

    private LeaveActivityLog(Guid? userId, Guid? leaveRequestId, string action, string? oldValueJson, string? newValueJson, string? ipAddress, string? userAgent, string? detailsJson)
    {
        UserId = userId;
        LeaveRequestId = leaveRequestId;
        Action = action.Trim();
        OldValueJson = oldValueJson;
        NewValueJson = newValueJson;
        Timestamp = DateTimeOffset.UtcNow;
        IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();
        UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim();
        DetailsJson = detailsJson;
    }

    public Guid? UserId { get; private set; }

    public Guid? LeaveRequestId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string? OldValueJson { get; private set; }

    public string? NewValueJson { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public string? DetailsJson { get; private set; }

    /// <summary>
    /// Creates an activity log entry.
    /// </summary>
    public static LeaveActivityLog Create(Guid? userId, Guid? leaveRequestId, string action, string? oldValueJson, string? newValueJson, string? ipAddress, string? userAgent, string? detailsJson)
    {
        return new LeaveActivityLog(userId, leaveRequestId, action, oldValueJson, newValueJson, ipAddress, userAgent, detailsJson);
    }
}
