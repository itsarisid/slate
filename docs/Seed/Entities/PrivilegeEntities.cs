// Entities inferred from CreatePrivilegeRequest/UpdatePrivilegeRequest,
// CreatePrivilegeCategoryRequest, CreatePrivilegePolicyRequest,
// AssignUserPrivilegeRequest/AssignRolePrivilegesRequest,
// CreatePrivilegeAccessRequest/DecidePrivilegeRequest, and the read-side DTOs
// (PrivilegeDto, PrivilegeAssignmentDto, PrivilegeAuditLogDto) in swagger.json.

using System;
using System.Collections.Generic;

namespace Alphabet.Domain.Entities
{
    public class PrivilegeCategory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public int SortOrder { get; set; }
    }

    public class Privilege
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!; // machine key, e.g. "assets.write"
        public string DisplayName { get; set; } = default!;
        public string? Description { get; set; }
        public Guid? CategoryId { get; set; }
        public string? ResourceType { get; set; }
        public List<string> Actions { get; set; } = new();
        public bool IsGlobal { get; set; }
        public bool IsDeprecated { get; set; }
        public List<string> DependsOn { get; set; } = new(); // names of prerequisite privileges
        public Dictionary<string, string?> Attributes { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PrivilegePolicy
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public List<string> PrivilegeNames { get; set; } = new();
        public PrivilegePolicyCondition Condition { get; set; }
    }

    public class UserPrivilegeAssignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid PrivilegeId { get; set; }
        public PrivilegeEffect Effect { get; set; }
        public string AssignmentSource { get; set; } = "Direct"; // "Direct" | "Role" | "Policy"
        public DateTime GrantedAt { get; set; }
        public string? GrantedBy { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Reason { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class RolePrivilegeAssignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RoleId { get; set; }
        public Guid PrivilegeId { get; set; }
        public DateTime GrantedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PrivilegeAccessRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid PrivilegeId { get; set; }
        public string? Reason { get; set; }
        public int RequestedDurationDays { get; set; }
        public string? ApproverEmail { get; set; }
        public PrivilegeAccessRequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? DecisionNotes { get; set; }
    }

    public class PrivilegeAuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? UserId { get; set; }
        public Guid? PrivilegeId { get; set; }
        public string Action { get; set; } = default!; // e.g. "Granted", "Revoked", "Checked", "Denied"
        public string? Source { get; set; }
        public string? PerformedBy { get; set; }
        public DateTime PerformedAt { get; set; }
        public string? IpAddress { get; set; }
        public Dictionary<string, string?> Metadata { get; set; } = new();
    }
}
