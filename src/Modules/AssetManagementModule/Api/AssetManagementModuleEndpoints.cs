using Alphabet.Modules.AssetManagementModule.Api.Resource;
using Alphabet.Common.Extensions;
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

        group.MapPost(ApiResource.CreateAsset.Endpoint, async Task<Results<Created<string>, BadRequest<ProblemDetails>>> (
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
        .WithDocumentation(ApiResource.CreateAsset);

        group.MapGet(ApiResource.GetAssets.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetAssets);

        group.MapGet(ApiResource.GetAssetById.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetAssetById);

        group.MapPut(ApiResource.UpdateAsset.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.UpdateAsset);

        group.MapDelete(ApiResource.RetireAsset.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.RetireAsset);

        group.MapPost(ApiResource.MoveAsset.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.MoveAsset);

        group.MapPost(ApiResource.AssignAsset.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.AssignAsset);

        group.MapPost(ApiResource.UnassignAsset.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.UnassignAsset);

        group.MapPost(ApiResource.TransferAsset.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.TransferAsset);

        group.MapGet(ApiResource.GetAssetAssignments.Endpoint, async Task<IResult> (
            Guid assetId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetAssignmentsQuery(assetId), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Assignment history failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetAssetAssignments);

        group.MapPost(ApiResource.ScheduleAssetMaintenance.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.ScheduleAssetMaintenance);

        group.MapPost(ApiResource.CompleteAssetMaintenance.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CompleteAssetMaintenance);

        group.MapGet(ApiResource.GetAssetMaintenanceHistory.Endpoint, async Task<IResult> (
            Guid assetId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetMaintenanceHistoryQuery(assetId), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Maintenance history failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetAssetMaintenanceHistory);

        group.MapGet(ApiResource.GetAssetDepreciation.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetAssetDepreciation);

        group.MapGet(ApiResource.GetAssetActivity.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetAssetActivity);

        group.MapGet(ApiResource.ScanAsset.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.ScanAsset);
    }

    private static void MapCategories(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/asset-categories")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapPost(ApiResource.CreateAssetCategory.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CreateAssetCategory);

        group.MapGet(ApiResource.GetAssetCategoryTree.Endpoint, async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetCategoryTreeQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Category tree failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetAssetCategoryTree);
    }

    private static void MapLocations(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/locations")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapPost(ApiResource.CreateAssetLocation.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CreateAssetLocation);
    }

    private static void MapWorkflows(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/workflows")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapPost(ApiResource.CreateAssetWorkflowDefinition.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.CreateAssetWorkflowDefinition);

        group.MapPost(ApiResource.StartAssetWorkflow.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.StartAssetWorkflow);

        group.MapGet(ApiResource.GetAssetWorkflowInstance.Endpoint, async Task<IResult> (
            Guid instanceId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetWorkflowInstanceQuery(instanceId), ct);
            return result.IsFailure ? Results.NotFound(new ProblemDetails { Title = "Workflow instance not found", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("workflow.approve"))
        .Produces(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetAssetWorkflowInstance);

        group.MapPost(ApiResource.ActOnAssetWorkflowStep.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.ActOnAssetWorkflowStep);

        group.MapGet(ApiResource.GetPendingAssetWorkflows.Endpoint, async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPendingAssetWorkflowsQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Pending workflow query failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("workflow.approve"))
        .Produces(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetPendingAssetWorkflows);
    }

    private static void MapInventory(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/inventory")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapPost(ApiResource.AdjustAssetInventory.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.AdjustAssetInventory);

        group.MapPost(ApiResource.PerformAssetStockTake.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.PerformAssetStockTake);

        group.MapGet(ApiResource.GetLowStockAssets.Endpoint, async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetLowStockAssetsQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Low stock query failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetLowStockAssets);

        group.MapGet(ApiResource.GetCurrentStockReport.Endpoint, async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCurrentStockReportQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Current stock report failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.view"))
        .Produces(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetCurrentStockReport);
    }

    private static void MapReports(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/reports")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapGet(ApiResource.GetAssetUtilizationReport.Endpoint, async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetUtilizationReportQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Utilization report failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("report.generate"))
        .Produces(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetAssetUtilizationReport);

        group.MapGet(ApiResource.GetAssetLifecycleReport.Endpoint, async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetLifecycleReportQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Lifecycle report failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("report.generate"))
        .Produces(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetAssetLifecycleReport);

        group.MapGet(ApiResource.GetAssetComplianceReport.Endpoint, async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAssetComplianceReportQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "Compliance report failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithMetadata(new PrivilegeAuthorizeAttribute("asset.audit"))
        .Produces(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.GetAssetComplianceReport);
    }

    private static void MapAdmin(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/admin")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapGet(ApiResource.GetGlobalAssetActivity.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GetGlobalAssetActivity);

        group.MapPost(ApiResource.GenerateAssetAuditReport.Endpoint, async Task<IResult> (
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
        .WithDocumentation(ApiResource.GenerateAssetAuditReport);
    }

    private static void MapSelfService(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/users/me")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Asset Management Module")
            .RequireAuthorization();

        group.MapGet(ApiResource.GetMyAssignedAssets.Endpoint, async Task<IResult> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMyAssignedAssetsQuery(), ct);
            return result.IsFailure ? Results.BadRequest(new ProblemDetails { Title = "My assets query failed", Detail = result.Error }) : Results.Ok(result.Value);
        })
        .WithDocumentation(ApiResource.GetMyAssignedAssets);
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
