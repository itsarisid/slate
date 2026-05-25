using Alphabet.Common.Models;

namespace Alphabet.Modules.AssetManagementModule.Api.Resource;

public static class ApiResource
{
    // ── MapAssets ───────────────────────────────────────────────────────

    public static EndpointDetails CreateAsset => new()
    {
        Endpoint = "/",
        Name = "CreateAsset",
        Summary = "Creates a new asset.",
        Description = "Creates an asset master record, generates barcode/QR payloads, and records the operation in the asset activity log."
    };

    public static EndpointDetails GetAssets => new()
    {
        Endpoint = "/",
        Name = "GetAssets",
        Summary = "Gets a filtered and paginated asset list.",
        Description = "Returns assets with advanced filtering for status, category, location, assignee, cost range, warranty expiry, and search text."
    };

    public static EndpointDetails GetAssetById => new()
    {
        Endpoint = "/{assetId:guid}",
        Name = "GetAssetById",
        Summary = "Gets a single asset with full details.",
        Description = "Returns the asset master record together with assignment history, maintenance history, and activity timeline."
    };

    public static EndpointDetails UpdateAsset => new()
    {
        Endpoint = "/{assetId:guid}",
        Name = "UpdateAsset",
        Summary = "Updates an existing asset.",
        Description = "Updates asset descriptive information, ownership metadata, and custom fields while preserving history."
    };

    public static EndpointDetails RetireAsset => new()
    {
        Endpoint = "/{assetId:guid}",
        Name = "RetireAsset",
        Summary = "Soft deletes an asset by retiring or disposing it.",
        Description = "Marks the asset as retired or disposed and records the change in the audit trail. Historical data remains intact."
    };

    public static EndpointDetails MoveAsset => new()
    {
        Endpoint = "/{assetId:guid}/move",
        Name = "MoveAsset",
        Summary = "Moves an asset to a new location.",
        Description = "Updates the asset location, records a movement history row, and writes an activity log entry with the move reason."
    };

    public static EndpointDetails AssignAsset => new()
    {
        Endpoint = "/{assetId:guid}/assign",
        Name = "AssignAsset",
        Summary = "Assigns an asset to a user.",
        Description = "Assigns an available asset, creates assignment history, updates the asset status, and sends assignment notifications."
    };

    public static EndpointDetails UnassignAsset => new()
    {
        Endpoint = "/{assetId:guid}/unassign",
        Name = "UnassignAsset",
        Summary = "Returns an assigned asset.",
        Description = "Ends the active assignment, records return details and condition, and makes the asset available again."
    };

    public static EndpointDetails TransferAsset => new()
    {
        Endpoint = "/{assetId:guid}/transfer",
        Name = "TransferAsset",
        Summary = "Transfers an assignment between users.",
        Description = "Closes the source assignment, creates a new assignment for the target user, and keeps a continuous audit trail."
    };

    public static EndpointDetails GetAssetAssignments => new()
    {
        Endpoint = "/{assetId:guid}/assignments",
        Name = "GetAssetAssignments",
        Summary = "Gets assignment history for an asset.",
        Description = "Returns all historical and active assignments for the selected asset in reverse chronological order."
    };

    public static EndpointDetails ScheduleAssetMaintenance => new()
    {
        Endpoint = "/{assetId:guid}/maintenance",
        Name = "ScheduleAssetMaintenance",
        Summary = "Schedules maintenance for an asset.",
        Description = "Creates a maintenance record, marks the asset under repair, and stores the schedule for later reminders."
    };

    public static EndpointDetails CompleteAssetMaintenance => new()
    {
        Endpoint = "/{assetId:guid}/maintenance/{maintenanceId:guid}/complete",
        Name = "CompleteAssetMaintenance",
        Summary = "Completes a maintenance record.",
        Description = "Records completion details, actual cost, next due date, and returns the asset to an available state."
    };

    public static EndpointDetails GetAssetMaintenanceHistory => new()
    {
        Endpoint = "/{assetId:guid}/maintenance",
        Name = "GetAssetMaintenanceHistory",
        Summary = "Gets maintenance history for an asset.",
        Description = "Returns all scheduled and completed maintenance records for the selected asset."
    };

    public static EndpointDetails GetAssetDepreciation => new()
    {
        Endpoint = "/{assetId:guid}/depreciation",
        Name = "GetAssetDepreciation",
        Summary = "Calculates depreciation for an asset.",
        Description = "Returns current asset value, salvage value, accumulated depreciation, and year-to-date depreciation as of the requested date."
    };

    public static EndpointDetails GetAssetActivity => new()
    {
        Endpoint = "/{assetId:guid}/activity",
        Name = "GetAssetActivity",
        Summary = "Gets the activity timeline for an asset.",
        Description = "Returns activity records for the selected asset with optional filtering by action, date range, and actor."
    };

    public static EndpointDetails ScanAsset => new()
    {
        Endpoint = "/scan",
        Name = "ScanAsset",
        Summary = "Scans an asset by barcode or QR payload.",
        Description = "Resolves an asset quickly from an asset tag, barcode payload, or QR payload and returns a concise quick-view response."
    };

    // ── MapCategories ──────────────────────────────────────────────────

    public static EndpointDetails CreateAssetCategory => new()
    {
        Endpoint = "/",
        Name = "CreateAssetCategory",
        Summary = "Creates a category in the asset hierarchy.",
        Description = "Creates a category with optional parent, depreciation settings, and dynamic custom field schema metadata."
    };

    public static EndpointDetails GetAssetCategoryTree => new()
    {
        Endpoint = "/tree",
        Name = "GetAssetCategoryTree",
        Summary = "Gets the asset category hierarchy.",
        Description = "Returns categories as a recursive tree to drive category pickers and reporting rollups."
    };

    // ── MapLocations ───────────────────────────────────────────────────

    public static EndpointDetails CreateAssetLocation => new()
    {
        Endpoint = "/",
        Name = "CreateAssetLocation",
        Summary = "Creates a location used by the asset module.",
        Description = "Creates a physical or logical location with address, type, optional coordinates, and hierarchy information."
    };

    // ── MapWorkflows ───────────────────────────────────────────────────

    public static EndpointDetails CreateAssetWorkflowDefinition => new()
    {
        Endpoint = "/",
        Name = "CreateAssetWorkflowDefinition",
        Summary = "Creates an asset workflow definition.",
        Description = "Creates a reusable workflow definition with ordered steps, role ownership, approval requirements, timeouts, and allowed actions."
    };

    public static EndpointDetails StartAssetWorkflow => new()
    {
        Endpoint = "/assets/{assetId:guid}/start",
        Name = "StartAssetWorkflow",
        Summary = "Starts a workflow for an asset.",
        Description = "Creates a workflow instance for the selected asset and positions it on the first step."
    };

    public static EndpointDetails GetAssetWorkflowInstance => new()
    {
        Endpoint = "/instances/{instanceId:guid}",
        Name = "GetAssetWorkflowInstance",
        Summary = "Gets a workflow instance.",
        Description = "Returns the current workflow state, active step, completion status, and step history for an asset workflow instance."
    };

    public static EndpointDetails ActOnAssetWorkflowStep => new()
    {
        Endpoint = "/instances/{instanceId:guid}/steps/{stepId:guid}/action",
        Name = "ActOnAssetWorkflowStep",
        Summary = "Applies an action to the current workflow step.",
        Description = "Completes or rejects the current workflow step and advances the instance when appropriate."
    };

    public static EndpointDetails GetPendingAssetWorkflows => new()
    {
        Endpoint = "/pending",
        Name = "GetPendingAssetWorkflows",
        Summary = "Gets pending workflow work for the current user.",
        Description = "Returns active workflow instances where the current user is the delegate or one of the allowed roles for the current step."
    };

    // ── MapInventory ───────────────────────────────────────────────────

    public static EndpointDetails AdjustAssetInventory => new()
    {
        Endpoint = "/stock-adjustments",
        Name = "AdjustAssetInventory",
        Summary = "Applies a stock adjustment.",
        Description = "Adds, removes, or sets stock quantities for a stock-tracked asset and location, then records the adjustment and audit trail."
    };

    public static EndpointDetails PerformAssetStockTake => new()
    {
        Endpoint = "/stock-take",
        Name = "PerformAssetStockTake",
        Summary = "Performs a physical stock take.",
        Description = "Records counted quantities for a location and updates the inventory balances for the counted assets."
    };

    public static EndpointDetails GetLowStockAssets => new()
    {
        Endpoint = "/low-stock",
        Name = "GetLowStockAssets",
        Summary = "Gets low stock items.",
        Description = "Returns stock balances that are at or below their configured minimum thresholds."
    };

    public static EndpointDetails GetCurrentStockReport => new()
    {
        Endpoint = "/reports/current-stock",
        Name = "GetCurrentStockReport",
        Summary = "Gets the current stock report.",
        Description = "Returns the current inventory balances for all tracked asset and location combinations."
    };

    // ── MapReports ─────────────────────────────────────────────────────

    public static EndpointDetails GetAssetUtilizationReport => new()
    {
        Endpoint = "/utilization",
        Name = "GetAssetUtilizationReport",
        Summary = "Gets asset utilization metrics.",
        Description = "Returns assignment and idle metrics to help identify underused or highly utilized asset groups."
    };

    public static EndpointDetails GetAssetLifecycleReport => new()
    {
        Endpoint = "/lifecycle",
        Name = "GetAssetLifecycleReport",
        Summary = "Gets asset lifecycle metrics.",
        Description = "Groups assets by age buckets and supports replacement planning and end-of-life analysis."
    };

    public static EndpointDetails GetAssetComplianceReport => new()
    {
        Endpoint = "/compliance",
        Name = "GetAssetComplianceReport",
        Summary = "Gets asset compliance metrics.",
        Description = "Returns counts for unassigned assets, overdue returns, missing warranties, and maintenance items due soon."
    };

    // ── MapAdmin ────────────────────────────────────────────────────────

    public static EndpointDetails GetGlobalAssetActivity => new()
    {
        Endpoint = "/activity",
        Name = "GetGlobalAssetActivity",
        Summary = "Gets the global asset activity feed.",
        Description = "Returns cross-system asset activity entries for audit, investigation, and operational reporting."
    };

    public static EndpointDetails GenerateAssetAuditReport => new()
    {
        Endpoint = "/audit/generate",
        Name = "GenerateAssetAuditReport",
        Summary = "Queues an audit report for generation.",
        Description = "Queues a background job that gathers asset activity records for the requested date range and report format."
    };

    // ── MapSelfService ─────────────────────────────────────────────────

    public static EndpointDetails GetMyAssignedAssets => new()
    {
        Endpoint = "/assets",
        Name = "GetMyAssignedAssets",
        Summary = "Gets assets assigned to the current user.",
        Description = "Returns the authenticated user's currently assigned assets for self-service visibility."
    };
}
