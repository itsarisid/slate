# Scheduler Module

The scheduler module adds Hangfire-first background job management to Alphabet. It supports HTTP calls, stored procedures, code execution, and file operations through a modular API surface under `/api/v1/scheduler`.

## Setup

1. Restore packages:
   `dotnet restore Alphabet.slnx`
2. Create or update the application schema:
   `dotnet tool run dotnet-ef migrations add SchedulerInit --context AppDbContext --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj`
   `dotnet tool run dotnet-ef database update --context AppDbContext --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj`
3. Run the API:
   `dotnet run --project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj`
4. Open the Hangfire dashboard at `/hangfire` with an authenticated admin account.

## Job types

### HTTP call jobs

Use `jobType: "HttpCall"` with `url`, `method`, optional headers, optional body, and timeout settings.

### Stored procedure jobs

Use `jobType: "StoredProcedure"` with `storedProcedureName`, optional parameters, and optional timeout settings.

### Code execution jobs

Use `jobType: "CodeExecution"` with `handlerType` that resolves to a registered `IJobHandler`.

### File operation jobs

Use `jobType: "FileOperation"` with `sourcePath` and an operation such as delete, move, archive, or compress. File operations are constrained by `Scheduler:Jobs:AllowedFileRoots`.

## Variable substitution

The scheduler resolves:

- `{{Today}}`
- `{{Now}}`
- `{{Job.Id}}`
- `{{Job.Name}}`
- `{{Environment.MachineName}}`
- `{{User.Email}}`

## Custom code execution jobs

1. Implement `Alphabet.Domain.Interfaces.IJobHandler`.
2. Register the type in DI.
3. Use the fully qualified type name in `jobConfiguration.handlerType`.

Example handlers:

- [SampleJob.cs](/C:/Users/moinc/OneDrive/Documents/New%20project/src/Modules/SchedulerModule/Infrastructure/Scheduler/ExampleJobs/SampleJob.cs)
- [ReportGenerationJob.cs](/C:/Users/moinc/OneDrive/Documents/New%20project/src/Modules/SchedulerModule/Infrastructure/Scheduler/ExampleJobs/ReportGenerationJob.cs)
- [CleanupJob.cs](/C:/Users/moinc/OneDrive/Documents/New%20project/src/Modules/SchedulerModule/Infrastructure/Scheduler/ExampleJobs/CleanupJob.cs)

## Monitoring

- Module endpoints: `/api/v1/scheduler`
- Dashboard stats: `/api/v1/scheduler/dashboard/stats`
- Failed jobs: `/api/v1/scheduler/jobs/failed`
- Timeline: `/api/v1/scheduler/executions/timeline`
- Hangfire dashboard: `/hangfire`

## Performance tuning

- Tune `Scheduler:Hangfire:WorkerCount`
- Separate traffic with `Scheduler:Hangfire:Queues`
- Periodically clear old execution logs through `/api/v1/scheduler/admin/clear-logs`

## Disaster recovery

- Export current job definitions from `/api/v1/scheduler/admin/export`
- Import them back with `/api/v1/scheduler/admin/import`
- Back up the primary application database because scheduler metadata and history are stored there

## Notes

- Hangfire is the default and recommended provider in this solution.
- Quartz is represented by the abstraction but currently returns a clear not-supported message until a dedicated implementation is added.
