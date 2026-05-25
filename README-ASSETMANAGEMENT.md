# Asset Management Module

The asset management module lives in [src/Modules/AssetManagementModule](/C:/Users/moinc/OneDrive/Documents/New%20project/src/Modules/AssetManagementModule) and follows the same module shape as the rest of the solution:

- `Api`
- `Application`
- `Domain`
- `Infrastructure`

It adds asset lifecycle tracking, inventory management, maintenance scheduling, workflow approvals, assignment history, audit logging, reporting, and scan-ready asset lookup.

## Setup and migrations

1. Create the migration after pulling the latest code:

   ```bash
   dotnet tool run dotnet-ef migrations add AssetManagementInit --context AppDbContext --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
   ```

2. Apply the migration:

   ```bash
   dotnet tool run dotnet-ef database update --context AppDbContext --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
   ```

3. Optional reference SQL scripts live in:
   - [001_create_asset_management_tables.sql](/C:/Users/moinc/OneDrive/Documents/New%20project/docs/asset-management/001_create_asset_management_tables.sql)
   - [002_seed_asset_management_defaults.sql](/C:/Users/moinc/OneDrive/Documents/New%20project/docs/asset-management/002_seed_asset_management_defaults.sql)

4. The app seeds default locations, categories, and the procurement workflow on startup through [AssetManagementSeedDataSeeder.cs](/C:/Users/moinc/OneDrive/Documents/New%20project/src/Modules/AssetManagementModule/Infrastructure/Data/Seeders/AssetManagementSeedDataSeeder.cs).

## Configuration

Configuration lives in:

- [appsettings.json](/C:/Users/moinc/OneDrive/Documents/New%20project/src/Gateway/Alphabet.AppWire/appsettings.json)
- [appsettings.Development.json](/C:/Users/moinc/OneDrive/Documents/New%20project/src/Gateway/Alphabet.AppWire/appsettings.Development.json)

Relevant sections:

- `AssetManagementSettings`
- `AssetWorkflowSettings`
- `AssetDepreciationSettings`
- `AssetNotificationSettings`
- `AssetBarcodeSettings`
- `AssetImportExportSettings`

Key defaults:

- asset tag prefix: `AST`
- QR code payload generation: enabled
- barcode payload generation: enabled
- low stock threshold: `5`
- depreciation method: `StraightLine`
- maintenance reminder lead time: `14` days
- workflow escalation check interval: `60` minutes

## Core endpoints

Asset master:

- `POST /api/v1/assets`
- `GET /api/v1/assets`
- `GET /api/v1/assets/{assetId}`
- `PUT /api/v1/assets/{assetId}`
- `DELETE /api/v1/assets/{assetId}`
- `POST /api/v1/assets/{assetId}/move`
- `GET /api/v1/assets/scan?barcode=AST-000001`

Assignments:

- `POST /api/v1/assets/{assetId}/assign`
- `POST /api/v1/assets/{assetId}/unassign`
- `POST /api/v1/assets/{assetId}/transfer`
- `GET /api/v1/assets/{assetId}/assignments`
- `GET /api/v1/users/me/assets`

Maintenance:

- `POST /api/v1/assets/{assetId}/maintenance`
- `POST /api/v1/assets/{assetId}/maintenance/{maintenanceId}/complete`
- `GET /api/v1/assets/{assetId}/maintenance`

Workflows:

- `POST /api/v1/workflows`
- `POST /api/v1/workflows/assets/{assetId}/start`
- `GET /api/v1/workflows/instances/{instanceId}`
- `POST /api/v1/workflows/instances/{instanceId}/steps/{stepId}/action`
- `GET /api/v1/workflows/pending`

Inventory and reporting:

- `POST /api/v1/inventory/stock-adjustments`
- `POST /api/v1/inventory/stock-take`
- `GET /api/v1/inventory/low-stock`
- `GET /api/v1/inventory/reports/current-stock`
- `GET /api/v1/assets/{assetId}/depreciation`
- `GET /api/v1/reports/utilization`
- `GET /api/v1/reports/lifecycle`
- `GET /api/v1/reports/compliance`
- `GET /api/v1/admin/activity`
- `POST /api/v1/admin/audit/generate`

## Asset lifecycle management

Typical lifecycle:

1. Create an asset with `POST /api/v1/assets`.
2. Assign it with `POST /api/v1/assets/{assetId}/assign`.
3. Review assignment history with `GET /api/v1/assets/{assetId}/assignments`.
4. Return it with `POST /api/v1/assets/{assetId}/unassign`.
5. Retire or dispose it with `DELETE /api/v1/assets/{assetId}` and a retirement body.

Best practices:

- Keep `AssetTag` stable and human-readable. The default generator uses `AST-######`.
- Use category-specific `CustomFields` for structured metadata such as processor, RAM, or storage.
- Use assignment reasons and return notes consistently because they feed audit and operational reporting.

## Workflow configuration

Workflow definitions are created with ordered steps. Each step captures:

- name
- role owner
- required approvals
- timeout in hours
- allowed actions

Use workflows for:

- procurement approvals
- disposal approvals
- exception approvals for sensitive asset assignment

Escalation is handled by the recurring background job in [AssetManagementBackgroundJobs.cs](/C:/Users/moinc/OneDrive/Documents/New%20project/src/Modules/AssetManagementModule/Infrastructure/BackgroundJobs/AssetManagementBackgroundJobs.cs).

## Inventory management

Use `InventoryBalances` for consumable or stock-tracked assets and `StockAdjustments` for audit-safe quantity changes.

Recommended flow:

1. Receive stock with `POST /api/v1/inventory/stock-adjustments`.
2. Monitor low stock with `GET /api/v1/inventory/low-stock`.
3. Perform physical counts with `POST /api/v1/inventory/stock-take`.
4. Review the rolled-up snapshot using `GET /api/v1/inventory/reports/current-stock`.

## Audit and compliance

Every important asset action writes an `AssetActivityLog` entry, including:

- create
- update
- assign
- unassign
- transfer
- move
- maintenance schedule/complete
- workflow start/action
- stock adjustment and stock take

Use:

- `GET /api/v1/assets/{assetId}/activity` for an asset-level timeline
- `GET /api/v1/admin/activity` for cross-system review
- `POST /api/v1/admin/audit/generate` to queue a broader report job

Retention defaults to seven years through `AssetManagementSettings.RetentionDays`.

## Integration with other modules

Privilege module:

- asset routes use privilege metadata such as `asset.view`, `asset.create`, `asset.assign`, and `asset.audit`
- seeded defaults are added in [DefaultPrivilegesSeeder.cs](/C:/Users/moinc/OneDrive/Documents/New%20project/src/Modules/PrivilegeModule/Infrastructure/Data/Seeders/DefaultPrivilegesSeeder.cs)

Scheduler module:

- recurring workflow escalation
- recurring maintenance reminders

Communication module:

- assignment notifications
- low stock alerts
- maintenance reminder delivery hooks

Identity module:

- validates assignees through the application user directory abstraction
- supports `/api/v1/users/me/assets` through the current authenticated user context
