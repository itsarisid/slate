# Alphabet Seed Data — full package

Dummy-data seeders for every module in the Alphabet API's swagger spec, plus
one minimal-API endpoint file (`SeedEndpoints.cs`) that exposes them all
under `/api/v1/seed/*`.

## Folder layout

```
AlphabetSeedData/
├── Entities/      24 files — EF Core entities + enums for every module
├── Seeders/        16 files — one seeder per module, plus the orchestrator
└── Endpoints/       1 file  — SeedEndpoints.cs (minimal API, all routes)
```

Nothing here is a controller — the earlier `*SeedController.cs` files from
each module have been superseded by `Endpoints/SeedEndpoints.cs`, which maps
the same routes as minimal API endpoints instead. Delete the old controller
files if you copied them in already; you don't need both.

## What's in each folder

### Entities/ (24 files)
EF Core entity classes and enums, one pair per module, inferred from the
swagger request/response DTOs:

| File(s) | Module |
|---|---|
| `Product.cs` | Products |
| `AssetEnums.cs`, `AssetEntities.cs` | Assets (Location, AssetCategory, Asset, AssetAssignment, AssetMaintenance) |
| `LeaveEnums.cs`, `LeaveEntities.cs` | Leave (LeaveType, AccrualRule, Balance, Request, PublicHoliday, BlackoutPeriod, Delegation) |
| `PrivilegeEnums.cs`, `PrivilegeEntities.cs` | Privileges (Category, Privilege, Policy, UserAssignment, RoleAssignment, AccessRequest, AuditLog) |
| `SchedulerJobEnums.cs`, `SchedulerJobEntities.cs` | Scheduler Jobs (SchedulerJob, Execution, Dependency, Exclusion) |
| `WorkflowEnums.cs`, `WorkflowEntities.cs` | Workflows (Definition, Step, Instance, StepInstance, SchedulerWorkflow, SchedulerWorkflowJob) |
| `IdentityEntities.cs` | Admin/Identity (AppUser, AppRole, UserPreferences, UserSession, UserAuditLog) |
| `TodoEnums.cs`, `TodoEntities.cs` | Todos (Todo, RecurrencePattern, ChecklistItem) |
| `NoteEnums.cs`, `NoteEntities.cs` | Notes (Notebook, Note, NoteShare, NoteVersion) |
| `EventEnums.cs`, `EventEntities.cs` | Events (Event, RecurrencePattern, EventAttendee) |
| `ReminderEnums.cs`, `ReminderEntities.cs` | Reminders (Reminder, RecurrencePattern) |
| `TaskEnums.cs`, `TaskEntities.cs` | Tasks (**WorkTask** — not `Task`, see note below) |
| `SmartListEntities.cs` | Smart Views (SmartList) |
| `TemplateEntities.cs` | Templates (Template) |
| `CommunicationEntities.cs` | Communication (CommunicationBatch, DeliveryResult, Configuration) |

**Naming note:** the Tasks module's entity is `WorkTask`/`WorkTaskStatus`,
not `Task`/`TaskStatus` — those collide with
`System.Threading.Tasks.Task`/`TaskStatus`, used everywhere in async C#.

### Seeders/ (16 files)
One seeder class + interface per module (e.g. `IAssetSeeder`/`AssetSeeder`),
each using [Bogus](https://github.com/bchavez/Bogus) to generate realistic
data and an injected `DbContext` to save it — **except** `IdentitySeeder`,
which goes through `UserManager`/`RoleManager` instead, since ASP.NET Core
Identity needs proper password hashing.

`SeedOrchestrator.cs` is the odd one out: it doesn't seed anything itself —
it calls all 15 other seeders in dependency order and threads real generated
IDs (users, roles, scheduler jobs) between them instead of letting each one
fall back to random Guids.

### Endpoints/SeedEndpoints.cs (1 file)
A single `MapSeedEndpoints(this IEndpointRouteBuilder app)` extension
covering every seeder above, in your minimal-API style
(`Results<Ok<T>, BadRequest<ProblemDetails>>`, `.Produces<T>()`). Call
`app.MapSeedEndpoints();` once from `Program.cs`.

## Every endpoint this adds

| Route | Seeds |
|---|---|
| `POST /api/v1/seed/products` | Products |
| `POST /api/v1/seed/assets` | Locations, Categories, Assets, Assignments, Maintenance |
| `POST /api/v1/seed/leave` | Leave Types, Accrual Rules, Balances, Requests, Holidays, Blackouts, Delegations |
| `POST /api/v1/seed/privileges` | Categories, Privileges, Policies, Assignments, Access Requests, Audit Log |
| `POST /api/v1/seed/scheduler-jobs` | Jobs, Executions, Dependencies, Exclusions |
| `POST /api/v1/seed/workflows` | Definitions, Steps, Instances, Scheduler DAG Workflows |
| `POST /api/v1/seed/users` | Roles, Users, Preferences, Sessions, Audit Log |
| `POST /api/v1/seed/todos` | Todos, Checklist Items |
| `POST /api/v1/seed/notes` | Notebooks, Notes, Shares, Versions |
| `POST /api/v1/seed/events` | Events, Attendees |
| `POST /api/v1/seed/reminders` | Reminders |
| `POST /api/v1/seed/tasks` | WorkTasks, Checklist Items, Dependencies, Time Entries, Status History |
| `POST /api/v1/seed/smart-lists` | Smart Lists |
| `POST /api/v1/seed/templates` | Templates |
| `POST /api/v1/seed/communications` | Configuration, Batches, Delivery Results |
| `POST /api/v1/seed/all` | **Everything above, in order, with real IDs threaded through** |

Every endpoint takes `?wipeExisting=true|false` plus its own count-style
query params (e.g. `?assets=100&locations=5`) — see each `*SeedOptions`
class in `Seeders/` for the full list per module.

## Not seeded — nothing to seed there
- Productivity Reports dashboard, Smart Views' search/today-dashboard —
  entirely computed from other tables at query time.
- System Log — reads Serilog files off disk, not a database table.

## Setting it up in your project

1. **Add the Bogus package** (used by every seeder except Identity's DI
   pieces, which don't need it):
   ```
   dotnet add package Bogus
   ```

2. **Fix namespaces.** Every file uses:
   - `Alphabet.Domain.Entities` (Entities/)
   - `Alphabet.Infrastructure.Data.Seed` (Seeders/)
   - `Alphabet.Api.Endpoints` (Endpoints/)

   Rename these to match your actual project structure, or add the folders
   under those namespaces as-is.

3. **Swap the `DbContext` placeholder.** Every seeder constructor takes a
   bare `DbContext context` — replace with your actual `AppDbContext` (or
   whatever it's called) in all 16 seeder files.

4. **Confirm/add `DbSet<T>`s.** Each seeder's file header comments list which
   entities it needs a `DbSet<T>` for. Add any missing ones to your
   `DbContext`.

5. **Configure owned types**, used for embedded value objects:
   ```csharp
   modelBuilder.Entity<SchedulerJob>().OwnsOne(j => j.Configuration);
   modelBuilder.Entity<SchedulerJob>().OwnsOne(j => j.RetryPolicy);
   modelBuilder.Entity<Todo>().OwnsOne(t => t.RecurrencePattern);
   modelBuilder.Entity<Event>().OwnsOne(e => e.Recurrence);
   modelBuilder.Entity<Reminder>().OwnsOne(r => r.RecurrencePattern);
   ```

6. **Identity module specifics** (`IdentitySeeder.cs`): if your project
   already has `ApplicationUser`/`ApplicationRole` (or similar), delete
   `AppUser`/`AppRole` from `IdentityEntities.cs` and point
   `IdentitySeeder`'s `UserManager<AppUser>`/`RoleManager<AppRole>` at your
   real classes instead.

7. **Register every seeder in DI** (`Program.cs` or your DI extension file):
   ```csharp
   services.AddScoped<IProductSeeder, ProductSeeder>();
   services.AddScoped<IAssetSeeder, AssetSeeder>();
   services.AddScoped<ILeaveSeeder, LeaveSeeder>();
   services.AddScoped<IPrivilegeSeeder, PrivilegeSeeder>();
   services.AddScoped<ISchedulerJobSeeder, SchedulerJobSeeder>();
   services.AddScoped<IWorkflowSeeder, WorkflowSeeder>();
   services.AddScoped<IIdentitySeeder, IdentitySeeder>();
   services.AddScoped<ITodoSeeder, TodoSeeder>();
   services.AddScoped<INoteSeeder, NoteSeeder>();
   services.AddScoped<IEventSeeder, EventSeeder>();
   services.AddScoped<IReminderSeeder, ReminderSeeder>();
   services.AddScoped<IWorkTaskSeeder, WorkTaskSeeder>();
   services.AddScoped<ISmartListSeeder, SmartListSeeder>();
   services.AddScoped<ITemplateSeeder, TemplateSeeder>();
   services.AddScoped<ICommunicationSeeder, CommunicationSeeder>();
   services.AddScoped<ISeedOrchestrator, SeedOrchestrator>();
   ```

8. **Map the endpoints**, once, in `Program.cs`:
   ```csharp
   app.MapSeedEndpoints();
   ```

9. **Lock it down before production.** Every route here creates or deletes
   real data, and `/users` creates real login-capable accounts. Either:
   ```csharp
   var group = app.MapGroup("/api/v1/seed")
       .RequireAuthorization("AdminOnly");
   ```
   or only call `app.MapSeedEndpoints()` when
   `app.Environment.IsDevelopment()`.

## Optional: 3 small patches for tighter cross-module linkage
Three seeders currently only return counts, not the IDs they generated.
Adding these lets later steps in `seed/all` link to real rows instead of
random placeholder Guids — everything works without them, just with looser
links:

- `SchedulerJobSeedResult.JobIds` (used by Workflows)
- `TodoSeedResult.TodoIds` (used by Reminders)
- `EventSeedResult.EventIds` (used by Reminders)

Same shape each time — add `public List<Guid> XIds { get; set; } = new();`
to the result class, and `XIds = items.Select(i => i.Id).ToList()` in the
`return new ...Result { ... }` at the end of `SeedAsync`.

## Recommended order to run things (or just call `/seed/all`)
```
users → products → assets → leave → privileges → scheduler-jobs →
workflows → todos → notes → events → reminders → tasks → smart-lists →
templates → communications
```
Users first because almost everything else references `UserId`/`RoleId`;
Workflows after Scheduler Jobs because it links to generated job IDs.
Everything else is independent of each other and can run in any order.
