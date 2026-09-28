// Runs every module seeder from Modules 1-7 and 9-16 in dependency order,
// passing real generated IDs from earlier steps into later ones instead of
// each seeder's random-Guid fallback.
//
// This REPLACES the SeedOrchestrator.cs from Module 8's first version — it
// adds Todos, Notes, Events, Reminders, Tasks, Smart Lists, Templates, and
// Communication to the original Users/Products/Assets/Leave/Privileges/
// SchedulerJobs/Workflows chain.
//
// Order:
//   Users/Roles (7) -> Products (1) -> Assets (2) -> Leave (3) ->
//   Privileges (4) -> Scheduler Jobs (5) -> Workflows (6) -> Todos (9) ->
//   Notes (10) -> Events (11) -> Reminders (12) -> Tasks (13) ->
//   Smart Lists (14) -> Templates (15) -> Communication (16)
//
// PREREQUISITE PATCHES: three seed results need a small addition so their
// generated IDs can be threaded into later steps. See README.md "Patches
// needed" — same two-line shape for each:
//   - SchedulerJobSeedResult.JobIds        (Module 5)
//   - TodoSeedResult.TodoIds               (Module 9)
//   - EventSeedResult.EventIds             (Module 11)
// Until patched, the affected wiring below just passes an empty list, and
// those seeders fall back to their own random-Guid placeholders (same as
// running them standalone) — nothing breaks, you just get looser linkage.

using System.Threading;
using System.Threading.Tasks;

namespace Alphabet.Infrastructure.Data.Seed
{
    public class SeedAllOptions
    {
        public bool WipeExisting { get; set; } = false;

        public int UserCount { get; set; } = 25;
        public string DefaultPassword { get; set; } = "Seed@12345";

        public int ProductCount { get; set; } = 50;

        public int LocationCount { get; set; } = 5;
        public int AssetCategoryCount { get; set; } = 8;
        public int AssetCount { get; set; } = 100;

        public int LeaveRequestsPerUser { get; set; } = 3;
        public int PublicHolidayCount { get; set; } = 10;
        public int BlackoutPeriodCount { get; set; } = 2;
        public int LeaveDelegationCount { get; set; } = 5;

        public int PrivilegePolicyCount { get; set; } = 6;
        public int PrivilegeAssignmentsPerUser { get; set; } = 4;
        public int PrivilegeAccessRequestCount { get; set; } = 15;
        public int PrivilegeAuditLogCount { get; set; } = 100;

        public int SchedulerJobCount { get; set; } = 25;
        public int MinExecutionsPerJob { get; set; } = 3;
        public int MaxExecutionsPerJob { get; set; } = 15;
        public int JobDependencyCount { get; set; } = 8;
        public int JobExclusionCount { get; set; } = 5;

        public int WorkflowDefinitionCount { get; set; } = 6;
        public int WorkflowInstancesPerDefinition { get; set; } = 5;
        public int SchedulerWorkflowCount { get; set; } = 4;
        public int MinJobsPerSchedulerWorkflow { get; set; } = 2;
        public int MaxJobsPerSchedulerWorkflow { get; set; } = 6;

        public int TodosPerUser { get; set; } = 8;

        public int NotebooksPerUser { get; set; } = 2;
        public int NotesPerUser { get; set; } = 10;

        public int EventsPerUser { get; set; } = 6;

        public int RemindersPerUser { get; set; } = 5;

        public int TaskCount { get; set; } = 40;
        public double TaskSubtaskRatio { get; set; } = 0.25;
        public int TaskDependencyCount { get; set; } = 15;

        public int SmartListsPerUser { get; set; } = 3;

        public int TemplatesPerUser { get; set; } = 2;

        public int CommunicationBatchCount { get; set; } = 60;
    }

    public class SeedAllResult
    {
        public IdentitySeedResult? Identity { get; set; }
        public int ProductsSeeded { get; set; }
        public AssetSeedResult? Assets { get; set; }
        public LeaveSeedResult? Leave { get; set; }
        public PrivilegeSeedResult? Privileges { get; set; }
        public SchedulerJobSeedResult? SchedulerJobs { get; set; }
        public WorkflowSeedResult? Workflows { get; set; }
        public TodoSeedResult? Todos { get; set; }
        public NoteSeedResult? Notes { get; set; }
        public EventSeedResult? Events { get; set; }
        public ReminderSeedResult? Reminders { get; set; }
        public WorkTaskSeedResult? Tasks { get; set; }
        public SmartListSeedResult? SmartLists { get; set; }
        public TemplateSeedResult? Templates { get; set; }
        public CommunicationSeedResult? Communications { get; set; }
    }

    public interface ISeedOrchestrator
    {
        Task<SeedAllResult> SeedAllAsync(SeedAllOptions options, CancellationToken ct = default);
    }

    public class SeedOrchestrator : ISeedOrchestrator
    {
        private readonly IIdentitySeeder _identitySeeder;
        private readonly IProductSeeder _productSeeder;
        private readonly IAssetSeeder _assetSeeder;
        private readonly ILeaveSeeder _leaveSeeder;
        private readonly IPrivilegeSeeder _privilegeSeeder;
        private readonly ISchedulerJobSeeder _schedulerJobSeeder;
        private readonly IWorkflowSeeder _workflowSeeder;
        private readonly ITodoSeeder _todoSeeder;
        private readonly INoteSeeder _noteSeeder;
        private readonly IEventSeeder _eventSeeder;
        private readonly IReminderSeeder _reminderSeeder;
        private readonly IWorkTaskSeeder _workTaskSeeder;
        private readonly ISmartListSeeder _smartListSeeder;
        private readonly ITemplateSeeder _templateSeeder;
        private readonly ICommunicationSeeder _communicationSeeder;

        public SeedOrchestrator(
            IIdentitySeeder identitySeeder,
            IProductSeeder productSeeder,
            IAssetSeeder assetSeeder,
            ILeaveSeeder leaveSeeder,
            IPrivilegeSeeder privilegeSeeder,
            ISchedulerJobSeeder schedulerJobSeeder,
            IWorkflowSeeder workflowSeeder,
            ITodoSeeder todoSeeder,
            INoteSeeder noteSeeder,
            IEventSeeder eventSeeder,
            IReminderSeeder reminderSeeder,
            IWorkTaskSeeder workTaskSeeder,
            ISmartListSeeder smartListSeeder,
            ITemplateSeeder templateSeeder,
            ICommunicationSeeder communicationSeeder)
        {
            _identitySeeder = identitySeeder;
            _productSeeder = productSeeder;
            _assetSeeder = assetSeeder;
            _leaveSeeder = leaveSeeder;
            _privilegeSeeder = privilegeSeeder;
            _schedulerJobSeeder = schedulerJobSeeder;
            _workflowSeeder = workflowSeeder;
            _todoSeeder = todoSeeder;
            _noteSeeder = noteSeeder;
            _eventSeeder = eventSeeder;
            _reminderSeeder = reminderSeeder;
            _workTaskSeeder = workTaskSeeder;
            _smartListSeeder = smartListSeeder;
            _templateSeeder = templateSeeder;
            _communicationSeeder = communicationSeeder;
        }

        public async Task<SeedAllResult> SeedAllAsync(SeedAllOptions options, CancellationToken ct = default)
        {
            var result = new SeedAllResult();

            // 1. Users/Roles first — everything else references these.
            var identity = await _identitySeeder.SeedAsync(new IdentitySeedOptions
            {
                UserCount = options.UserCount,
                DefaultPassword = options.DefaultPassword,
                WipeExisting = options.WipeExisting
            }, ct);
            result.Identity = identity;

            var userIds = identity.UserIds;
            var roleIds = identity.RoleIds;

            // 2. Products — independent of users.
            result.ProductsSeeded = await _productSeeder.SeedAsync(options.ProductCount, options.WipeExisting, ct);

            // 3. Assets
            var assets = await _assetSeeder.SeedAsync(new AssetSeedOptions
            {
                LocationCount = options.LocationCount,
                CategoryCount = options.AssetCategoryCount,
                AssetCount = options.AssetCount,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds
            }, ct);
            result.Assets = assets;

            // 4. Leave
            var leave = await _leaveSeeder.SeedAsync(new LeaveSeedOptions
            {
                UserCount = options.UserCount,
                RequestsPerUser = options.LeaveRequestsPerUser,
                PublicHolidayCount = options.PublicHolidayCount,
                BlackoutPeriodCount = options.BlackoutPeriodCount,
                DelegationCount = options.LeaveDelegationCount,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds
            }, ct);
            result.Leave = leave;

            // 5. Privileges
            var privileges = await _privilegeSeeder.SeedAsync(new PrivilegeSeedOptions
            {
                UserCount = options.UserCount,
                RoleCount = roleIds.Count,
                PolicyCount = options.PrivilegePolicyCount,
                AssignmentsPerUser = options.PrivilegeAssignmentsPerUser,
                AccessRequestCount = options.PrivilegeAccessRequestCount,
                AuditLogCount = options.PrivilegeAuditLogCount,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds,
                ExistingRoleIds = roleIds
            }, ct);
            result.Privileges = privileges;

            // 6. Scheduler Jobs
            var schedulerJobs = await _schedulerJobSeeder.SeedAsync(new SchedulerJobSeedOptions
            {
                JobCount = options.SchedulerJobCount,
                MinExecutionsPerJob = options.MinExecutionsPerJob,
                MaxExecutionsPerJob = options.MaxExecutionsPerJob,
                DependencyCount = options.JobDependencyCount,
                ExclusionCount = options.JobExclusionCount,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds
            }, ct);
            result.SchedulerJobs = schedulerJobs;

            // 7. Workflows — needs job ids from step 6 (requires the Module 5 patch).
            var workflows = await _workflowSeeder.SeedAsync(new WorkflowSeedOptions
            {
                DefinitionCount = options.WorkflowDefinitionCount,
                InstancesPerDefinition = options.WorkflowInstancesPerDefinition,
                SchedulerWorkflowCount = options.SchedulerWorkflowCount,
                MinJobsPerSchedulerWorkflow = options.MinJobsPerSchedulerWorkflow,
                MaxJobsPerSchedulerWorkflow = options.MaxJobsPerSchedulerWorkflow,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds,
                ExistingSchedulerJobIds = schedulerJobs.JobIds // requires Module 5 patch
            }, ct);
            result.Workflows = workflows;

            // 8. Todos
            var todos = await _todoSeeder.SeedAsync(new TodoSeedOptions
            {
                TodosPerUser = options.TodosPerUser,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds
            }, ct);
            result.Todos = todos;

            // 9. Notes
            var notes = await _noteSeeder.SeedAsync(new NoteSeedOptions
            {
                NotebooksPerUser = options.NotebooksPerUser,
                NotesPerUser = options.NotesPerUser,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds
            }, ct);
            result.Notes = notes;

            // 10. Events
            var events = await _eventSeeder.SeedAsync(new EventSeedOptions
            {
                EventsPerUser = options.EventsPerUser,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds
            }, ct);
            result.Events = events;

            // 11. Reminders — can link to Todos/Events if those seeders expose
            //     their generated ids (requires the Module 9 / Module 11 patches).
            var reminders = await _reminderSeeder.SeedAsync(new ReminderSeedOptions
            {
                RemindersPerUser = options.RemindersPerUser,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds,
                ExistingTodoIds = todos.TodoIds,   // requires Module 9 patch
                ExistingEventIds = events.EventIds // requires Module 11 patch
            }, ct);
            result.Reminders = reminders;

            // 12. Tasks
            var tasks = await _workTaskSeeder.SeedAsync(new WorkTaskSeedOptions
            {
                TaskCount = options.TaskCount,
                SubtaskRatio = options.TaskSubtaskRatio,
                DependencyCount = options.TaskDependencyCount,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds
            }, ct);
            result.Tasks = tasks;

            // 13. Smart Lists
            var smartLists = await _smartListSeeder.SeedAsync(new SmartListSeedOptions
            {
                ListsPerUser = options.SmartListsPerUser,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds
            }, ct);
            result.SmartLists = smartLists;

            // 14. Templates
            var templates = await _templateSeeder.SeedAsync(new TemplateSeedOptions
            {
                TemplatesPerUser = options.TemplatesPerUser,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds
            }, ct);
            result.Templates = templates;

            // 15. Communication
            var communications = await _communicationSeeder.SeedAsync(new CommunicationSeedOptions
            {
                BatchCount = options.CommunicationBatchCount,
                WipeExisting = options.WipeExisting,
                ExistingUserIds = userIds
            }, ct);
            result.Communications = communications;

            return result;
        }
    }
}
