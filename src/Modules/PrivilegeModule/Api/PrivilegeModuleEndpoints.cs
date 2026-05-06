using System.Text;
using System.Text.Json;
using Alphabet.Application.Common.Security;
using Alphabet.Application.Features.Privilege.Commands;
using Alphabet.Application.Features.Privilege.Dtos;
using Alphabet.Application.Features.Privilege.Queries;
using Alphabet.Modules.PrivilegeModule.Api.Models;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Alphabet.Modules.PrivilegeModule.Api;

/// <summary>
/// Maps privilege management and evaluation endpoints.
/// </summary>
public static class PrivilegeModuleEndpoints
{
    /// <summary>
    /// Registers the privilege module endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapPrivilegeModule(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        MapPrivilegeManagement(endpoints, versionSet);
        MapRoleAssignments(endpoints, versionSet);
        MapUserAssignments(endpoints, versionSet);
        MapPrivilegeEvaluation(endpoints, versionSet);
        MapPrivilegeAdministration(endpoints, versionSet);
        MapSelfService(endpoints, versionSet);
        return endpoints;
    }

    private static void MapPrivilegeManagement(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/privileges")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Privilege Module")
            .RequireAuthorization("PrivilegeManagers");

        group.MapPost("/", async Task<Results<Created<Guid>, BadRequest<ProblemDetails>>> (
            [FromBody] CreatePrivilegeRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreatePrivilegeCommand(
                request.Name,
                request.DisplayName,
                request.Description,
                request.Category,
                request.ResourceType,
                request.Actions,
                request.IsGlobal,
                request.DependsOn,
                request.Attributes), ct);

            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Privilege creation failed", Detail = result.Error })
                : TypedResults.Created($"/api/v1/privileges/{result.Value}", result.Value);
        })
        .Accepts<CreatePrivilegeRequest>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreatePrivilege")
        .WithSummary("Creates a new privilege definition.")
        .WithDescription("Creates a fine-grained privilege that can later be assigned to roles, users, or composite policies. Categories are auto-created when needed.");

        group.MapGet("/", async Task<Ok<PagedResponseDto<PrivilegeDto>>> (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            [FromQuery] string? category,
            [FromQuery] string? search,
            [FromQuery] bool includeDeprecated,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPrivilegesQuery(pageNumber <= 0 ? 1 : pageNumber, pageSize <= 0 ? 50 : pageSize, category, search, includeDeprecated), ct);
            return TypedResults.Ok(result);
        })
        .Produces<PagedResponseDto<PrivilegeDto>>(StatusCodes.Status200OK)
        .WithName("GetPrivileges")
        .WithSummary("Gets a paginated list of privileges.")
        .WithDescription("Returns privileges with optional pagination, category filtering, full-text search, and deprecated inclusion.");

        group.MapGet("/{privilegeId:guid}", async Task<Results<Ok<PrivilegeDto>, NotFound<ProblemDetails>>> (
            Guid privilegeId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPrivilegeByIdQuery(privilegeId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.NotFound(new ProblemDetails { Title = "Privilege not found", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Produces<PrivilegeDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .WithName("GetPrivilegeById")
        .WithSummary("Gets a single privilege definition.")
        .WithDescription("Returns privilege metadata, dependencies, actions, and category details for the specified privilege id.");

        group.MapPut("/{privilegeId:guid}", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid privilegeId,
            [FromBody] UpdatePrivilegeRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdatePrivilegeCommand(
                privilegeId,
                request.DisplayName,
                request.Description,
                request.Category,
                request.ResourceType,
                request.Actions,
                request.DependsOn,
                request.Attributes), ct);

            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Privilege update failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<UpdatePrivilegeRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("UpdatePrivilege")
        .WithSummary("Updates privilege metadata.")
        .WithDescription("Updates a privilege's mutable fields including display name, description, category, attributes, actions, and dependencies. The privilege name remains immutable.");

        group.MapDelete("/{privilegeId:guid}", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid privilegeId,
            [FromQuery] bool hardDelete,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DeletePrivilegeCommand(privilegeId, hardDelete), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Privilege deletion failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("DeletePrivilege")
        .WithSummary("Deprecates or hard-deletes a privilege.")
        .WithDescription("Soft-deletes a privilege by default. When hardDelete is true, the API attempts a permanent delete and rejects the operation if the privilege is still assigned.");

        group.MapPost("/categories", async Task<Results<Created<Guid>, BadRequest<ProblemDetails>>> (
            [FromBody] CreatePrivilegeCategoryRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreatePrivilegeCategoryCommand(request.Name, request.Description, request.ParentCategoryId, request.SortOrder), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Category creation failed", Detail = result.Error })
                : TypedResults.Created($"/api/v1/privileges/categories/{result.Value}", result.Value);
        })
        .Accepts<CreatePrivilegeCategoryRequest>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreatePrivilegeCategory")
        .WithSummary("Creates a privilege category.")
        .WithDescription("Creates a privilege category and optionally places it under a parent category to build a hierarchy.");

        group.MapGet("/categories", async Task<Ok<IReadOnlyList<PrivilegeCategoryDto>>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPrivilegeCategoriesQuery(), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<PrivilegeCategoryDto>>(StatusCodes.Status200OK)
        .WithName("GetPrivilegeCategories")
        .WithSummary("Gets all privilege categories.")
        .WithDescription("Returns a hierarchical tree of privilege categories that can be used to organize and browse the permission catalog.");

        group.MapPatch("/{privilegeId:guid}/category", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid privilegeId,
            [FromBody] MovePrivilegeCategoryRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new MovePrivilegeCategoryCommand(privilegeId, request.CategoryId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Move privilege category failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<MovePrivilegeCategoryRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("MovePrivilegeCategory")
        .WithSummary("Moves a privilege to a different category.")
        .WithDescription("Reassigns an existing privilege to another privilege category.");

        group.MapPost("/policies", async Task<Results<Created<Guid>, BadRequest<ProblemDetails>>> (
            [FromBody] CreatePrivilegePolicyRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreatePrivilegePolicyCommand(request.Name, request.Description, request.Privileges, request.Condition), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Policy creation failed", Detail = result.Error })
                : TypedResults.Created($"/api/v1/privileges/policies/{result.Value}", result.Value);
        })
        .Accepts<CreatePrivilegePolicyRequest>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreatePrivilegePolicy")
        .WithSummary("Creates a composite privilege policy.")
        .WithDescription("Creates a reusable policy that groups multiple privileges together using either all-required or any-required evaluation semantics.");
    }

    private static void MapRoleAssignments(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/roles")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Privilege Module")
            .RequireAuthorization("PrivilegeManagers");

        group.MapPost("/{roleId:guid}/privileges", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid roleId,
            [FromBody] AssignRolePrivilegesRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AssignPrivilegeToRoleCommand(roleId, request.PrivilegeIds, request.ExpiresAt), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Assign role privileges failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<AssignRolePrivilegesRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AssignPrivilegeToRole")
        .WithSummary("Assigns privileges to a role.")
        .WithDescription("Grants one or more privileges to a role and optionally applies an expiration date to the assignment.");

        group.MapGet("/{roleId:guid}/privileges", async Task<Ok<IReadOnlyList<PrivilegeAssignmentDto>>> (
            Guid roleId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRolePrivilegesQuery(roleId), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<PrivilegeAssignmentDto>>(StatusCodes.Status200OK)
        .WithName("GetRolePrivileges")
        .WithSummary("Gets privileges assigned to a role.")
        .WithDescription("Returns direct role privilege assignments together with grant metadata and active status.");

        group.MapDelete("/{roleId:guid}/privileges/{privilegeId:guid}", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid roleId,
            Guid privilegeId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RevokePrivilegeFromRoleCommand(roleId, privilegeId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Revoke role privilege failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("RevokePrivilegeFromRole")
        .WithSummary("Revokes a privilege from a role.")
        .WithDescription("Deactivates a privilege assignment for the specified role.");

        group.MapPost("/bulk/assign-privileges", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            [FromBody] BulkRolePrivilegeRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new BulkAssignPrivilegesCommand(request.RoleIds, request.PrivilegeIds, request.Operation, request.ExpiresAt), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Bulk role privilege operation failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<BulkRolePrivilegeRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("BulkAssignPrivilegesToRoles")
        .WithSummary("Bulk assigns or revokes privileges for roles.")
        .WithDescription("Adds or removes one or more privileges across multiple roles in a single operation.");

        group.MapPost("/{roleId:guid}/policies", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid roleId,
            [FromBody] AssignPolicyRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AssignPolicyToRoleCommand(roleId, request.PolicyId, request.ExpiresAt), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Assign role policy failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<AssignPolicyRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AssignPolicyToRole")
        .WithSummary("Assigns a privilege policy to a role.")
        .WithDescription("Associates a composite privilege policy with a role so the policy's privileges are evaluated through the role.");
    }

    private static void MapUserAssignments(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var adminGroup = endpoints.MapGroup("api/v{version:apiVersion}/users")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Privilege Module")
            .RequireAuthorization("PrivilegeManagers");

        adminGroup.MapPost("/{userId:guid}/privileges", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid userId,
            [FromBody] AssignUserPrivilegeRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AssignPrivilegeToUserCommand(userId, request.PrivilegeId, request.Effect, request.ExpiresAt, request.Reason), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Assign user privilege failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<AssignUserPrivilegeRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AssignPrivilegeToUser")
        .WithSummary("Assigns a direct privilege to a user.")
        .WithDescription("Creates a direct allow or deny assignment for a user. Direct denies override role-based allows during privilege evaluation.");

        adminGroup.MapGet("/{userId:guid}/privileges/effective", async Task<Ok<IReadOnlyList<UserEffectivePrivilegeDto>>> (
            Guid userId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUserEffectivePrivilegesQuery(userId), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<UserEffectivePrivilegeDto>>(StatusCodes.Status200OK)
        .WithName("GetUserEffectivePrivileges")
        .WithSummary("Gets the user's effective privileges.")
        .WithDescription("Returns the full evaluated privilege set for the user after combining role-based grants, direct user grants, direct user denies, and composite policies.");

        adminGroup.MapDelete("/{userId:guid}/privileges/{privilegeId:guid}", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid userId,
            Guid privilegeId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RevokePrivilegeFromUserCommand(userId, privilegeId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Revoke user privilege failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("RevokePrivilegeFromUser")
        .WithSummary("Revokes a direct privilege from a user.")
        .WithDescription("Revokes the user's direct privilege assignment while preserving the audit trail.");

        adminGroup.MapGet("/{userId:guid}/privileges/audit", async Task<Ok<IReadOnlyList<PrivilegeAuditLogDto>>> (
            Guid userId,
            [FromQuery] int take,
            [FromQuery] int skip,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPrivilegeAuditLogQuery(userId, null, null, null, null, take <= 0 ? 100 : take, skip < 0 ? 0 : skip), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<PrivilegeAuditLogDto>>(StatusCodes.Status200OK)
        .WithName("GetUserPrivilegeAudit")
        .WithSummary("Gets privilege audit history for a user.")
        .WithDescription("Returns assignment, revocation, and evaluation events for the selected user's privilege history.");

        adminGroup.MapPost("/{userId:guid}/policies", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid userId,
            [FromBody] AssignPolicyRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AssignPolicyToUserCommand(userId, request.PolicyId, request.ExpiresAt), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Assign user policy failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<AssignPolicyRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AssignPolicyToUser")
        .WithSummary("Assigns a privilege policy directly to a user.")
        .WithDescription("Associates a composite policy with a specific user outside role membership.");
    }

    private static void MapPrivilegeEvaluation(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/auth")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Privilege Module")
            .RequireAuthorization();

        group.MapGet("/check-privilege/{privilegeName}", async Task<Results<Ok<PrivilegeCheckResultDto>, BadRequest<ProblemDetails>>> (
            string privilegeName,
            HttpContext httpContext,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            if (!TryResolveCurrentUserId(httpContext, out var userId))
            {
                return TypedResults.BadRequest(new ProblemDetails { Title = "Current user was not resolved." });
            }

            var result = await sender.Send(new CheckUserPrivilegeQuery(userId, privilegeName), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Privilege check failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Produces<PrivilegeCheckResultDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CheckCurrentUserPrivilege")
        .WithSummary("Checks whether the current user has a privilege.")
        .WithDescription("Evaluates a single privilege for the authenticated user and returns whether it is currently granted, together with the source that granted it.");

        group.MapPost("/check-privileges", async Task<Results<Ok<IReadOnlyDictionary<string, bool>>, BadRequest<ProblemDetails>>> (
            [FromBody] BatchPrivilegeCheckRequest request,
            HttpContext httpContext,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            if (!TryResolveCurrentUserId(httpContext, out var userId))
            {
                return TypedResults.BadRequest(new ProblemDetails { Title = "Current user was not resolved." });
            }

            var result = await sender.Send(new BatchCheckUserPrivilegesQuery(userId, request.Privileges), ct);
            return TypedResults.Ok(result);
        })
        .Accepts<BatchPrivilegeCheckRequest>("application/json")
        .Produces<IReadOnlyDictionary<string, bool>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("BatchCheckCurrentUserPrivileges")
        .WithSummary("Checks multiple privileges for the current user.")
        .WithDescription("Evaluates multiple privilege names in a single request and returns a privilege-to-boolean map for the authenticated user.");
    }

    private static void MapPrivilegeAdministration(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/admin")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Privilege Module")
            .RequireAuthorization("PrivilegeManagers");

        group.MapGet("/privileges/analytics", async Task<Ok<PrivilegeAnalyticsDto>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPrivilegeAnalyticsQuery(), ct);
            return TypedResults.Ok(result);
        })
        .Produces<PrivilegeAnalyticsDto>(StatusCodes.Status200OK)
        .WithName("GetPrivilegeAnalytics")
        .WithSummary("Gets privilege usage analytics.")
        .WithDescription("Returns privilege usage metrics, unused privileges, and assignment trends to support governance and access reviews.");

        group.MapGet("/audit/privileges", async Task<Ok<IReadOnlyList<PrivilegeAuditLogDto>>> (
            [FromQuery] Guid? userId,
            [FromQuery] Guid? privilegeId,
            [FromQuery] Alphabet.Domain.Enums.PrivilegeAction? action,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to,
            [FromQuery] int take,
            [FromQuery] int skip,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPrivilegeAuditLogQuery(userId, privilegeId, action, from, to, take <= 0 ? 100 : take, skip < 0 ? 0 : skip), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<PrivilegeAuditLogDto>>(StatusCodes.Status200OK)
        .WithName("GetPrivilegeAuditLogs")
        .WithSummary("Gets privilege audit logs.")
        .WithDescription("Searches privilege audit logs by user, privilege, action, and date range for governance and operational investigations.");

        group.MapGet("/privileges/export", async Task<IResult> (
            [FromQuery] string format,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var privileges = await sender.Send(new GetPrivilegesQuery(1, 5000, null, null, true), ct);
            var normalizedFormat = string.IsNullOrWhiteSpace(format) ? "json" : format.Trim().ToLowerInvariant();

            if (normalizedFormat == "csv")
            {
                var builder = new StringBuilder();
                builder.AppendLine("Id,Name,DisplayName,Category,IsDeprecated,IsGlobal");
                foreach (var privilege in privileges.Items)
                {
                    builder.AppendLine($"{privilege.Id},{privilege.Name},{Escape(privilege.DisplayName)},{Escape(privilege.CategoryName ?? string.Empty)},{privilege.IsDeprecated},{privilege.IsGlobal}");
                }

                return Results.File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "privileges.csv");
            }

            return Results.Json(privileges.Items, contentType: "application/json");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("ExportPrivilegeMatrix")
        .WithSummary("Exports the privilege catalog.")
        .WithDescription("Exports the privilege catalog in JSON or CSV format for governance reviews, reporting, and offline analysis.");

        group.MapPost("/privilege-requests/{requestId:guid}/approve", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid requestId,
            [FromBody] DecidePrivilegeRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ApprovePrivilegeRequestCommand(requestId, request.Notes), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Approve privilege request failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<DecidePrivilegeRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("ApprovePrivilegeRequest")
        .WithSummary("Approves a self-service privilege request.")
        .WithDescription("Approves a pending user privilege request and grants the requested privilege for the approved duration.");

        group.MapPost("/privilege-requests/{requestId:guid}/deny", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid requestId,
            [FromBody] DecidePrivilegeRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DenyPrivilegeRequestCommand(requestId, request.Notes), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Deny privilege request failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<DecidePrivilegeRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("DenyPrivilegeRequest")
        .WithSummary("Denies a self-service privilege request.")
        .WithDescription("Denies a pending user privilege request while preserving the request and decision details for audit.");
    }

    private static void MapSelfService(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/users/me")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Privilege Module")
            .RequireAuthorization();

        group.MapPost("/privilege-requests", async Task<Results<Created<Guid>, BadRequest<ProblemDetails>>> (
            [FromBody] CreatePrivilegeAccessRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreatePrivilegeRequestCommand(request.PrivilegeId, request.Reason, request.RequestedDurationDays, request.ApproverEmail), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Privilege request creation failed", Detail = result.Error })
                : TypedResults.Created($"/api/v1/users/me/privilege-requests/{result.Value}", result.Value);
        })
        .Accepts<CreatePrivilegeAccessRequest>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreatePrivilegeRequest")
        .WithSummary("Requests temporary additional privilege access.")
        .WithDescription("Allows the authenticated user to request extra privilege access for a limited period, subject to approval workflows.");

        group.MapGet("/privileges", async Task<Ok<IReadOnlyList<UserEffectivePrivilegeDto>>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMyPrivilegesQuery(), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<UserEffectivePrivilegeDto>>(StatusCodes.Status200OK)
        .WithName("GetMyPrivileges")
        .WithSummary("Gets the authenticated user's current privileges.")
        .WithDescription("Returns the authenticated user's current effective privileges after all role, user, and policy rules have been applied.");
    }

    private static bool TryResolveCurrentUserId(HttpContext httpContext, out Guid userId)
    {
        return Guid.TryParse(httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out userId);
    }

    private static string Escape(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
