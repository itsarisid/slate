using Alphabet.Common.Models;

namespace Alphabet.Modules.PrivilegeModule.Api.Resource;

public static class ApiResource
{
    // ── Privilege Management ──────────────────────────────────────────

    public static EndpointDetails CreatePrivilege => new()
    {
        Endpoint = "/",
        Name = "CreatePrivilege",
        Summary = "Creates a new privilege definition.",
        Description = "Creates a fine-grained privilege that can later be assigned to roles, users, or composite policies. Categories are auto-created when needed."
    };

    public static EndpointDetails GetPrivileges => new()
    {
        Endpoint = "/",
        Name = "GetPrivileges",
        Summary = "Gets a paginated list of privileges.",
        Description = "Returns privileges with optional pagination, category filtering, full-text search, and deprecated inclusion."
    };

    public static EndpointDetails GetPrivilegeById => new()
    {
        Endpoint = "/{privilegeId:guid}",
        Name = "GetPrivilegeById",
        Summary = "Gets a single privilege definition.",
        Description = "Returns privilege metadata, dependencies, actions, and category details for the specified privilege id."
    };

    public static EndpointDetails UpdatePrivilege => new()
    {
        Endpoint = "/{privilegeId:guid}",
        Name = "UpdatePrivilege",
        Summary = "Updates privilege metadata.",
        Description = "Updates a privilege's mutable fields including display name, description, category, attributes, actions, and dependencies. The privilege name remains immutable."
    };

    public static EndpointDetails DeletePrivilege => new()
    {
        Endpoint = "/{privilegeId:guid}",
        Name = "DeletePrivilege",
        Summary = "Deprecates or hard-deletes a privilege.",
        Description = "Soft-deletes a privilege by default. When hardDelete is true, the API attempts a permanent delete and rejects the operation if the privilege is still assigned."
    };

    public static EndpointDetails CreatePrivilegeCategory => new()
    {
        Endpoint = "/categories",
        Name = "CreatePrivilegeCategory",
        Summary = "Creates a privilege category.",
        Description = "Creates a privilege category and optionally places it under a parent category to build a hierarchy."
    };

    public static EndpointDetails GetPrivilegeCategories => new()
    {
        Endpoint = "/categories",
        Name = "GetPrivilegeCategories",
        Summary = "Gets all privilege categories.",
        Description = "Returns a hierarchical tree of privilege categories that can be used to organize and browse the permission catalog."
    };

    public static EndpointDetails MovePrivilegeCategory => new()
    {
        Endpoint = "/{privilegeId:guid}/category",
        Name = "MovePrivilegeCategory",
        Summary = "Moves a privilege to a different category.",
        Description = "Reassigns an existing privilege to another privilege category."
    };

    public static EndpointDetails CreatePrivilegePolicy => new()
    {
        Endpoint = "/policies",
        Name = "CreatePrivilegePolicy",
        Summary = "Creates a composite privilege policy.",
        Description = "Creates a reusable policy that groups multiple privileges together using either all-required or any-required evaluation semantics."
    };

    // ── Role Assignments ──────────────────────────────────────────────

    public static EndpointDetails AssignPrivilegeToRole => new()
    {
        Endpoint = "/{roleId:guid}/privileges",
        Name = "AssignPrivilegeToRole",
        Summary = "Assigns privileges to a role.",
        Description = "Grants one or more privileges to a role and optionally applies an expiration date to the assignment."
    };

    public static EndpointDetails GetRolePrivileges => new()
    {
        Endpoint = "/{roleId:guid}/privileges",
        Name = "GetRolePrivileges",
        Summary = "Gets privileges assigned to a role.",
        Description = "Returns direct role privilege assignments together with grant metadata and active status."
    };

    public static EndpointDetails RevokePrivilegeFromRole => new()
    {
        Endpoint = "/{roleId:guid}/privileges/{privilegeId:guid}",
        Name = "RevokePrivilegeFromRole",
        Summary = "Revokes a privilege from a role.",
        Description = "Deactivates a privilege assignment for the specified role."
    };

    public static EndpointDetails BulkAssignPrivilegesToRoles => new()
    {
        Endpoint = "/bulk/assign-privileges",
        Name = "BulkAssignPrivilegesToRoles",
        Summary = "Bulk assigns or revokes privileges for roles.",
        Description = "Adds or removes one or more privileges across multiple roles in a single operation."
    };

    public static EndpointDetails AssignPolicyToRole => new()
    {
        Endpoint = "/{roleId:guid}/policies",
        Name = "AssignPolicyToRole",
        Summary = "Assigns a privilege policy to a role.",
        Description = "Associates a composite privilege policy with a role so the policy's privileges are evaluated through the role."
    };

    // ── User Assignments ──────────────────────────────────────────────

    public static EndpointDetails AssignPrivilegeToUser => new()
    {
        Endpoint = "/{userId:guid}/privileges",
        Name = "AssignPrivilegeToUser",
        Summary = "Assigns a direct privilege to a user.",
        Description = "Creates a direct allow or deny assignment for a user. Direct denies override role-based allows during privilege evaluation."
    };

    public static EndpointDetails GetUserEffectivePrivileges => new()
    {
        Endpoint = "/{userId:guid}/privileges/effective",
        Name = "GetUserEffectivePrivileges",
        Summary = "Gets the user's effective privileges.",
        Description = "Returns the full evaluated privilege set for the user after combining role-based grants, direct user grants, direct user denies, and composite policies."
    };

    public static EndpointDetails RevokePrivilegeFromUser => new()
    {
        Endpoint = "/{userId:guid}/privileges/{privilegeId:guid}",
        Name = "RevokePrivilegeFromUser",
        Summary = "Revokes a direct privilege from a user.",
        Description = "Revokes the user's direct privilege assignment while preserving the audit trail."
    };

    public static EndpointDetails GetUserPrivilegeAudit => new()
    {
        Endpoint = "/{userId:guid}/privileges/audit",
        Name = "GetUserPrivilegeAudit",
        Summary = "Gets privilege audit history for a user.",
        Description = "Returns assignment, revocation, and evaluation events for the selected user's privilege history."
    };

    public static EndpointDetails AssignPolicyToUser => new()
    {
        Endpoint = "/{userId:guid}/policies",
        Name = "AssignPolicyToUser",
        Summary = "Assigns a privilege policy directly to a user.",
        Description = "Associates a composite policy with a specific user outside role membership."
    };

    // ── Privilege Evaluation ──────────────────────────────────────────

    public static EndpointDetails CheckCurrentUserPrivilege => new()
    {
        Endpoint = "/check-privilege/{privilegeName}",
        Name = "CheckCurrentUserPrivilege",
        Summary = "Checks whether the current user has a privilege.",
        Description = "Evaluates a single privilege for the authenticated user and returns whether it is currently granted, together with the source that granted it."
    };

    public static EndpointDetails BatchCheckCurrentUserPrivileges => new()
    {
        Endpoint = "/check-privileges",
        Name = "BatchCheckCurrentUserPrivileges",
        Summary = "Checks multiple privileges for the current user.",
        Description = "Evaluates multiple privilege names in a single request and returns a privilege-to-boolean map for the authenticated user."
    };

    // ── Privilege Administration ──────────────────────────────────────

    public static EndpointDetails GetPrivilegeAnalytics => new()
    {
        Endpoint = "/privileges/analytics",
        Name = "GetPrivilegeAnalytics",
        Summary = "Gets privilege usage analytics.",
        Description = "Returns privilege usage metrics, unused privileges, and assignment trends to support governance and access reviews."
    };

    public static EndpointDetails GetPrivilegeAuditLogs => new()
    {
        Endpoint = "/audit/privileges",
        Name = "GetPrivilegeAuditLogs",
        Summary = "Gets privilege audit logs.",
        Description = "Searches privilege audit logs by user, privilege, action, and date range for governance and operational investigations."
    };

    public static EndpointDetails ExportPrivilegeMatrix => new()
    {
        Endpoint = "/privileges/export",
        Name = "ExportPrivilegeMatrix",
        Summary = "Exports the privilege catalog.",
        Description = "Exports the privilege catalog in JSON or CSV format for governance reviews, reporting, and offline analysis."
    };

    public static EndpointDetails ApprovePrivilegeRequest => new()
    {
        Endpoint = "/privilege-requests/{requestId:guid}/approve",
        Name = "ApprovePrivilegeRequest",
        Summary = "Approves a self-service privilege request.",
        Description = "Approves a pending user privilege request and grants the requested privilege for the approved duration."
    };

    public static EndpointDetails DenyPrivilegeRequest => new()
    {
        Endpoint = "/privilege-requests/{requestId:guid}/deny",
        Name = "DenyPrivilegeRequest",
        Summary = "Denies a self-service privilege request.",
        Description = "Denies a pending user privilege request while preserving the request and decision details for audit."
    };

    // ── Self Service ──────────────────────────────────────────────────

    public static EndpointDetails CreatePrivilegeRequest => new()
    {
        Endpoint = "/privilege-requests",
        Name = "CreatePrivilegeRequest",
        Summary = "Requests temporary additional privilege access.",
        Description = "Allows the authenticated user to request extra privilege access for a limited period, subject to approval workflows."
    };

    public static EndpointDetails GetMyPrivileges => new()
    {
        Endpoint = "/privileges",
        Name = "GetMyPrivileges",
        Summary = "Gets the authenticated user's current privileges.",
        Description = "Returns the authenticated user's current effective privileges after all role, user, and policy rules have been applied."
    };

}
