# Leave Management Module

The Leave Management module adds configurable leave types, employee balances, leave requests, multi-level approvals, delegations, holidays, blackout periods, accrual rules, audit logs, reports, and real-time notifications.

## Architecture

- API: `src/Modules/LeaveManagementModule/Api`
- Application: `src/Modules/LeaveManagementModule/Application`
- Domain: `src/Modules/LeaveManagementModule/Domain`
- Infrastructure: `src/Modules/LeaveManagementModule/Infrastructure`
- Linked into core projects through `Alphabet.Application.csproj`, `Alphabet.Domain.csproj`, and `Alphabet.Infrastructure.csproj`

The module follows the same structure as `OrderModule`: feature API endpoints live inside the module project, while application/domain/infrastructure implementations are compiled into their respective core layers.

## Main Endpoints

- `GET /api/v1/leave/types`: Lists configured leave policies.
- `POST /api/v1/leave/types`: Creates a leave policy. Requires `leave.type.manage`.
- `POST /api/v1/leave/requests`: Submits a leave request. Requires `leave.request.create`.
- `GET /api/v1/leave/requests`: Searches leave requests. Requires `leave.request.view`.
- `POST /api/v1/leave/approvals/{requestId}/approve`: Approves the current workflow step.
- `POST /api/v1/leave/approvals/{requestId}/reject`: Rejects a request and releases reserved balance.
- `GET /api/v1/leave/balances/me`: Shows the current user's balances.
- `POST /api/v1/leave/balances/adjust`: Adjusts a user's balance. Requires `leave.balance.adjust`.
- `POST /api/v1/leave/delegations`: Creates approval delegation. Requires `leave.delegate.create`.
- `GET /api/v1/leave/calendar`: Returns approved leave grouped by day.
- `GET /api/v1/leave/admin/audit`: Returns leave audit entries.
- `GET /api/v1/leave/admin/reports/summary`: Returns reporting metrics.
- `GET /hubs/leave-management`: SignalR hub for live leave notifications.

Every endpoint is documented in Swagger with summary, description, request body, and response metadata.

## Privileges

The default privilege seeder adds:

`leave.request.create`, `leave.request.view`, `leave.request.cancel`, `leave.request.modify`, `leave.approve.level1`, `leave.approve.level2`, `leave.approve.levelN`, `leave.approve.batch`, `leave.delegate.create`, `leave.delegate.revoke`, `leave.balance.view`, `leave.balance.view.any`, `leave.balance.adjust`, `leave.type.manage`, `leave.chain.manage`, `leave.report.view`, `leave.audit.view`, `leave.admin`.

The `Admin` role receives all leave privileges automatically.

## Configuration

Configuration lives under `LeaveManagement` in `appsettings.json`.

```json
{
  "LeaveManagement": {
    "DefaultCountry": "QA",
    "ExcludeWeekends": true,
    "ExcludePublicHolidays": true,
    "WeekendDays": [ "Friday", "Saturday" ],
    "NotificationChannels": [ "InApp", "Email" ],
    "Jobs": {
      "EnableAccrualJob": true,
      "EnableCarryForwardJob": true,
      "EnableEscalationJob": true
    }
  }
}
```

## Database

Generate EF migrations after the module is added:

```powershell
dotnet tool run dotnet-ef migrations add LeaveManagementInit --context AppDbContext --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj --output-dir Persistence/Migrations
dotnet tool run dotnet-ef database update --context AppDbContext --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
```

Optional SQL scripts are available in `docs/leave-management`:

- `001_create_leave_management_tables.sql`
- `002_seed_leave_management_defaults.sql`
- `sample-approval-workflow.json`

## Typical Workflow

1. Admin creates leave types and approval chains.
2. HR initializes yearly leave balances.
3. Employee submits a leave request.
4. The module validates overlap, blackout periods, and available balance.
5. The workflow creates pending approval steps.
6. Approvers approve, reject, or request changes.
7. Final approval updates balance totals and queues calendar sync.
8. Audit logs capture each important mutation.

## Running Tests

```powershell
dotnet test tests/Alphabet.UnitTests/Alphabet.UnitTests.csproj --nologo -v minimal
dotnet test tests/Alphabet.IntegrationTests/Alphabet.IntegrationTests.csproj --nologo -v minimal
```

## Adding New Leave Features

1. Add domain entities or enums under `src/Modules/LeaveManagementModule/Domain`.
2. Add commands, queries, DTOs, validators, and handlers under `Application/Features/LeaveManagement`.
3. Add persistence in `Infrastructure/Repositories` and mappings in `Infrastructure/Persistence/Configurations`.
4. Add endpoints in `Api/LeaveManagementModuleEndpoints.cs`.
5. Register new services in `Alphabet.Infrastructure/DependencyInjection.cs`.
6. Add privileges in `DefaultPrivilegesSeeder.cs` when authorization is required.
7. Add tests under `tests/Alphabet.UnitTests/LeaveManagement`.
