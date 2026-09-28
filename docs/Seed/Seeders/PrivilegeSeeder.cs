// Requires: dotnet add package Bogus
//
// Seeds, in dependency order:
//   PrivilegeCategories -> Privileges -> PrivilegePolicies ->
//   UserPrivilegeAssignments -> RolePrivilegeAssignments ->
//   PrivilegeAccessRequests -> PrivilegeAuditLog
//
// USER/ROLE REFERENCES: UserId/RoleId are random Guids unless you pass real
// ones via ExistingUserIds / ExistingRoleIds.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Alphabet.Domain.Entities;

namespace Alphabet.Infrastructure.Data.Seed
{
    public class PrivilegeSeedOptions
    {
        public int UserCount { get; set; } = 20;
        public int RoleCount { get; set; } = 5;
        public int PolicyCount { get; set; } = 6;
        public int AssignmentsPerUser { get; set; } = 4;
        public int AccessRequestCount { get; set; } = 15;
        public int AuditLogCount { get; set; } = 100;
        public bool WipeExisting { get; set; } = false;
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
        public IReadOnlyList<Guid> ExistingRoleIds { get; set; } = Array.Empty<Guid>();
    }

    public class PrivilegeSeedResult
    {
        public int Categories { get; set; }
        public int Privileges { get; set; }
        public int Policies { get; set; }
        public int UserAssignments { get; set; }
        public int RoleAssignments { get; set; }
        public int AccessRequests { get; set; }
        public int AuditLogEntries { get; set; }
    }

    public interface IPrivilegeSeeder
    {
        Task<PrivilegeSeedResult> SeedAsync(PrivilegeSeedOptions options, CancellationToken ct = default);
    }

    public class PrivilegeSeeder : IPrivilegeSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        // Realistic module/resource pairing, mirroring the modules in this API
        // (Assets, Leave, Privileges, Scheduler Jobs, Workflows, plus core CRM-ish areas).
        private static readonly string[] ResourceTypes =
        {
            "Asset", "AssetCategory", "LeaveRequest", "LeaveType", "Privilege",
            "SchedulerJob", "WorkflowDefinition", "User", "Report", "Product",
            "Todo", "Note", "Notification"
        };

        private static readonly (string Action, PrivilegeAction Value)[] ActionDefs =
        {
            ("create", PrivilegeAction.Create),
            ("read", PrivilegeAction.Read),
            ("update", PrivilegeAction.Update),
            ("delete", PrivilegeAction.Delete),
            ("approve", PrivilegeAction.Approve),
            ("export", PrivilegeAction.Export),
            ("assign", PrivilegeAction.Assign),
            ("manage", PrivilegeAction.Manage),
        };

        public PrivilegeSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<PrivilegeSeedResult> SeedAsync(PrivilegeSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var faker = new Faker();

            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, options.UserCount).Select(_ => Guid.NewGuid()).ToList();

            var roleIds = options.ExistingRoleIds.Count > 0
                ? options.ExistingRoleIds
                : Enumerable.Range(0, options.RoleCount).Select(_ => Guid.NewGuid()).ToList();

            // ---- Categories (one per resource "group", flat for simplicity) ----
            var categories = ResourceTypes.Select(r => new PrivilegeCategory
            {
                Id = Guid.NewGuid(),
                Name = r,
                Description = $"Privileges related to {r}.",
                SortOrder = Array.IndexOf(ResourceTypes, r)
            }).ToList();
            await _context.Set<PrivilegeCategory>().AddRangeAsync(categories, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Privileges: 2-4 actions per resource type ----
            var privileges = new List<Privilege>();
            foreach (var resource in ResourceTypes)
            {
                var category = categories.First(c => c.Name == resource);
                var actionsForResource = ActionDefs.OrderBy(_ => random.Next()).Take(random.Next(2, 5)).ToList();

                foreach (var (actionName, actionValue) in actionsForResource)
                {
                    var name = $"{resource.ToLowerInvariant()}.{actionName}";
                    privileges.Add(new Privilege
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        DisplayName = $"{Capitalize(actionName)} {resource}",
                        Description = $"Allows the user to {actionName} {resource} records.",
                        CategoryId = category.Id,
                        ResourceType = resource,
                        Actions = new List<string> { actionName },
                        IsGlobal = actionValue is PrivilegeAction.Manage or PrivilegeAction.Approve && random.NextDouble() < 0.3,
                        IsDeprecated = false,
                        DependsOn = actionValue == PrivilegeAction.Delete
                            ? new List<string> { $"{resource.ToLowerInvariant()}.update" }
                            : new List<string>(),
                        Attributes = new Dictionary<string, string?>(),
                        CreatedAt = faker.Date.Past(1),
                        CreatedBy = "seed",
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
            await _context.Set<Privilege>().AddRangeAsync(privileges, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Policies (bundles of privilege names) ----
            var policies = new List<PrivilegePolicy>();
            for (int i = 0; i < options.PolicyCount; i++)
            {
                var bundleSize = random.Next(2, 6);
                var bundle = privileges.OrderBy(_ => random.Next()).Take(bundleSize).Select(p => p.Name).ToList();
                policies.Add(new PrivilegePolicy
                {
                    Id = Guid.NewGuid(),
                    Name = faker.PickRandom("Read-Only Auditor", "Asset Manager", "Leave Approver",
                        "Report Viewer", "Workflow Admin", "Scheduler Operator", "Full Access") + $" #{i + 1}",
                    Description = faker.Lorem.Sentence(),
                    PrivilegeNames = bundle,
                    Condition = faker.PickRandom<PrivilegePolicyCondition>()
                });
            }
            await _context.Set<PrivilegePolicy>().AddRangeAsync(policies, ct);
            await _context.SaveChangesAsync(ct);

            // ---- User privilege assignments (direct grants/denies) ----
            var userAssignments = new List<UserPrivilegeAssignment>();
            foreach (var userId in userIds)
            {
                var granted = privileges.OrderBy(_ => random.Next()).Take(options.AssignmentsPerUser).ToList();
                foreach (var priv in granted)
                {
                    userAssignments.Add(new UserPrivilegeAssignment
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        PrivilegeId = priv.Id,
                        Effect = random.NextDouble() < 0.9 ? PrivilegeEffect.Allow : PrivilegeEffect.Deny,
                        AssignmentSource = "Direct",
                        GrantedAt = faker.Date.Past(1),
                        GrantedBy = "seed",
                        ExpiresAt = random.NextDouble() < 0.2 ? DateTime.UtcNow.AddMonths(random.Next(1, 12)) : null,
                        Reason = random.NextDouble() < 0.3 ? faker.Lorem.Sentence() : null,
                        IsActive = true
                    });
                }
            }
            await _context.Set<UserPrivilegeAssignment>().AddRangeAsync(userAssignments, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Role privilege assignments ----
            var roleAssignments = new List<RolePrivilegeAssignment>();
            foreach (var roleId in roleIds)
            {
                var granted = privileges.OrderBy(_ => random.Next()).Take(random.Next(3, 10)).ToList();
                foreach (var priv in granted)
                {
                    roleAssignments.Add(new RolePrivilegeAssignment
                    {
                        Id = Guid.NewGuid(),
                        RoleId = roleId,
                        PrivilegeId = priv.Id,
                        GrantedAt = faker.Date.Past(1),
                        ExpiresAt = null,
                        IsActive = true
                    });
                }
            }
            await _context.Set<RolePrivilegeAssignment>().AddRangeAsync(roleAssignments, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Access requests (self-service, time-bound access) ----
            var accessRequests = new List<PrivilegeAccessRequest>();
            for (int i = 0; i < options.AccessRequestCount; i++)
            {
                var requestedAt = faker.Date.Past(1);
                var status = faker.PickRandom<PrivilegeAccessRequestStatus>();
                var request = new PrivilegeAccessRequest
                {
                    Id = Guid.NewGuid(),
                    UserId = userIds[random.Next(userIds.Count)],
                    PrivilegeId = privileges[random.Next(privileges.Count)].Id,
                    Reason = faker.Lorem.Sentence(8),
                    RequestedDurationDays = faker.PickRandom(1, 7, 14, 30, 90),
                    ApproverEmail = faker.Internet.Email(),
                    Status = status,
                    RequestedAt = requestedAt,
                };
                if (status != PrivilegeAccessRequestStatus.Pending)
                {
                    request.DecidedAt = requestedAt.AddDays(random.Next(1, 5));
                    request.DecisionNotes = faker.Lorem.Sentence();
                }
                accessRequests.Add(request);
            }
            await _context.Set<PrivilegeAccessRequest>().AddRangeAsync(accessRequests, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Audit log ----
            var auditActions = new[] { "Granted", "Revoked", "Checked", "Denied", "RequestApproved", "RequestRejected" };
            var auditLogs = new Faker<PrivilegeAuditLog>()
                .RuleFor(a => a.Id, _ => Guid.NewGuid())
                .RuleFor(a => a.UserId, f => f.PickRandom(userIds))
                .RuleFor(a => a.PrivilegeId, f => f.PickRandom(privileges).Id)
                .RuleFor(a => a.Action, f => f.PickRandom(auditActions))
                .RuleFor(a => a.Source, f => f.PickRandom("Direct", "Role", "Policy", "AccessRequest"))
                .RuleFor(a => a.PerformedBy, f => f.Internet.Email())
                .RuleFor(a => a.PerformedAt, f => f.Date.Past(1))
                .RuleFor(a => a.IpAddress, f => f.Internet.Ip())
                .Generate(options.AuditLogCount);

            await _context.Set<PrivilegeAuditLog>().AddRangeAsync(auditLogs, ct);
            await _context.SaveChangesAsync(ct);

            return new PrivilegeSeedResult
            {
                Categories = categories.Count,
                Privileges = privileges.Count,
                Policies = policies.Count,
                UserAssignments = userAssignments.Count,
                RoleAssignments = roleAssignments.Count,
                AccessRequests = accessRequests.Count,
                AuditLogEntries = auditLogs.Count
            };
        }

        private static string Capitalize(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<PrivilegeAuditLog>().RemoveRange(_context.Set<PrivilegeAuditLog>());
            _context.Set<PrivilegeAccessRequest>().RemoveRange(_context.Set<PrivilegeAccessRequest>());
            _context.Set<RolePrivilegeAssignment>().RemoveRange(_context.Set<RolePrivilegeAssignment>());
            _context.Set<UserPrivilegeAssignment>().RemoveRange(_context.Set<UserPrivilegeAssignment>());
            _context.Set<PrivilegePolicy>().RemoveRange(_context.Set<PrivilegePolicy>());
            _context.Set<Privilege>().RemoveRange(_context.Set<Privilege>());
            _context.Set<PrivilegeCategory>().RemoveRange(_context.Set<PrivilegeCategory>());
            await _context.SaveChangesAsync(ct);
        }
    }
}
