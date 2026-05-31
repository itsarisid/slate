using Alphabet.Application.Common.Security;
using Alphabet.Application.Features.LeaveManagement.Commands;
using Alphabet.Application.Features.LeaveManagement.Queries;
using Alphabet.Modules.LeaveManagementModule.Api.Hubs;
using Alphabet.Modules.LeaveManagementModule.Api.Models;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Alphabet.Modules.LeaveManagementModule.Api;

/// <summary>
/// Maps leave management endpoints.
/// </summary>
public static class LeaveManagementModuleEndpoints
{
    /// <summary>
    /// Registers leave management HTTP endpoints and the SignalR hub.
    /// </summary>
    public static IEndpointRouteBuilder MapLeaveManagementModule(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        MapLeaveTypes(endpoints, versionSet);
        MapRequests(endpoints, versionSet);
        MapApprovals(endpoints, versionSet);
        MapBalances(endpoints, versionSet);
        MapDelegations(endpoints, versionSet);
        MapCalendar(endpoints, versionSet);
        MapAdmin(endpoints, versionSet);
        endpoints.MapHub<LeaveManagementHub>("/hubs/leave-management").RequireAuthorization();
        return endpoints;
    }

    private static void MapLeaveTypes(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/leave/types")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Leave Management Module - Leave Types")
            .RequireAuthorization();

        group.MapGet("/", async Task<IResult> (
            [FromQuery] bool? isActive,
            [FromQuery] bool? isPaid,
            [FromQuery] string? applicableToRole,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetLeaveTypesQuery(isActive, isPaid, applicableToRole), ct);
            return ToOkOrProblem(result, "Leave type query failed");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetLeaveTypes")
        .WithSummary("Gets configured leave types.")
        .WithDescription("Returns leave policies such as annual, sick, and unpaid leave with entitlement and eligibility settings.");

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateLeaveTypeRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateLeaveTypeCommand(
                request.Name,
                request.Code,
                request.Description,
                request.Color,
                request.Icon,
                request.IsPaid,
                request.DefaultDaysPerYear,
                request.MaxConsecutiveDays,
                request.MinDaysPerRequest,
                request.MaxDaysPerRequest,
                request.RequiresApproval,
                request.ApprovalChainId,
                request.CarryForwardEnabled,
                request.MaxCarryForwardDays,
                request.CarryForwardExpiryMonths,
                request.EncashmentEnabled,
                request.EncashmentRate,
                request.ProrationEnabled,
                request.EligibilityRules,
                request.BlackoutDates,
                request.RequiresAttachment,
                request.AllowedAttachmentTypes,
                request.AutoApproveRules,
                request.IsActive), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Leave type creation failed", result.Error))
                : TypedResults.Created($"/api/v1/leave/types/{result.Value.Id}", result.Value);
        })
        .Accepts<CreateLeaveTypeRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateLeaveType")
        .WithSummary("Creates a leave type.")
        .WithDescription("Creates a configurable leave policy including entitlement, attachment, carry-forward, eligibility, and auto-approval rules.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.type.manage"));

        group.MapPut("/{leaveTypeId:guid}", async Task<IResult> (
            Guid leaveTypeId,
            [FromBody] UpdateLeaveTypeRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateLeaveTypeCommand(
                leaveTypeId,
                request.Name,
                request.Description,
                request.Color,
                request.Icon,
                request.IsPaid,
                request.DefaultDaysPerYear,
                request.MaxConsecutiveDays,
                request.MinDaysPerRequest,
                request.MaxDaysPerRequest,
                request.RequiresApproval,
                request.ApprovalChainId,
                request.CarryForwardEnabled,
                request.MaxCarryForwardDays,
                request.CarryForwardExpiryMonths,
                request.EncashmentEnabled,
                request.EncashmentRate,
                request.ProrationEnabled,
                request.EligibilityRules,
                request.BlackoutDates,
                request.RequiresAttachment,
                request.AllowedAttachmentTypes,
                request.AutoApproveRules,
                request.IsActive), ct);
            return ToOkOrProblem(result, "Leave type update failed");
        })
        .Accepts<UpdateLeaveTypeRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("UpdateLeaveType")
        .WithSummary("Updates a leave type.")
        .WithDescription("Updates leave policy settings without changing existing request history.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.type.manage"));

        group.MapDelete("/{leaveTypeId:guid}", async Task<IResult> (
            Guid leaveTypeId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteLeaveTypeCommand(leaveTypeId), ct);
            return ToOkOrProblem(result, "Leave type deactivation failed");
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("DeactivateLeaveType")
        .WithSummary("Deactivates a leave type.")
        .WithDescription("Soft-deactivates a leave type so existing history remains intact while new requests are prevented.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.type.manage"));
    }

    private static void MapRequests(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/leave/requests")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Leave Management Module - Requests")
            .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            [FromBody] SubmitLeaveRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new SubmitLeaveRequestCommand(request.LeaveTypeId, request.StartDate, request.EndDate, request.PartialDays, request.Reason, request.AttachmentIds, request.ContactNumber, request.AlternateArrangements, request.ApplyToAllDays, request.IsHalfDay), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Leave request submission failed", result.Error))
                : TypedResults.Created($"/api/v1/leave/requests/{result.Value.Id}", result.Value);
        })
        .Accepts<SubmitLeaveRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("SubmitLeaveRequest")
        .WithSummary("Submits a leave request.")
        .WithDescription("Validates entitlement, blackout dates, overlap rules, and starts the configured approval workflow.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.request.create"));

        group.MapGet("/", async Task<IResult> (
            [FromQuery] Guid? userId,
            [FromQuery] string? status,
            [FromQuery] string? fromDate,
            [FromQuery] string? toDate,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var parsedStatus = Enum.TryParse<Alphabet.Domain.Enums.LeaveRequestStatus>(status, true, out var s) ? s : (Alphabet.Domain.Enums.LeaveRequestStatus?)null;
            var result = await sender.Send(new SearchLeaveRequestsQuery(userId, parsedStatus, ParseDate(fromDate), ParseDate(toDate), page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize), ct);
            return ToOkOrProblem(result, "Leave request query failed");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("SearchLeaveRequests")
        .WithSummary("Searches leave requests.")
        .WithDescription("Returns paged leave requests filtered by user, status, and date range. Without userId it returns the current user's requests.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.request.view"));

        group.MapGet("/{requestId:guid}", async Task<IResult> (
            Guid requestId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetLeaveRequestByIdQuery(requestId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.NotFound(CreateProblem("Leave request not found", result.Error))
                : TypedResults.Ok(result.Value);
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .WithName("GetLeaveRequestById")
        .WithSummary("Gets a leave request.")
        .WithDescription("Returns a leave request with current status and approval level.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.request.view"));

        group.MapPut("/{requestId:guid}", async Task<IResult> (
            Guid requestId,
            [FromBody] ModifyLeaveRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ModifyLeaveRequestCommand(requestId, request.StartDate, request.EndDate, request.PartialDays, request.Reason, request.AttachmentIds, request.ContactNumber, request.AlternateArrangements), ct);
            return ToOkOrProblem(result, "Leave request update failed");
        })
        .Accepts<ModifyLeaveRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("ModifyLeaveRequest")
        .WithSummary("Modifies a leave request.")
        .WithDescription("Updates a pending or change-requested leave request and recalculates requested days.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.request.modify"));

        group.MapPost("/{requestId:guid}/cancel", async Task<IResult> (
            Guid requestId,
            [FromBody] CancelLeaveRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CancelLeaveRequestCommand(requestId, request.Reason), ct);
            return ToOkOrProblem(result, "Leave request cancellation failed");
        })
        .Accepts<CancelLeaveRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CancelLeaveRequest")
        .WithSummary("Cancels a leave request.")
        .WithDescription("Cancels an active request and releases pending balance reservations when applicable.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.request.cancel"));

        group.MapPost("/{requestId:guid}/withdraw", async Task<IResult> (
            Guid requestId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new WithdrawLeaveRequestCommand(requestId), ct);
            return ToOkOrProblem(result, "Leave request withdrawal failed");
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("WithdrawLeaveRequest")
        .WithSummary("Withdraws a pending leave request.")
        .WithDescription("Removes a pending request from the active approval queue.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.request.cancel"));
    }

    private static void MapApprovals(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/leave/approvals")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Leave Management Module - Approvals")
            .RequireAuthorization();

        group.MapGet("/pending", async Task<IResult> ([FromServices] ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPendingLeaveApprovalsQuery(), ct);
            return ToOkOrProblem(result, "Pending approvals query failed");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetPendingLeaveApprovals")
        .WithSummary("Gets pending approvals.")
        .WithDescription("Returns leave approval tasks assigned directly to the current user or to one of their roles.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.approve.level1,leave.approve.level2,leave.approve.levelN"));

        group.MapPost("/{requestId:guid}/approve", async Task<IResult> (
            Guid requestId,
            [FromBody] ApprovalDecisionRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ApproveLeaveRequestCommand(requestId, request.Comment, request.Attachments, request.Level, request.NotifyApplicant, request.ApplyPartialApproval, request.ApprovedDays), ct);
            return ToOkOrProblem(result, "Leave approval failed");
        })
        .Accepts<ApprovalDecisionRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("ApproveLeaveRequest")
        .WithSummary("Approves a leave request.")
        .WithDescription("Records approval for the current workflow level and advances to the next level or completes the request.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.approve.level1,leave.approve.level2,leave.approve.levelN"));

        group.MapPost("/{requestId:guid}/reject", async Task<IResult> (
            Guid requestId,
            [FromBody] RejectLeaveRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RejectLeaveRequestCommand(requestId, request.Reason, request.SuggestedDates), ct);
            return ToOkOrProblem(result, "Leave rejection failed");
        })
        .Accepts<RejectLeaveRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("RejectLeaveRequest")
        .WithSummary("Rejects a leave request.")
        .WithDescription("Rejects a request, releases reserved balance, and optionally returns suggested date ranges.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.approve.level1,leave.approve.level2,leave.approve.levelN"));

        group.MapPost("/{requestId:guid}/changes", async Task<IResult> (
            Guid requestId,
            [FromBody] RequestChangesRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RequestLeaveChangesCommand(requestId, request.Comment), ct);
            return ToOkOrProblem(result, "Request changes failed");
        })
        .Accepts<RequestChangesRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("RequestLeaveChanges")
        .WithSummary("Requests leave request changes.")
        .WithDescription("Returns the request to the employee with a comment so it can be corrected and resubmitted.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.approve.level1,leave.approve.level2,leave.approve.levelN"));

        group.MapPost("/batch", async Task<IResult> (
            [FromBody] BatchApprovalRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new BatchLeaveApprovalCommand(request.RequestIds, request.Action, request.Comment), ct);
            return ToOkOrProblem(result, "Batch approval failed");
        })
        .Accepts<BatchApprovalRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("BatchLeaveApproval")
        .WithSummary("Processes approvals in batch.")
        .WithDescription("Applies an approve or reject action to multiple leave requests in a single operation.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.approve.batch"));

        group.MapGet("/{requestId:guid}/history", async Task<IResult> (
            Guid requestId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetLeaveApprovalHistoryQuery(requestId), ct);
            return ToOkOrProblem(result, "Approval history query failed");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetLeaveApprovalHistory")
        .WithSummary("Gets approval history.")
        .WithDescription("Returns all workflow steps, approver assignments, decisions, comments, and timestamps for a request.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.request.view,leave.audit.view"));
    }

    private static void MapBalances(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/leave/balances")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Leave Management Module - Balances")
            .RequireAuthorization();

        group.MapGet("/me", async Task<IResult> ([FromQuery] int year, [FromServices] ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMyLeaveBalancesQuery(year <= 0 ? DateTime.UtcNow.Year : year), ct);
            return ToOkOrProblem(result, "Leave balance query failed");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetMyLeaveBalances")
        .WithSummary("Gets my leave balances.")
        .WithDescription("Returns annual balance totals for the current user grouped by leave type.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.balance.view"));

        group.MapGet("/users/{userId:guid}", async Task<IResult> (Guid userId, [FromQuery] int year, [FromServices] ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUserLeaveBalancesQuery(userId, year <= 0 ? DateTime.UtcNow.Year : year), ct);
            return ToOkOrProblem(result, "User leave balance query failed");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetUserLeaveBalances")
        .WithSummary("Gets another user's leave balances.")
        .WithDescription("Returns annual balance totals for a selected user. Intended for HR, managers, and admins.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.balance.view.any"));

        group.MapPost("/initialize", async Task<IResult> (
            [FromBody] InitializeLeaveBalanceRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var items = request.Balances.Select(x => new InitializeLeaveBalanceItem(x.LeaveTypeId, x.Allocated, x.Remaining, x.CarryForward)).ToArray();
            var result = await sender.Send(new InitializeLeaveBalanceCommand(request.UserId, request.Year, items), ct);
            return ToOkOrProblem(result, "Leave balance initialization failed");
        })
        .Accepts<InitializeLeaveBalanceRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("InitializeLeaveBalances")
        .WithSummary("Initializes leave balances.")
        .WithDescription("Creates annual balance rows for a user and year without duplicating existing balances.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.balance.adjust"));

        group.MapPost("/adjust", async Task<IResult> (
            [FromBody] AdjustLeaveBalanceRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AdjustLeaveBalanceCommand(request.UserId, request.LeaveTypeId, request.Year, request.Adjustment, request.Reason), ct);
            return ToOkOrProblem(result, "Leave balance adjustment failed");
        })
        .Accepts<AdjustLeaveBalanceRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AdjustLeaveBalance")
        .WithSummary("Adjusts a leave balance.")
        .WithDescription("Applies a positive or negative manual balance adjustment and writes an audit entry.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.balance.adjust"));
    }

    private static void MapDelegations(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/leave/delegations")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Leave Management Module - Delegations")
            .RequireAuthorization();

        group.MapGet("/users/{userId:guid}", async Task<IResult> (Guid userId, [FromServices] ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetLeaveDelegationsQuery(userId), ct);
            return ToOkOrProblem(result, "Delegation query failed");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetLeaveDelegations")
        .WithSummary("Gets leave delegations.")
        .WithDescription("Returns delegation records where the user is either delegator or delegate.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.delegate.create,leave.delegate.revoke"));

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateDelegationRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateDelegationCommand(request.DelegatorUserId, request.DelegateToUserId, request.DelegationType, request.Permission, request.ApplicableLeaveTypes, request.ApplicableApprovalLevels, request.ApplicableDepartments, request.ApplicableEmployees, request.StartDate, request.EndDate, request.Reason, request.IsActive), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Delegation creation failed", result.Error))
                : TypedResults.Created($"/api/v1/leave/delegations/{result.Value.Id}", result.Value);
        })
        .Accepts<CreateDelegationRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateLeaveDelegation")
        .WithSummary("Creates approval delegation.")
        .WithDescription("Delegates leave approval authority to another user for selected leave types, approval levels, dates, or employee scopes.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.delegate.create"));

        group.MapPost("/{delegationId:guid}/revoke", async Task<IResult> (
            Guid delegationId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RevokeDelegationCommand(delegationId), ct);
            return ToOkOrProblem(result, "Delegation revocation failed");
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("RevokeLeaveDelegation")
        .WithSummary("Revokes approval delegation.")
        .WithDescription("Deactivates a delegation and prevents future approval routing through it.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.delegate.revoke"));
    }

    private static void MapCalendar(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/leave/calendar")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Leave Management Module - Calendar")
            .RequireAuthorization();

        group.MapGet("/", async Task<IResult> (
            [FromQuery] string? fromDate,
            [FromQuery] string? toDate,
            [FromQuery] Guid? userId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var from = ParseDate(fromDate) ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var to = ParseDate(toDate) ?? from.AddMonths(1);
            var result = await sender.Send(new GetLeaveCalendarQuery(from, to, userId), ct);
            return ToOkOrProblem(result, "Leave calendar query failed");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetLeaveCalendar")
        .WithSummary("Gets leave calendar data.")
        .WithDescription("Returns approved leave grouped by day so clients can render team availability and coverage issues.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.request.view"));

        group.MapGet("/holidays", async Task<IResult> (
            [FromQuery] string? fromDate,
            [FromQuery] string? toDate,
            [FromQuery] string? country,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var from = ParseDate(fromDate) ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var to = ParseDate(toDate) ?? from.AddYears(1);
            var result = await sender.Send(new GetPublicHolidaysQuery(from, to, country), ct);
            return ToOkOrProblem(result, "Holiday query failed");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetPublicHolidays")
        .WithSummary("Gets public holidays.")
        .WithDescription("Returns configured public holidays that can be excluded from leave-day calculations.");
    }

    private static void MapAdmin(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/leave/admin")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Leave Management Module - Administration")
            .RequireAuthorization();

        group.MapGet("/approval-chains", async Task<IResult> ([FromServices] ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetApprovalChainsQuery(), ct);
            return ToOkOrProblem(result, "Approval chain query failed");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetLeaveApprovalChains")
        .WithSummary("Gets approval chains.")
        .WithDescription("Returns configured N-level leave approval chains and their routing rules.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.chain.manage"));

        group.MapPost("/approval-chains", async Task<IResult> (
            [FromBody] CreateApprovalChainRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateApprovalChainCommand(request.Name, request.Code, request.Description, request.ApplicableTo, request.ApprovalLevels, request.FinalApprovalLevel, request.AllowSkipLevels, request.ParallelApproval, request.IsActive), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Approval chain creation failed", result.Error))
                : TypedResults.Created($"/api/v1/leave/admin/approval-chains/{result.Value.Id}", result.Value);
        })
        .Accepts<CreateApprovalChainRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateLeaveApprovalChain")
        .WithSummary("Creates an approval chain.")
        .WithDescription("Creates an N-level workflow definition with role, user, HR, manager, delegation, timeout, and escalation settings.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.chain.manage"));

        group.MapPost("/holidays", async Task<IResult> (
            [FromBody] CreatePublicHolidayRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreatePublicHolidayCommand(request.Name, request.Date, request.Country, request.State, request.IsPaid, request.Recurring), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(CreateProblem("Holiday creation failed", result.Error))
                : TypedResults.Created($"/api/v1/leave/calendar/holidays/{result.Value.Id}", result.Value);
        })
        .Accepts<CreatePublicHolidayRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreatePublicHoliday")
        .WithSummary("Creates a public holiday.")
        .WithDescription("Adds a paid or unpaid public holiday used by leave calculations.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.admin"));

        group.MapPost("/blackout-periods", async Task<IResult> (
            [FromBody] CreateBlackoutPeriodRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateBlackoutPeriodCommand(request.StartDate, request.EndDate, request.Reason, request.ApplicableTo), ct);
            return ToOkOrProblem(result, "Blackout period creation failed");
        })
        .Accepts<CreateBlackoutPeriodRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateLeaveBlackoutPeriod")
        .WithSummary("Creates a blackout period.")
        .WithDescription("Blocks or flags leave requests for critical periods such as payroll close, audits, or high-demand operations.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.admin"));

        group.MapPost("/accrual-rules", async Task<IResult> (
            [FromBody] CreateAccrualRuleRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateAccrualRuleCommand(request.LeaveTypeId, request.AccrualMethod, request.AccrualRate, request.MaxAccrual, request.TenureRulesJson), ct);
            return ToOkOrProblem(result, "Accrual rule creation failed");
        })
        .Accepts<CreateAccrualRuleRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateLeaveAccrualRule")
        .WithSummary("Creates an accrual rule.")
        .WithDescription("Configures monthly, daily, yearly, or tenure-based accrual rules for a leave type.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.admin"));

        group.MapPost("/accrual/run", async Task<IResult> (
            [FromQuery] int year,
            [FromQuery] string? reason,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AccrueLeaveBalancesCommand(year <= 0 ? DateTime.UtcNow.Year : year, reason ?? "Manual accrual run."), ct);
            return ToOkOrProblem(result, "Accrual run failed");
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("RunLeaveAccrual")
        .WithSummary("Runs leave accrual.")
        .WithDescription("Manually triggers accrual processing for configured leave rules.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.admin"));

        group.MapGet("/audit", async Task<IResult> (
            [FromQuery] Guid? userId,
            [FromQuery] Guid? requestId,
            [FromQuery] string? fromDate,
            [FromQuery] string? toDate,
            [FromQuery] int take,
            [FromQuery] int skip,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetLeaveActivityQuery(userId, requestId, ParseDateTime(fromDate), ParseDateTime(toDate), take <= 0 ? 100 : take, skip), ct);
            return ToOkOrProblem(result, "Leave audit query failed");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetLeaveAuditLogs")
        .WithSummary("Gets leave audit logs.")
        .WithDescription("Returns immutable audit entries for leave requests, balances, delegations, and administrative changes.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.audit.view"));

        group.MapGet("/reports/summary", async Task<IResult> (
            [FromQuery] string? fromDate,
            [FromQuery] string? toDate,
            [FromQuery] Guid? userId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var from = ParseDate(fromDate) ?? DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(-1));
            var to = ParseDate(toDate) ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var result = await sender.Send(new GetLeaveReportSummaryQuery(from, to, userId), ct);
            return ToOkOrProblem(result, "Leave report query failed");
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("GetLeaveReportSummary")
        .WithSummary("Gets leave report summary.")
        .WithDescription("Returns request counts and approved-day totals for reporting dashboards.")
        .RequireAuthorization(new PrivilegeAuthorizeAttribute("leave.report.view"));
    }

    private static IResult ToOkOrProblem<T>(Alphabet.Application.Results.Result<T> result, string title)
    {
        return result.IsFailure || result.Value is null
            ? TypedResults.BadRequest(CreateProblem(title, result.Error))
            : TypedResults.Ok(result.Value);
    }

    private static ProblemDetails CreateProblem(string title, string? detail)
        => new() { Title = title, Detail = detail };

    private static DateOnly? ParseDate(string? value)
        => DateOnly.TryParse(value, out var date) ? date : null;

    private static DateTimeOffset? ParseDateTime(string? value)
        => DateTimeOffset.TryParse(value, out var date) ? date : null;
}
