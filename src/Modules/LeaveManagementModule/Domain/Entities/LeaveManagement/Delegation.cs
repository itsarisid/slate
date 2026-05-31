using System.ComponentModel.DataAnnotations.Schema;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Models;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents approval authority delegated from one user to another.
/// </summary>
public sealed class Delegation : BaseEntity
{
    private Delegation()
    {
    }

    private Delegation(Guid delegatorUserId, Guid delegateToUserId, LeaveDelegationType delegationType, LeaveDelegationPermission permission, IReadOnlyCollection<Guid> applicableLeaveTypes, IReadOnlyCollection<int> applicableApprovalLevels, IReadOnlyCollection<string> applicableDepartments, IReadOnlyCollection<Guid> applicableEmployees, DateOnly startDate, DateOnly? endDate, string reason, bool isActive)
    {
        DelegatorUserId = delegatorUserId;
        DelegateToUserId = delegateToUserId;
        DelegationType = delegationType;
        Permission = permission;
        ApplicableLeaveTypesJson = LeaveManagementJson.Serialize(applicableLeaveTypes);
        ApplicableApprovalLevelsJson = LeaveManagementJson.Serialize(applicableApprovalLevels);
        ApplicableDepartmentsJson = LeaveManagementJson.Serialize(applicableDepartments);
        ApplicableEmployeesJson = LeaveManagementJson.Serialize(applicableEmployees);
        StartDate = startDate;
        EndDate = endDate;
        Reason = reason.Trim();
        IsActive = isActive;
    }

    public Guid DelegatorUserId { get; private set; }

    public Guid DelegateToUserId { get; private set; }

    public LeaveDelegationType DelegationType { get; private set; }

    public LeaveDelegationPermission Permission { get; private set; }

    public string ApplicableLeaveTypesJson { get; private set; } = "[]";

    public string ApplicableApprovalLevelsJson { get; private set; } = "[]";

    public string ApplicableDepartmentsJson { get; private set; } = "[]";

    public string ApplicableEmployeesJson { get; private set; } = "[]";

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    [NotMapped]
    public IReadOnlyList<Guid> ApplicableLeaveTypes => LeaveManagementJson.DeserializeList<Guid>(ApplicableLeaveTypesJson);

    [NotMapped]
    public IReadOnlyList<int> ApplicableApprovalLevels => LeaveManagementJson.DeserializeList<int>(ApplicableApprovalLevelsJson);

    /// <summary>
    /// Creates a delegation.
    /// </summary>
    public static Delegation Create(Guid delegatorUserId, Guid delegateToUserId, LeaveDelegationType delegationType, LeaveDelegationPermission permission, IReadOnlyCollection<Guid> applicableLeaveTypes, IReadOnlyCollection<int> applicableApprovalLevels, IReadOnlyCollection<string> applicableDepartments, IReadOnlyCollection<Guid> applicableEmployees, DateOnly startDate, DateOnly? endDate, string reason, bool isActive)
    {
        return new Delegation(delegatorUserId, delegateToUserId, delegationType, permission, applicableLeaveTypes, applicableApprovalLevels, applicableDepartments, applicableEmployees, startDate, endDate, reason, isActive);
    }

    /// <summary>
    /// Revokes the delegation.
    /// </summary>
    public void Revoke()
    {
        IsActive = false;
        RevokedAt = DateTimeOffset.UtcNow;
        Touch();
    }

    /// <summary>
    /// Returns true when the delegation is effective on the supplied date.
    /// </summary>
    public bool IsEffective(DateOnly date)
    {
        return IsActive && date >= StartDate && (!EndDate.HasValue || date <= EndDate.Value);
    }
}
