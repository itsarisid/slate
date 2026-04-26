# Alphabet

Alphabet is a production-oriented .NET 10 Web API solution template built with Clean Architecture, vertical slicing, CQRS, MediatR, FluentValidation, EF Core, JWT authentication, Swagger, health checks, and cache abstractions that can switch between memory and Redis.

Authentication and MFA guidance lives in `README-AUTH.md`.

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
|   |-- Modules/ProductModule
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
3. Add a vertical slice under `src/Core/Alphabet.Application/Features/<FeatureName>` with commands, queries, handlers, validators, and DTOs.
4. Implement persistence or external adapters inside `src/Core/Alphabet.Infrastructure`.
5. Register only the new adapters in `Alphabet.Infrastructure/DependencyInjection.cs`.
6. Expose endpoints in the module's `Api` folder under `src/Modules/<ModuleName>/Api`.
7. Add unit tests for handlers and validators, then add integration tests for persistence and API behavior.

## How to add a new module

1. Create a new folder under `src/Modules/<ModuleName>`.
2. Keep the module self-contained with `Domain`, `Application`, `Infrastructure`, and `Api` folders.
3. Avoid direct references from one module to another.
4. Communicate across modules using domain events, integration events, or a message bus abstraction.
5. Register the module from the gateway or composition root only.

Every module in this repository now follows the same visible folder shape as `OrderModule`, even when some business logic still lives in shared Core projects during the current transition:

- `Api`
- `Application`
- `Domain`
- `Infrastructure`

## Communication module

The solution includes a dedicated communication module for alerts, notifications, and outbound user messaging.

- Module project: `src/Modules/CommunicationModule`
- Application feature slices: `src/Core/Alphabet.Application/Features/Communication`
- Transport implementations: `src/Core/Alphabet.Infrastructure/Services`

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
