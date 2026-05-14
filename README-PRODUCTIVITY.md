# Productivity Module

The productivity module lives in `src/Modules/ProductivityModule` and provides modular todo, reminder, note, task, calendar, search, smart-view, template, reporting, and SignalR collaboration capabilities.

## Included capabilities

- `Todos`
  - create, update, complete, reopen, trash/restore
  - filtering, paging, tag-based search
  - convert todo to task
  - create a reminder from a todo
- `Reminders`
  - create, query, snooze, dismiss, test trigger
  - Hangfire-backed scheduling
- `Notes`
  - create, update, share, version history, export
  - notebook organization
  - convert note to todo
- `Tasks`
  - create, board view, status updates, dependency graph
  - time tracking
- `Calendar events`
  - create, calendar views, attendee responses
  - availability checks
  - meeting-time suggestions
  - iCal export
- `Cross-entity views`
  - global search
  - today dashboard
  - productivity report
  - smart lists and templates
- `Real-time collaboration`
  - SignalR hub at `/hubs/productivity`

## Setup instructions

1. Create and apply migrations after pulling the module changes:

   ```bash
   dotnet tool restore
   dotnet tool run dotnet-ef migrations add ProductivityInit --context AppDbContext --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
   dotnet tool run dotnet-ef database update --context AppDbContext --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
   ```

2. Confirm the new config sections exist in:

   - `src/Gateway/Alphabet.AppWire/appsettings.json`
   - `src/Gateway/Alphabet.AppWire/appsettings.Development.json`

3. Run the API:

   ```bash
   dotnet run --project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
   ```

## Background jobs

The module uses Hangfire through the existing scheduler stack.

Recurring jobs configured at startup:

- `productivity:recurring-task-generator`
- `productivity:trash-cleanup`
- `productivity:weekly-report`
- `productivity:calendar-sync`

Reminder scheduling uses one-off Hangfire jobs through `ReminderSchedulerService`.

## File storage

Attachment storage is configured through the `FileStorage` section.

Current baseline provider:

- `Local`

Config example:

```json
"FileStorage": {
  "Provider": "Local",
  "LocalPath": "C:\\ProductivityAttachments",
  "AzureConnectionString": "",
  "AWSBucketName": ""
}
```

The baseline implementation writes files to the local configured path. Cloud-provider fields are present so Azure Blob or S3 can be added without changing the application layer.

## Notification providers

Productivity delivery behavior is controlled through `NotificationSettings`.

Example:

```json
"NotificationSettings": {
  "EmailEnabled": true,
  "SmsEnabled": false,
  "PushEnabled": true,
  "FirebaseServerKey": "",
  "SendGridApiKey": ""
}
```

The productivity notification service currently delegates delivery through the shared communication module so the rest of the module stays transport-agnostic.

## SignalR integration

Hub route:

- `/hubs/productivity`

Client-callable hub methods:

- `JoinTodoGroup(todoId)`
- `SendTodoUpdate(todo)`
- `JoinNoteGroup(noteId)`
- `SendNoteDelta(noteId, delta)`

Typical client flow:

1. Authenticate using JWT or cookie auth.
2. Connect to `/hubs/productivity`.
3. Join the todo or note group you care about.
4. Listen for:
   - `TodoUpdated`
   - `NoteUpdated`

## Calendar integration

The module exposes:

- `GET /api/v1/events/export/ical`

That endpoint returns a baseline `.ics` file for the next month of calendar data.

The current implementation does not yet perform OAuth sync with Google or Outlook, but the background-job and settings structure is ready for that extension.

## Search configuration

Current note search uses EF Core `Contains`-based querying through `NoteSearchService`.

If you want richer search later:

1. keep `IProductivityReadService` stable
2. swap the internals to SQL Server full-text search or Elasticsearch
3. preserve the same API contracts

The `ProductivitySettings.FullTextSearchEnabled` flag is already present for that migration path.

## Performance guidance

Recommended indexes for SQL Server or PostgreSQL:

- todo owner + due date
- note owner + updated date
- reminder owner + reminder time + status
- task owner/assignee + status
- event owner + start/end range
- tag entity type + normalized name + entity id

Caching opportunities:

- dashboard snapshots
- task board views
- meeting-time suggestions
- recent note lists

## Mobile app considerations

For mobile clients:

- keep push enabled through `NotificationSettings.PushEnabled`
- use `/api/v1/dashboard/today` for compact home-screen hydration
- subscribe to `/hubs/productivity` after foreground resume
- keep reminder delivery server-driven rather than app-timer-driven

## Testing

Run the module-related test suites:

```bash
dotnet test tests/Alphabet.UnitTests/Alphabet.UnitTests.csproj
dotnet test tests/Alphabet.IntegrationTests/Alphabet.IntegrationTests.csproj
dotnet test tests/Alphabet.PerformanceTests/Alphabet.PerformanceTests.csproj
```
