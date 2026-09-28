// Consolidates every seed endpoint (Modules 1-16 + the seed/all orchestrator
// from Module 8) into one minimal-API endpoint group, following the same
// pattern as your UploadMyAvatar endpoint: typed `Results<...>` return,
// `.Produces<T>()`, grouped under a shared route prefix.
//
// I don't have your `ApiResource` catalog, so each endpoint below uses
// `.WithName(...)` / `.WithSummary(...)` instead of `.WithDocumentation(...)`.
// If you keep an ApiResource entry per endpoint (the way UploadMyAvatar
// does), add one for each `SeedX` name below and swap the two calls for
// `.WithDocumentation(ApiResource.SeedX)` — everything else (the route,
// the handler, `.Produces<>()`) stays the same.
//
// SECURITY: every one of these creates/deletes real data — several create
// real login-capable users. Add `.RequireAuthorization("AdminOnly")` (or
// your equivalent policy) to the group, or wrap `MapSeedEndpoints` in
// `if (app.Environment.IsDevelopment())` at the call site, before this ships
// anywhere near production.
//
// WIRING: swap every `Alphabet.Infrastructure.Data.Seed` interface below for
// wherever you actually placed the seeders from Modules 1-16, and register
// `app.MapSeedEndpoints();` once in Program.cs / your endpoint-mapping
// extension chain.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Alphabet.Infrastructure.Data.Seed;

namespace Alphabet.Api.Endpoints
{
    public static class SeedEndpoints
    {
        public static IEndpointRouteBuilder MapSeedEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/seed")
                .WithTags("Seed")
                // .RequireAuthorization("AdminOnly") // <-- uncomment once you have a policy for this
                ;

            // ---- Module 1: Products ----
            group.MapPost("/products", async Task<Results<Ok<object>, BadRequest<ProblemDetails>>> (
                [FromServices] IProductSeeder seeder,
                [FromQuery] int count,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                count = count == 0 ? 50 : count;
                if (count <= 0 || count > 5000)
                    return TypedResults.BadRequest(Problem("count must be between 1 and 5000."));

                var inserted = await seeder.SeedAsync(count, wipeExisting, ct);
                return TypedResults.Ok<object>(new { message = "Products module seeded.", count = inserted });
            })
            .WithName("SeedProducts")
            .WithSummary("Seed the Products module with dummy data.")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 2: Assets ----
            group.MapPost("/assets", async Task<Results<Ok<AssetSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] IAssetSeeder seeder,
                [FromQuery] int locations,
                [FromQuery] int categories,
                [FromQuery] int assets,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                assets = assets == 0 ? 100 : assets;
                if (assets <= 0 || assets > 5000)
                    return TypedResults.BadRequest(Problem("assets must be between 1 and 5000."));

                var result = await seeder.SeedAsync(new AssetSeedOptions
                {
                    LocationCount = locations == 0 ? 5 : locations,
                    CategoryCount = categories == 0 ? 8 : categories,
                    AssetCount = assets,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedAssets")
            .WithSummary("Seed the Assets module (Locations, Categories, Assets, Assignments, Maintenance).")
            .Produces<AssetSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 3: Leave ----
            group.MapPost("/leave", async Task<Results<Ok<LeaveSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] ILeaveSeeder seeder,
                [FromQuery] int users,
                [FromQuery] int requestsPerUser,
                [FromQuery] int publicHolidays,
                [FromQuery] int blackoutPeriods,
                [FromQuery] int delegations,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                users = users == 0 ? 20 : users;
                if (users <= 0 || users > 2000)
                    return TypedResults.BadRequest(Problem("users must be between 1 and 2000."));

                var result = await seeder.SeedAsync(new LeaveSeedOptions
                {
                    UserCount = users,
                    RequestsPerUser = requestsPerUser == 0 ? 3 : requestsPerUser,
                    PublicHolidayCount = publicHolidays == 0 ? 10 : publicHolidays,
                    BlackoutPeriodCount = blackoutPeriods == 0 ? 2 : blackoutPeriods,
                    DelegationCount = delegations == 0 ? 5 : delegations,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedLeave")
            .WithSummary("Seed the Leave module (Types, Accrual Rules, Balances, Requests, Holidays, Blackouts, Delegations).")
            .Produces<LeaveSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 4: Privileges ----
            group.MapPost("/privileges", async Task<Results<Ok<PrivilegeSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] IPrivilegeSeeder seeder,
                [FromQuery] int users,
                [FromQuery] int roles,
                [FromQuery] int policies,
                [FromQuery] int assignmentsPerUser,
                [FromQuery] int accessRequests,
                [FromQuery] int auditLogEntries,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                users = users == 0 ? 20 : users;
                if (users <= 0 || users > 2000)
                    return TypedResults.BadRequest(Problem("users must be between 1 and 2000."));

                var result = await seeder.SeedAsync(new PrivilegeSeedOptions
                {
                    UserCount = users,
                    RoleCount = roles == 0 ? 5 : roles,
                    PolicyCount = policies == 0 ? 6 : policies,
                    AssignmentsPerUser = assignmentsPerUser == 0 ? 4 : assignmentsPerUser,
                    AccessRequestCount = accessRequests == 0 ? 15 : accessRequests,
                    AuditLogCount = auditLogEntries == 0 ? 100 : auditLogEntries,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedPrivileges")
            .WithSummary("Seed the Privileges module (Categories, Privileges, Policies, Assignments, Access Requests, Audit Log).")
            .Produces<PrivilegeSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 5: Scheduler Jobs ----
            group.MapPost("/scheduler-jobs", async Task<Results<Ok<SchedulerJobSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] ISchedulerJobSeeder seeder,
                [FromQuery] int jobs,
                [FromQuery] int minExecutionsPerJob,
                [FromQuery] int maxExecutionsPerJob,
                [FromQuery] int dependencies,
                [FromQuery] int exclusions,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                jobs = jobs == 0 ? 25 : jobs;
                minExecutionsPerJob = minExecutionsPerJob == 0 ? 3 : minExecutionsPerJob;
                maxExecutionsPerJob = maxExecutionsPerJob == 0 ? 15 : maxExecutionsPerJob;
                if (jobs <= 0 || jobs > 2000)
                    return TypedResults.BadRequest(Problem("jobs must be between 1 and 2000."));
                if (minExecutionsPerJob > maxExecutionsPerJob)
                    return TypedResults.BadRequest(Problem("minExecutionsPerJob must be <= maxExecutionsPerJob."));

                var result = await seeder.SeedAsync(new SchedulerJobSeedOptions
                {
                    JobCount = jobs,
                    MinExecutionsPerJob = minExecutionsPerJob,
                    MaxExecutionsPerJob = maxExecutionsPerJob,
                    DependencyCount = dependencies == 0 ? 8 : dependencies,
                    ExclusionCount = exclusions == 0 ? 5 : exclusions,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedSchedulerJobs")
            .WithSummary("Seed the Scheduler Jobs module (Jobs, Executions, Dependencies, Exclusions).")
            .Produces<SchedulerJobSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 6: Workflows ----
            group.MapPost("/workflows", async Task<Results<Ok<WorkflowSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] IWorkflowSeeder seeder,
                [FromQuery] int definitions,
                [FromQuery] int instancesPerDefinition,
                [FromQuery] int schedulerWorkflows,
                [FromQuery] int minJobsPerSchedulerWorkflow,
                [FromQuery] int maxJobsPerSchedulerWorkflow,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                minJobsPerSchedulerWorkflow = minJobsPerSchedulerWorkflow == 0 ? 2 : minJobsPerSchedulerWorkflow;
                maxJobsPerSchedulerWorkflow = maxJobsPerSchedulerWorkflow == 0 ? 6 : maxJobsPerSchedulerWorkflow;
                if (minJobsPerSchedulerWorkflow > maxJobsPerSchedulerWorkflow)
                    return TypedResults.BadRequest(Problem("minJobsPerSchedulerWorkflow must be <= maxJobsPerSchedulerWorkflow."));

                var result = await seeder.SeedAsync(new WorkflowSeedOptions
                {
                    DefinitionCount = definitions == 0 ? 6 : definitions,
                    InstancesPerDefinition = instancesPerDefinition == 0 ? 5 : instancesPerDefinition,
                    SchedulerWorkflowCount = schedulerWorkflows == 0 ? 4 : schedulerWorkflows,
                    MinJobsPerSchedulerWorkflow = minJobsPerSchedulerWorkflow,
                    MaxJobsPerSchedulerWorkflow = maxJobsPerSchedulerWorkflow,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedWorkflows")
            .WithSummary("Seed the Workflows module (Definitions, Steps, Instances, Scheduler DAG Workflows).")
            .Produces<WorkflowSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 7: Admin/Identity (Users) ----
            group.MapPost("/users", async Task<Results<Ok<IdentitySeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] IIdentitySeeder seeder,
                [FromQuery] int users,
                [FromQuery] string? defaultPassword,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                users = users == 0 ? 25 : users;
                if (users <= 0 || users > 2000)
                    return TypedResults.BadRequest(Problem("users must be between 1 and 2000."));

                var result = await seeder.SeedAsync(new IdentitySeedOptions
                {
                    UserCount = users,
                    DefaultPassword = string.IsNullOrWhiteSpace(defaultPassword) ? "Seed@12345" : defaultPassword,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedUsers")
            .WithSummary("Seed the Admin/Identity module (Roles, Users, Preferences, Sessions, Audit Log). Creates real login-capable accounts.")
            .Produces<IdentitySeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 9: Todos ----
            group.MapPost("/todos", async Task<Results<Ok<TodoSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] ITodoSeeder seeder,
                [FromQuery] int todosPerUser,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                todosPerUser = todosPerUser == 0 ? 8 : todosPerUser;
                if (todosPerUser <= 0 || todosPerUser > 500)
                    return TypedResults.BadRequest(Problem("todosPerUser must be between 1 and 500."));

                var result = await seeder.SeedAsync(new TodoSeedOptions
                {
                    TodosPerUser = todosPerUser,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedTodos")
            .WithSummary("Seed the Todos module (Todos, Checklist Items).")
            .Produces<TodoSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 10: Notes ----
            group.MapPost("/notes", async Task<Results<Ok<NoteSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] INoteSeeder seeder,
                [FromQuery] int notebooksPerUser,
                [FromQuery] int notesPerUser,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                notesPerUser = notesPerUser == 0 ? 10 : notesPerUser;
                if (notesPerUser <= 0 || notesPerUser > 500)
                    return TypedResults.BadRequest(Problem("notesPerUser must be between 1 and 500."));

                var result = await seeder.SeedAsync(new NoteSeedOptions
                {
                    NotebooksPerUser = notebooksPerUser == 0 ? 2 : notebooksPerUser,
                    NotesPerUser = notesPerUser,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedNotes")
            .WithSummary("Seed the Notes module (Notebooks, Notes, Shares, Versions).")
            .Produces<NoteSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 11: Events ----
            group.MapPost("/events", async Task<Results<Ok<EventSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] IEventSeeder seeder,
                [FromQuery] int eventsPerUser,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                eventsPerUser = eventsPerUser == 0 ? 6 : eventsPerUser;
                if (eventsPerUser <= 0 || eventsPerUser > 200)
                    return TypedResults.BadRequest(Problem("eventsPerUser must be between 1 and 200."));

                var result = await seeder.SeedAsync(new EventSeedOptions
                {
                    EventsPerUser = eventsPerUser,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedEvents")
            .WithSummary("Seed the Events module (Events, Attendees).")
            .Produces<EventSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 12: Reminders ----
            group.MapPost("/reminders", async Task<Results<Ok<ReminderSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] IReminderSeeder seeder,
                [FromQuery] int remindersPerUser,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                remindersPerUser = remindersPerUser == 0 ? 5 : remindersPerUser;
                if (remindersPerUser <= 0 || remindersPerUser > 200)
                    return TypedResults.BadRequest(Problem("remindersPerUser must be between 1 and 200."));

                var result = await seeder.SeedAsync(new ReminderSeedOptions
                {
                    RemindersPerUser = remindersPerUser,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedReminders")
            .WithSummary("Seed the Reminders module.")
            .Produces<ReminderSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 13: Tasks (WorkTask) ----
            group.MapPost("/tasks", async Task<Results<Ok<WorkTaskSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] IWorkTaskSeeder seeder,
                [FromQuery] int taskCount,
                [FromQuery] double subtaskRatio,
                [FromQuery] int dependencyCount,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                taskCount = taskCount == 0 ? 40 : taskCount;
                subtaskRatio = subtaskRatio == 0 ? 0.25 : subtaskRatio;
                if (taskCount <= 0 || taskCount > 5000)
                    return TypedResults.BadRequest(Problem("taskCount must be between 1 and 5000."));
                if (subtaskRatio < 0 || subtaskRatio > 0.9)
                    return TypedResults.BadRequest(Problem("subtaskRatio must be between 0 and 0.9."));

                var result = await seeder.SeedAsync(new WorkTaskSeedOptions
                {
                    TaskCount = taskCount,
                    SubtaskRatio = subtaskRatio,
                    DependencyCount = dependencyCount == 0 ? 15 : dependencyCount,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedTasks")
            .WithSummary("Seed the Tasks module (WorkTasks, Checklist Items, Dependencies, Time Entries, Status History).")
            .Produces<WorkTaskSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 14: Smart Views (Smart Lists) ----
            group.MapPost("/smart-lists", async Task<Results<Ok<SmartListSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] ISmartListSeeder seeder,
                [FromQuery] int listsPerUser,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                listsPerUser = listsPerUser == 0 ? 3 : listsPerUser;
                if (listsPerUser <= 0 || listsPerUser > 20)
                    return TypedResults.BadRequest(Problem("listsPerUser must be between 1 and 20."));

                var result = await seeder.SeedAsync(new SmartListSeedOptions
                {
                    ListsPerUser = listsPerUser,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedSmartLists")
            .WithSummary("Seed the Smart Views module (Smart Lists).")
            .Produces<SmartListSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 15: Templates ----
            group.MapPost("/templates", async Task<Results<Ok<TemplateSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] ITemplateSeeder seeder,
                [FromQuery] int templatesPerUser,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                templatesPerUser = templatesPerUser == 0 ? 2 : templatesPerUser;
                if (templatesPerUser <= 0 || templatesPerUser > 20)
                    return TypedResults.BadRequest(Problem("templatesPerUser must be between 1 and 20."));

                var result = await seeder.SeedAsync(new TemplateSeedOptions
                {
                    TemplatesPerUser = templatesPerUser,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedTemplates")
            .WithSummary("Seed the Templates module.")
            .Produces<TemplateSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 16: Communication ----
            group.MapPost("/communications", async Task<Results<Ok<CommunicationSeedResult>, BadRequest<ProblemDetails>>> (
                [FromServices] ICommunicationSeeder seeder,
                [FromQuery] int batches,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                batches = batches == 0 ? 60 : batches;
                if (batches <= 0 || batches > 5000)
                    return TypedResults.BadRequest(Problem("batches must be between 1 and 5000."));

                var result = await seeder.SeedAsync(new CommunicationSeedOptions
                {
                    BatchCount = batches,
                    WipeExisting = wipeExisting
                }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedCommunications")
            .WithSummary("Seed the Communication module (Configuration, Batches, Delivery Results).")
            .Produces<CommunicationSeedResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            // ---- Module 8: Orchestrator (runs everything above, in order, with real IDs threaded through) ----
            group.MapPost("/all", async Task<Ok<SeedAllResult>> (
                [FromServices] ISeedOrchestrator orchestrator,
                [FromQuery] bool wipeExisting,
                CancellationToken ct) =>
            {
                var result = await orchestrator.SeedAllAsync(new SeedAllOptions { WipeExisting = wipeExisting }, ct);
                return TypedResults.Ok(result);
            })
            .WithName("SeedAll")
            .WithSummary("Seed every module in dependency order (Users -> Products -> Assets -> Leave -> Privileges -> Scheduler Jobs -> Workflows -> Todos -> Notes -> Events -> Reminders -> Tasks -> Smart Lists -> Templates -> Communication), threading real IDs through.")
            .Produces<SeedAllResult>(StatusCodes.Status200OK);

            return app;
        }

        private static ProblemDetails Problem(string detail) => new()
        {
            Title = "Invalid seed request",
            Detail = detail,
            Status = StatusCodes.Status400BadRequest
        };
    }
}

// Program.cs (or wherever you map endpoint groups):
//   app.MapSeedEndpoints();
