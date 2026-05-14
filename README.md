# Alphabet

Alphabet is a production-oriented .NET 10 Web API solution template built with Clean Architecture, vertical slicing, CQRS, MediatR, FluentValidation, EF Core, JWT authentication, Swagger, health checks, and cache abstractions that can switch between memory and Redis.

Authentication and MFA guidance lives in `README-AUTH.md`.
Privilege-based access control guidance lives in `README-PRIVILEGE.md`.
Productivity module guidance lives in `README-PRODUCTIVITY.md`.

## Solution structure

```text
Alphabet/
|-- src/
|   |-- Core/
|   |   |-- Alphabet.Application
|   |   |-- Alphabet.Contracts
|   |   |-- Alphabet.Domain
|   |   `-- Alphabet.Infrastructure
|   |-- Modules/CommunicationModule
|   |-- Modules/IdentityModule
|   |-- Modules/OrderModule
|   |-- Modules/PrivilegeModule
|   |-- Modules/ProductivityModule
|   |-- Modules/ProductModule
|   |-- Modules/SchedulerModule
|   |-- Gateway/Alphabet.AppWire
|   `-- Libraries/Alphabet.Utility
|-- tests/
|   |-- Alphabet.UnitTests
|   |-- Alphabet.IntegrationTests
|   `-- Alphabet.FunctionalTests
|-- docs/
|-- docker-compose.yml
`-- Alphabet.slnx
```

## How to run

1. Start dependencies if you want SQL Server and Redis:

   ```bash
   docker compose up -d
   ```

2. Restore packages:

   ```bash
   dotnet restore Alphabet.slnx
   ```

3. Run the API:

   ```bash
   dotnet run --project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
   ```

4. Browse Swagger at `https://localhost:5001/swagger` or your configured development URL.

## Migrations

Use the infrastructure assembly with the API startup project:

```bash
dotnet ef migrations add Init \
  --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj \
  --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj \
  --output-dir Persistence/Migrations
```

Apply migrations:

```bash
dotnet ef database update \
  --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj \
  --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
```

## How to add a new feature

1. Add or update the aggregate root and value objects in `src/Core/Alphabet.Domain`.
2. Add repository abstractions or specifications only when the existing generic repository is not enough.
3. Add a vertical slice under the related module when the feature belongs to a single bounded context, for example `src/Modules/<ModuleName>/Application/Features/<FeatureName>`.
4. Keep only shared, reusable infrastructure in `src/Core/Alphabet.Infrastructure`, and place feature-owned infrastructure beside the module in `src/Modules/<ModuleName>/Infrastructure`.
5. Register only the new adapters in `Alphabet.Infrastructure/DependencyInjection.cs`.
6. Expose endpoints in the module's `Api` folder under `src/Modules/<ModuleName>/Api`.
7. Add unit tests for handlers and validators, then add integration tests for persistence and API behavior.

## How to add a new module

1. Create a new folder under `src/Modules/<ModuleName>`.
2. Keep the module self-contained with `Domain`, `Application`, `Infrastructure`, and `Api` folders.
3. Avoid direct references from one module to another.
4. Communicate across modules using domain events, integration events, or a message bus abstraction.
5. Register the module from the gateway or composition root only.

Every module in this repository now follows the same visible folder shape as `OrderModule`:

- `Api`
- `Application`
- `Domain`
- `Infrastructure`

Shared infrastructure that is reused by multiple modules stays in `src/Core/Alphabet.Infrastructure`, such as:

- caching
- generic persistence and `AppDbContext`
- health checks
- logging
- current-user access
- generic background jobs

Feature-owned infrastructure lives inside the module folders and is compiled into the core infrastructure assembly through linked source includes. That keeps ownership close to the feature without duplicating runtime registrations.

## Communication module

The solution includes a dedicated communication module for alerts, notifications, and outbound user messaging.

- Module project: `src/Modules/CommunicationModule`
- Application feature slices: `src/Modules/CommunicationModule/Application/Features/Communication`
- Transport implementations: `src/Modules/CommunicationModule/Infrastructure/Services`

Supported channels:

- `Email`
- `Sms`
- `Push`
- `InApp`
- `Webhook`

Configuration is controlled through the `Communication` section in `appsettings.json` and `appsettings.Development.json`.

Example request payload:

```json
{
  "subject": "System maintenance",
  "body": "A scheduled maintenance window starts at 22:00 UTC.",
  "channels": ["Email", "Sms", "InApp"],
  "emailAddress": "user@example.com",
  "phoneNumber": "+97455555555",
  "userId": "3f45ef0f-39d1-4d65-8eaa-5db7c0b8f763",
  "pushToken": null,
  "webhookUrl": null,
  "isHtml": false
}
```

Endpoints:

- `POST /api/v1/communications/send`
- `GET /api/v1/communications/configuration`

All public methods and communication endpoints include XML comments plus Swagger summaries and descriptions so consumers can understand what each method and route does without reading the internal implementation.

## Privilege module

The solution includes a dedicated privilege-based access control module under `src/Modules/PrivilegeModule`.

- API base paths:
  - `POST /api/v1/privileges`
  - `GET /api/v1/privileges`
  - `POST /api/v1/roles/{roleId}/privileges`
  - `POST /api/v1/users/{userId}/privileges`
  - `GET /api/v1/auth/check-privilege/{privilegeName}`
  - `POST /api/v1/users/me/privilege-requests`
- documentation: `README-PRIVILEGE.md`

This module works alongside ASP.NET Core Identity roles and supports:

- privilege catalog management
- role and direct-user privilege assignments
- composite policies
- self-service privilege requests
- audit logging and analytics

## Scheduler module

The solution also includes a Hangfire-first scheduler module under [src/Modules/SchedulerModule](/C:/Users/moinc/OneDrive/Documents/New%20project/src/Modules/SchedulerModule) for dynamic job creation, rescheduling, monitoring, retries, and execution history.

- API base path: `/api/v1/scheduler`
- Admin operations: `/api/v1/scheduler/admin/*`
- Dashboard: `/hangfire`

Operational guidance and job examples live in [README-SCHEDULER.md](/C:/Users/moinc/OneDrive/Documents/New%20project/README-SCHEDULER.md).

## Productivity module

The solution includes a dedicated productivity module under [src/Modules/ProductivityModule](/C:/Users/moinc/OneDrive/Documents/New%20project/src/Modules/ProductivityModule) for collaborative work management and personal organization.

- API base paths:
  - `/api/v1/todos`
  - `/api/v1/reminders`
  - `/api/v1/notes`
  - `/api/v1/tasks`
  - `/api/v1/events`
  - `/api/v1/search`
  - `/api/v1/dashboard/today`
  - `/api/v1/reports/productivity`
- SignalR hub:
  - `/hubs/productivity`
- documentation:
  - `README-PRODUCTIVITY.md`

The module currently includes:

- todos with filtering, reminders, and task conversion
- reminders with Hangfire-backed scheduling
- notes with sharing, export, and version history
- task boards, time tracking, and dependency graphs
- calendar views, availability checks, and iCal export
- global search, dashboards, smart lists, templates, and reports

## How to switch database providers

The provider is selected by `Database.Provider` in `appsettings.json`.

- `SqlServer`: uses `UseSqlServer`.
- `PostgreSql` or `Postgres`: uses `UseNpgsql`.
- `InMemory`: uses `UseInMemoryDatabase`.

To move from SQL Server to PostgreSQL:

1. Change `Database.Provider` to `PostgreSql`.
2. Update `Database.ConnectionString`.
3. Create a new migration for the provider if needed.
4. Re-run `dotnet ef database update`.

All provider-specific code is isolated to `Alphabet.Infrastructure/DependencyInjection.cs`.

## How to switch from in-memory cache to Redis

1. Set `Cache.Provider` to `Redis`.
2. Set `Cache.RedisConnectionString`.
3. Ensure Redis is reachable.
4. Restart the API.

The `ICacheService` abstraction remains unchanged, so application code does not need to change.

## Testing

Run all tests:

```bash
dotnet test Alphabet.slnx
```

Run only unit tests:

```bash
dotnet test tests/Alphabet.UnitTests/Alphabet.UnitTests.csproj
```

## Notes

- Controllers and endpoints remain thin and delegate all behavior through MediatR.
- Repositories do not call `SaveChanges`; the unit of work owns transaction boundaries.
- Validation runs in the MediatR pipeline before handlers execute.
- Global exception handling returns Problem Details payloads.
- Secrets should be stored in user secrets, environment variables, or a secret vault in non-local environments.
