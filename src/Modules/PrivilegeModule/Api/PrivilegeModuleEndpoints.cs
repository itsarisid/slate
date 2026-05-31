using Alphabet.Modules.PrivilegeModule.Api.Resource;
using Alphabet.Common.Extensions;
using System.Text;
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
    /// <summary>
    /// Map privilege management.
    /// </summary>

    private static void MapPrivilegeManagement(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/privileges")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Privilege Module")
            .RequireAuthorization("PrivilegeManagers");

        group.MapPost(ApiResource.CreatePrivilege.Endpoint, async Task<Results<Created<Guid>, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.CreatePrivilege);

        group.MapGet(ApiResource.GetPrivileges.Endpoint, async Task<Ok<PagedResponseDto<PrivilegeDto>>> (
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
        .WithDocumentation(ApiResource.GetPrivileges);

        group.MapGet(ApiResource.GetPrivilegeById.Endpoint, async Task<Results<Ok<PrivilegeDto>, NotFound<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.GetPrivilegeById);

        group.MapPut(ApiResource.UpdatePrivilege.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.UpdatePrivilege);

        group.MapDelete(ApiResource.DeletePrivilege.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.DeletePrivilege);

        group.MapPost(ApiResource.CreatePrivilegeCategory.Endpoint, async Task<Results<Created<Guid>, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.CreatePrivilegeCategory);

        group.MapGet(ApiResource.GetPrivilegeCategories.Endpoint, async Task<Ok<IReadOnlyList<PrivilegeCategoryDto>>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPrivilegeCategoriesQuery(), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<PrivilegeCategoryDto>>(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetPrivilegeCategories);

        group.MapPatch(ApiResource.MovePrivilegeCategory.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.MovePrivilegeCategory);

        group.MapPost(ApiResource.CreatePrivilegePolicy.Endpoint, async Task<Results<Created<Guid>, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.CreatePrivilegePolicy);
    }
    /// <summary>
    /// Map role assignments.
    /// </summary>

    private static void MapRoleAssignments(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/roles")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Privilege Module")
            .RequireAuthorization("PrivilegeManagers");

        group.MapPost(ApiResource.AssignPrivilegeToRole.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.AssignPrivilegeToRole);

        group.MapGet(ApiResource.GetRolePrivileges.Endpoint, async Task<Ok<IReadOnlyList<PrivilegeAssignmentDto>>> (
            Guid roleId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRolePrivilegesQuery(roleId), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<PrivilegeAssignmentDto>>(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetRolePrivileges);

        group.MapDelete(ApiResource.RevokePrivilegeFromRole.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.RevokePrivilegeFromRole);

        group.MapPost(ApiResource.BulkAssignPrivilegesToRoles.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.BulkAssignPrivilegesToRoles);

        group.MapPost(ApiResource.AssignPolicyToRole.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.AssignPolicyToRole);
    }
    /// <summary>
    /// Map user assignments.
    /// </summary>

    private static void MapUserAssignments(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var adminGroup = endpoints.MapGroup("api/v{version:apiVersion}/users")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Privilege Module")
            .RequireAuthorization("PrivilegeManagers");

        adminGroup.MapPost(ApiResource.AssignPrivilegeToUser.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.AssignPrivilegeToUser);

        adminGroup.MapGet(ApiResource.GetUserEffectivePrivileges.Endpoint, async Task<Ok<IReadOnlyList<UserEffectivePrivilegeDto>>> (
            Guid userId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUserEffectivePrivilegesQuery(userId), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<UserEffectivePrivilegeDto>>(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetUserEffectivePrivileges);

        adminGroup.MapDelete(ApiResource.RevokePrivilegeFromUser.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.RevokePrivilegeFromUser);

        adminGroup.MapGet(ApiResource.GetUserPrivilegeAudit.Endpoint, async Task<Ok<IReadOnlyList<PrivilegeAuditLogDto>>> (
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
        .WithDocumentation(ApiResource.GetUserPrivilegeAudit);

        adminGroup.MapPost(ApiResource.AssignPolicyToUser.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.AssignPolicyToUser);
    }
    /// <summary>
    /// Map privilege evaluation.
    /// </summary>

    private static void MapPrivilegeEvaluation(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/auth")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Privilege Module")
            .RequireAuthorization();

        group.MapGet(ApiResource.CheckCurrentUserPrivilege.Endpoint, async Task<Results<Ok<PrivilegeCheckResultDto>, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.CheckCurrentUserPrivilege);

        group.MapPost(ApiResource.BatchCheckCurrentUserPrivileges.Endpoint, async Task<Results<Ok<IReadOnlyDictionary<string, bool>>, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.BatchCheckCurrentUserPrivileges);
    }
    /// <summary>
    /// Map privilege administration.
    /// </summary>

    private static void MapPrivilegeAdministration(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/admin")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Privilege Module")
            .RequireAuthorization("PrivilegeManagers");

        group.MapGet(ApiResource.GetPrivilegeAnalytics.Endpoint, async Task<Ok<PrivilegeAnalyticsDto>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPrivilegeAnalyticsQuery(), ct);
            return TypedResults.Ok(result);
        })
        .Produces<PrivilegeAnalyticsDto>(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetPrivilegeAnalytics);

        group.MapGet(ApiResource.GetPrivilegeAuditLogs.Endpoint, async Task<Ok<IReadOnlyList<PrivilegeAuditLogDto>>> (
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
        .WithDocumentation(ApiResource.GetPrivilegeAuditLogs);

        group.MapGet(ApiResource.ExportPrivilegeMatrix.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.ExportPrivilegeMatrix);

        group.MapPost(ApiResource.ApprovePrivilegeRequest.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.ApprovePrivilegeRequest);

        group.MapPost(ApiResource.DenyPrivilegeRequest.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.DenyPrivilegeRequest);
    }
    /// <summary>
    /// Map self service.
    /// </summary>

    private static void MapSelfService(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/users/me")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Privilege Module")
            .RequireAuthorization();

        group.MapPost(ApiResource.CreatePrivilegeRequest.Endpoint, async Task<Results<Created<Guid>, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.CreatePrivilegeRequest);

        group.MapGet(ApiResource.GetMyPrivileges.Endpoint, async Task<Ok<IReadOnlyList<UserEffectivePrivilegeDto>>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMyPrivilegesQuery(), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<UserEffectivePrivilegeDto>>(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetMyPrivileges);
    }
    /// <summary>
    /// Try resolve current user id.
    /// </summary>

    private static bool TryResolveCurrentUserId(HttpContext httpContext, out Guid userId)
    {
        return Guid.TryParse(httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out userId);
    }
    /// <summary>
    /// Escape.
    /// </summary>

    private static string Escape(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
