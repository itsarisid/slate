using Alphabet.Application.Common.Security;
using Alphabet.Application.Features.AssetManagement.Assignments.Commands;
using Alphabet.Application.Features.AssetManagement.Assignments.Queries;
using Alphabet.Application.Features.AssetManagement.Assets.Commands;
using Alphabet.Application.Features.AssetManagement.Assets.Queries;
using Alphabet.Application.Features.AssetManagement.Audit.Commands;
using Alphabet.Application.Features.AssetManagement.Audit.Queries;
using Alphabet.Application.Features.AssetManagement.Categories.Commands;
using Alphabet.Application.Features.AssetManagement.Categories.Queries;
using Alphabet.Application.Features.AssetManagement.Inventory.Commands;
using Alphabet.Application.Features.AssetManagement.Inventory.Queries;
using Alphabet.Application.Features.AssetManagement.Locations.Commands;
using Alphabet.Application.Features.AssetManagement.Maintenance.Commands;
using Alphabet.Application.Features.AssetManagement.Maintenance.Queries;
using Alphabet.Application.Features.AssetManagement.Reports.Queries;
using Alphabet.Application.Features.AssetManagement.Workflows.Commands;
using Alphabet.Application.Features.AssetManagement.Workflows.Queries;
using Alphabet.Modules.AssetManagementModule.Api.Hubs;
using Alphabet.Modules.AssetManagementModule.Api.Models;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Alphabet.Modules.AssetManagementModule.Api;

/// <summary>
/// Maps asset management module endpoints.
/// </summary>
public static class AssetManagementModuleEndpoints
{
    /// <summary>
    /// Registers all asset management endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapAssetManagementModule(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        MapAssets(endpoints, versionSet);
        MapCategories(endpoints, versionSet);
        MapLocations(endpoints, versionSet);
        MapWorkflows(endpoints, versionSet);
        MapInventory(endpoints, versionSet);
        MapReports(endpoints, versionSet);
        MapAdmin(endpoints, versionSet);
        MapSelfService(endpoints, versionSet);
        endpoints.MapHub<AssetManagementHub>("/hubs/assets")
            .RequireAuthorization()
            .WithTags("Asset Management Module");

        return endpoints;
    }

    private static void MapAssets(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/assets")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapPost("/", async Task<Results<Created<string>, BadRequest<ProblemDetails>>> (
            [FromBody] CreateAssetRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateAssetCommand(
                request.AssetTag,
                request.Name,
                request.Description,
                request.CategoryId,
                request.Subcategory,
                request.Manufacturer,
                request.Model,
                request.SerialNumber,
                request.PurchaseDate,
                request.WarrantyExpiry,
                request.Cost,
                request.Currency,
                request.Status,
                request.Condition,
                request.LocationId,
                request.SupplierId,
                request.CustomFields,
                request.Images,
                request.Documents), ct);

            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Asset creation failed", Detail = result.Error })
                : TypedResults.Created($"/api/v1/assets/{result.Value.Id}", result.Value.Message);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.create"))
        .Accepts<CreateAssetRequest>("application/json")
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CreateAsset")
        .WithSummary("Creates a new asset.")
        .WithDescription("Creates an asset master record, generates barcode/QR payloads, and records the operation in the asset activity log.");

        group.MapGet("/", async Task<IResult> (
            [AsParameters] GetAssetsRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetsQuery(
                request.Status,
                request.CategoryId,
                request.LocationId,
                request.AssignedToUserId,
                request.Search,
                request.PurchaseDateFrom,
                request.PurchaseDateTo,
                request.CostMin,
                request.CostMax,
                request.Condition,
                request.WarrantyExpiringInDays,
                request.IncludeRetired,
                request.SortBy,
                request.SortDirection,
                request.Page,
                request.PageSize), ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Asset list failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("GetAssets")
        .WithSummary("Gets a filtered and paginated asset list.")
        .WithDescription("Returns assets with advanced filtering for status, category, location, assignee, cost range, warranty expiry, and search text.");

        group.MapGet("/{assetId:guid}", async Task<IResult> (
            Guid assetId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetByIdQuery(assetId), ct);
            return result.IsFailure ? Results.NotFound(new ProblemDetails { Title = "Asset not found", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .WithName("GetAssetById")
        .WithSummary("Gets a single asset with full details.")
        .WithDescription("Returns the asset master record together with assignment history, maintenance history, and activity timeline.");

        group.MapPut("/{assetId:guid}", async Task<IResult> (
            Guid assetId,
            [FromBody] UpdateAssetRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateAssetCommand(
                assetId,
                request.Name,
                request.Description,
                request.CategoryId,
                request.Subcategory,
                request.Manufacturer,
                request.Model,
                request.SerialNumber,
                request.PurchaseDate,
                request.WarrantyExpiry,
                request.Cost,
                request.Currency,
                request.Condition,
                request.LocationId,
                request.SupplierId,
                request.CustomFields,
                request.Images,
                request.Documents), ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Asset update failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.update"))
        .Accepts<UpdateAssetRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("UpdateAsset")
        .WithSummary("Updates an existing asset.")
        .WithDescription("Updates asset descriptive information, ownership metadata, and custom fields while preserving history.");

        group.MapDelete("/{assetId:guid}", async Task<IResult> (
            Guid assetId,
            [FromBody] RetireAssetRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RetireAssetCommand(assetId, request.Status, request.Reason), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Asset retirement failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.delete"))
        .Accepts<RetireAssetRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("RetireAsset")
        .WithSummary("Soft deletes an asset by retiring or disposing it.")
        .WithDescription("Marks the asset as retired or disposed and records the change in the audit trail. Historical data remains intact.");

        group.MapPost("/{assetId:guid}/move", async Task<IResult> (
            Guid assetId,
            [FromBody] MoveAssetRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new MoveAssetCommand(assetId, request.NewLocationId, request.Reason), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Asset move failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.update"))
        .Accepts<MoveAssetRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("MoveAsset")
        .WithSummary("Moves an asset to a new location.")
        .WithDescription("Updates the asset location, records a movement history row, and writes an activity log entry with the move reason.");

        group.MapPost("/{assetId:guid}/assign", async Task<IResult> (
            Guid assetId,
            [FromBody] AssignAssetRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AssignAssetCommand(
                assetId,
                request.AssignedToUserId,
                request.ExpectedReturnDate,
                request.AssignmentType,
                request.Purpose,
                request.ConditionAtAssignment,
                request.Notes), ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Asset assignment failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.assign"))
        .Accepts<AssignAssetRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AssignAsset")
        .WithSummary("Assigns an asset to a user.")
        .WithDescription("Assigns an available asset, creates assignment history, updates the asset status, and sends assignment notifications.");

        group.MapPost("/{assetId:guid}/unassign", async Task<IResult> (
            Guid assetId,
            [FromBody] UnassignAssetRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UnassignAssetCommand(
                assetId,
                request.ReturnedByUserId,
                request.ReceivedByUserId,
                request.ConditionOnReturn,
                request.DamageNotes,
                request.MissingItems,
                request.ActualReturnDate), ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Asset return failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.unassign"))
        .Accepts<UnassignAssetRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("UnassignAsset")
        .WithSummary("Returns an assigned asset.")
        .WithDescription("Ends the active assignment, records return details and condition, and makes the asset available again.");

        group.MapPost("/{assetId:guid}/transfer", async Task<IResult> (
            Guid assetId,
            [FromBody] TransferAssetRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new TransferAssetCommand(
                assetId,
                request.FromUserId,
                request.ToUserId,
                request.Reason,
                request.TransferDate,
                request.ExpectedReturnDate), ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Asset transfer failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.transfer"))
        .Accepts<TransferAssetRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("TransferAsset")
        .WithSummary("Transfers an assignment between users.")
        .WithDescription("Closes the source assignment, creates a new assignment for the target user, and keeps a continuous audit trail.");

        group.MapGet("/{assetId:guid}/assignments", async Task<IResult> (
            Guid assetId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetAssignmentsQuery(assetId), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Assignment history failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .WithName("GetAssetAssignments")
        .WithSummary("Gets assignment history for an asset.")
        .WithDescription("Returns all historical and active assignments for the selected asset in reverse chronological order.");

        group.MapPost("/{assetId:guid}/maintenance", async Task<IResult> (
            Guid assetId,
            [FromBody] ScheduleMaintenanceRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ScheduleAssetMaintenanceCommand(
                assetId,
                request.MaintenanceType,
                request.ScheduledDate,
                request.Description,
                request.AssignedToVendor,
                request.EstimatedCost,
                request.Priority), ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Maintenance scheduling failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.maintenance"))
        .Accepts<ScheduleMaintenanceRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("ScheduleAssetMaintenance")
        .WithSummary("Schedules maintenance for an asset.")
        .WithDescription("Creates a maintenance record, marks the asset under repair, and stores the schedule for later reminders.");

        group.MapPost("/{assetId:guid}/maintenance/{maintenanceId:guid}/complete", async Task<IResult> (
            Guid assetId,
            Guid maintenanceId,
            [FromBody] CompleteMaintenanceRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CompleteAssetMaintenanceCommand(
                assetId,
                maintenanceId,
                request.CompletionDate,
                request.ActualCost,
                request.Notes,
                request.NextMaintenanceDueDate), ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Maintenance completion failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.maintenance"))
        .Accepts<CompleteMaintenanceRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("CompleteAssetMaintenance")
        .WithSummary("Completes a maintenance record.")
        .WithDescription("Records completion details, actual cost, next due date, and returns the asset to an available state.");

        group.MapGet("/{assetId:guid}/maintenance", async Task<IResult> (
            Guid assetId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetMaintenanceHistoryQuery(assetId), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Maintenance history failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .WithName("GetAssetMaintenanceHistory")
        .WithSummary("Gets maintenance history for an asset.")
        .WithDescription("Returns all scheduled and completed maintenance records for the selected asset.");

        group.MapGet("/{assetId:guid}/depreciation", async Task<IResult> (
            Guid assetId,
            [FromQuery] DateOnly? asOfDate,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetDepreciationQuery(assetId, asOfDate), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Depreciation calculation failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("GetAssetDepreciation")
        .WithSummary("Calculates depreciation for an asset.")
        .WithDescription("Returns current asset value, salvage value, accumulated depreciation, and year-to-date depreciation as of the requested date.");

        group.MapGet("/{assetId:guid}/activity", async Task<IResult> (
            Guid assetId,
            [FromQuery] string? action,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate,
            [FromQuery] Guid? userId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetActivityQuery(assetId, action, fromDate, toDate, userId), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Asset activity query failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.audit"))
        .Produces(StatusCodes.Status200OK)
        .WithName("GetAssetActivity")
        .WithSummary("Gets the activity timeline for an asset.")
        .WithDescription("Returns activity records for the selected asset with optional filtering by action, date range, and actor.");

        group.MapGet("/scan", async Task<IResult> (
            [FromQuery] string barcode,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ScanAssetQuery(barcode), ct);
            return result.IsFailure ? Results.NotFound(new ProblemDetails { Title = "Asset not found", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .WithName("ScanAsset")
        .WithSummary("Scans an asset by barcode or QR payload.")
        .WithDescription("Resolves an asset quickly from an asset tag, barcode payload, or QR payload and returns a concise quick-view response.");
    }

    private static void MapCategories(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/asset-categories")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateAssetCategoryRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateAssetCategoryCommand(
                request.Name,
                request.Description,
                request.ParentCategoryId,
                request.CustomFieldsSchema,
                request.DepreciationRate,
                request.DefaultLocationId), ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Category creation failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.create"))
        .Accepts<CreateAssetCategoryRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .WithName("CreateAssetCategory")
        .WithSummary("Creates a category in the asset hierarchy.")
        .WithDescription("Creates a category with optional parent, depreciation settings, and dynamic custom field schema metadata.");

        group.MapGet("/tree", async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetCategoryTreeQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Category tree failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .WithName("GetAssetCategoryTree")
        .WithSummary("Gets the asset category hierarchy.")
        .WithDescription("Returns categories as a recursive tree to drive category pickers and reporting rollups.");
    }

    private static void MapLocations(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/locations")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateLocationRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateLocationCommand(
                request.Name,
                request.Code,
                request.Type,
                request.Address,
                request.ParentLocationId,
                request.IsActive,
                request.Coordinates,
                request.ContactPerson,
                request.ContactPhone), ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Location creation failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.create"))
        .Accepts<CreateLocationRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .WithName("CreateAssetLocation")
        .WithSummary("Creates a location used by the asset module.")
        .WithDescription("Creates a physical or logical location with address, type, optional coordinates, and hierarchy information.");
    }

    private static void MapWorkflows(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/workflows")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateWorkflowDefinitionRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateAssetWorkflowDefinitionCommand(
                request.Name,
                request.Description,
                request.Version,
                request.Steps.Select(step => new AssetWorkflowDefinitionStepInput(
                    step.Name,
                    step.Order,
                    step.AssignedToRole,
                    step.RequiredApprovals,
                    step.TimeoutHours,
                    step.Actions)).ToArray()), ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Workflow definition creation failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("workflow.initiate"))
        .Accepts<CreateWorkflowDefinitionRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .WithName("CreateAssetWorkflowDefinition")
        .WithSummary("Creates an asset workflow definition.")
        .WithDescription("Creates a reusable workflow definition with ordered steps, role ownership, approval requirements, timeouts, and allowed actions.");

        group.MapPost("/assets/{assetId:guid}/start", async Task<IResult> (
            Guid assetId,
            [FromBody] StartWorkflowRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new StartAssetWorkflowCommand(assetId, request.WorkflowDefinitionId, request.Context), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Workflow start failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("workflow.initiate"))
        .Accepts<StartWorkflowRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .WithName("StartAssetWorkflow")
        .WithSummary("Starts a workflow for an asset.")
        .WithDescription("Creates a workflow instance for the selected asset and positions it on the first step.");

        group.MapGet("/instances/{instanceId:guid}", async Task<IResult> (
            Guid instanceId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetWorkflowInstanceQuery(instanceId), ct);
            return result.IsFailure ? Results.NotFound(new ProblemDetails { Title = "Workflow instance not found", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("workflow.approve"))
        .Produces(StatusCodes.Status200OK)
        .WithName("GetAssetWorkflowInstance")
        .WithSummary("Gets a workflow instance.")
        .WithDescription("Returns the current workflow state, active step, completion status, and step history for an asset workflow instance.");

        group.MapPost("/instances/{instanceId:guid}/steps/{stepId:guid}/action", async Task<IResult> (
            Guid instanceId,
            Guid stepId,
            [FromBody] WorkflowStepActionRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ActOnAssetWorkflowStepCommand(instanceId, stepId, request.Action, request.Comment), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Workflow action failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("workflow.approve"))
        .Accepts<WorkflowStepActionRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .WithName("ActOnAssetWorkflowStep")
        .WithSummary("Applies an action to the current workflow step.")
        .WithDescription("Completes or rejects the current workflow step and advances the instance when appropriate.");

        group.MapGet("/pending", async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPendingAssetWorkflowsQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Pending workflow query failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("workflow.approve"))
        .Produces(StatusCodes.Status200OK)
        .WithName("GetPendingAssetWorkflows")
        .WithSummary("Gets pending workflow work for the current user.")
        .WithDescription("Returns active workflow instances where the current user is the delegate or one of the allowed roles for the current step.");
    }

    private static void MapInventory(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/inventory")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapPost("/stock-adjustments", async Task<IResult> (
            [FromBody] StockAdjustmentRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AdjustInventoryStockCommand(
                request.AssetId,
                request.LocationId,
                request.AdjustmentType,
                request.Quantity,
                request.Reason,
                request.ReferenceNumber,
                request.MinimumThreshold), ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Stock adjustment failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.admin"))
        .Accepts<StockAdjustmentRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .WithName("AdjustAssetInventory")
        .WithSummary("Applies a stock adjustment.")
        .WithDescription("Adds, removes, or sets stock quantities for a stock-tracked asset and location, then records the adjustment and audit trail.");

        group.MapPost("/stock-take", async Task<IResult> (
            [FromBody] StockTakeRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new PerformAssetStockTakeCommand(
                    request.LocationId,
                    request.CountedItems.Select(x => new Alphabet.Domain.Models.StockTakeCountedItem(x.AssetId, x.CountedQuantity, x.ExpectedQuantity, x.Discrepancy)).ToArray()),
                ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Stock take failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.admin"))
        .Accepts<StockTakeRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .WithName("PerformAssetStockTake")
        .WithSummary("Performs a physical stock take.")
        .WithDescription("Records counted quantities for a location and updates the inventory balances for the counted assets.");

        group.MapGet("/low-stock", async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetLowStockAssetsQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Low stock query failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .WithName("GetLowStockAssets")
        .WithSummary("Gets low stock items.")
        .WithDescription("Returns stock balances that are at or below their configured minimum thresholds.");

        group.MapGet("/reports/current-stock", async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCurrentStockReportQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Current stock report failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .WithName("GetCurrentStockReport")
        .WithSummary("Gets the current stock report.")
        .WithDescription("Returns the current inventory balances for all tracked asset and location combinations.");
    }

    private static void MapReports(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/reports")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapGet("/utilization", async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetUtilizationReportQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Utilization report failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("report.generate"))
        .Produces(StatusCodes.Status200OK)
        .WithName("GetAssetUtilizationReport")
        .WithSummary("Gets asset utilization metrics.")
        .WithDescription("Returns assignment and idle metrics to help identify underused or highly utilized asset groups.");

        group.MapGet("/lifecycle", async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetLifecycleReportQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Lifecycle report failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("report.generate"))
        .Produces(StatusCodes.Status200OK)
        .WithName("GetAssetLifecycleReport")
        .WithSummary("Gets asset lifecycle metrics.")
        .WithDescription("Groups assets by age buckets and supports replacement planning and end-of-life analysis.");

        group.MapGet("/compliance", async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetComplianceReportQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Compliance report failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.audit"))
        .Produces(StatusCodes.Status200OK)
        .WithName("GetAssetComplianceReport")
        .WithSummary("Gets asset compliance metrics.")
        .WithDescription("Returns counts for unassigned assets, overdue returns, missing warranties, and maintenance items due soon.");
    }

    private static void MapAdmin(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/admin")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapGet("/activity", async Task<IResult> (
            [AsParameters] GetAdminActivityRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetGlobalAssetActivityQuery(
                request.Action,
                request.FromDate,
                request.ToDate,
                request.UserId,
                request.Take,
                request.Skip), ct);

            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Admin activity query failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.audit"))
        .Produces(StatusCodes.Status200OK)
        .WithName("GetGlobalAssetActivity")
        .WithSummary("Gets the global asset activity feed.")
        .WithDescription("Returns cross-system asset activity entries for audit, investigation, and operational reporting.");

        group.MapPost("/audit/generate", async Task<IResult> (
            [FromBody] GenerateAuditReportRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GenerateAssetAuditReportCommand(request.Start, request.End, request.Format, request.IncludeDeleted), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Audit report generation failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.audit"))
        .Accepts<GenerateAuditReportRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .WithName("GenerateAssetAuditReport")
        .WithSummary("Queues an audit report for generation.")
        .WithDescription("Queues a background job that gathers asset activity records for the requested date range and report format.");
    }

    private static void MapSelfService(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/users/me")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapGet("/assets", async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMyAssignedAssetsQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "My assets query failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithName("GetMyAssignedAssets")
        .WithSummary("Gets assets assigned to the current user.")
        .WithDescription("Returns the authenticated user's currently assigned assets for self-service visibility.");
    }
}

/// <summary>
/// Represents an asset list query from the API.
/// </summary>
public sealed class GetAssetsRequest
{
    [FromQuery(Name = "status")]
    public string? Status { get; init; }

    [FromQuery(Name = "categoryId")]
    public Guid? CategoryId { get; init; }

    [FromQuery(Name = "locationId")]
    public Guid? LocationId { get; init; }

    [FromQuery(Name = "assignedToUserId")]
    public Guid? AssignedToUserId { get; init; }

    [FromQuery(Name = "search")]
    public string? Search { get; init; }

    [FromQuery(Name = "purchaseDateFrom")]
    public DateOnly? PurchaseDateFrom { get; init; }

    [FromQuery(Name = "purchaseDateTo")]
    public DateOnly? PurchaseDateTo { get; init; }

    [FromQuery(Name = "costMin")]
    public decimal? CostMin { get; init; }

    [FromQuery(Name = "costMax")]
    public decimal? CostMax { get; init; }

    [FromQuery(Name = "condition")]
    public string? Condition { get; init; }

    [FromQuery(Name = "warrantyExpiringInDays")]
    public int? WarrantyExpiringInDays { get; init; }

    [FromQuery(Name = "includeRetired")]
    public bool IncludeRetired { get; init; }

    [FromQuery(Name = "sortBy")]
    public string? SortBy { get; init; }

    [FromQuery(Name = "sortDirection")]
    public string? SortDirection { get; init; }

    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// Represents an admin activity query request.
/// </summary>
public sealed class GetAdminActivityRequest
{
    [FromQuery(Name = "action")]
    public string? Action { get; init; }

    [FromQuery(Name = "fromDate")]
    public DateTimeOffset? FromDate { get; init; }

    [FromQuery(Name = "toDate")]
    public DateTimeOffset? ToDate { get; init; }

    [FromQuery(Name = "userId")]
    public Guid? UserId { get; init; }

    [FromQuery(Name = "take")]
    public int Take { get; init; } = 100;

    [FromQuery(Name = "skip")]
    public int Skip { get; init; }
}
