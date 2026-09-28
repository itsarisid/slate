// Requires: dotnet add package Bogus
//
// Unlike every other module so far, this one seeds through UserManager /
// RoleManager rather than raw DbContext inserts — Identity needs proper
// password hashing, normalized email/username, and security stamps, which
// UserManager.CreateAsync handles for you. Don't set PasswordHash directly.
//
// Seeds, in order: Roles -> Users (+ role assignments) -> UserPreferences ->
// UserSessions -> UserAuditLog.
//
// IMPORTANT: this seeder returns the generated user IDs (and role IDs) in
// the result. Feed those into the ExistingUserIds / ExistingRoleIds options
// on the Assets/Leave/Privileges/SchedulerJobs/Workflows seeders from
// earlier so everything points at real users instead of random Guids.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Alphabet.Domain.Entities;

namespace Alphabet.Infrastructure.Data.Seed
{
    public class IdentitySeedOptions
    {
        public int UserCount { get; set; } = 25;
        /// <summary>Plain-text password applied to every seeded user (never do this outside seed data).</summary>
        public string DefaultPassword { get; set; } = "Seed@12345";
        public bool WipeExisting { get; set; } = false;
    }

    public class IdentitySeedResult
    {
        public int Roles { get; set; }
        public int Users { get; set; }
        public int Preferences { get; set; }
        public int Sessions { get; set; }
        public int AuditLogEntries { get; set; }
        public List<Guid> UserIds { get; set; } = new();
        public List<Guid> RoleIds { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public interface IIdentitySeeder
    {
        Task<IdentitySeedResult> SeedAsync(IdentitySeedOptions options, CancellationToken ct = default);
    }

    public class IdentitySeeder : IIdentitySeeder
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly DbContext _context; // replace with your actual AppDbContext type, for the non-Identity tables

        // Role set chosen to match the roles referenced by the Privileges and
        // Workflows seeders from earlier modules (RolePrivilegeAssignment,
        // WorkflowDefinitionStep.AssignedToRole), so the two line up.
        private static readonly string[] RoleNames =
        {
            "Admin", "Manager", "Employee", "HR", "Finance", "IT",
            "Security", "Procurement", "Legal", "Executive", "TeamLead"
        };

        private static readonly string[] Departments =
        {
            "Engineering", "Human Resources", "Finance", "IT Support", "Operations",
            "Sales", "Marketing", "Legal", "Procurement", "Executive"
        };

        public IdentitySeeder(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, DbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IdentitySeedResult> SeedAsync(IdentitySeedOptions options, CancellationToken ct = default)
        {
            var result = new IdentitySeedResult();

            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var faker = new Faker();

            // ---- Roles ----
            var roleIds = new List<Guid>();
            foreach (var roleName in RoleNames)
            {
                var existing = await _roleManager.FindByNameAsync(roleName);
                if (existing != null)
                {
                    roleIds.Add(existing.Id);
                    continue;
                }

                var role = new AppRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    Description = $"{roleName} role."
                };
                var roleCreateResult = await _roleManager.CreateAsync(role);
                if (roleCreateResult.Succeeded)
                    roleIds.Add(role.Id);
                else
                    result.Errors.AddRange(roleCreateResult.Errors.Select(e => $"Role '{roleName}': {e.Description}"));
            }
            result.Roles = roleIds.Count;

            // ---- Users ----
            var userIds = new List<Guid>();
            for (int i = 0; i < options.UserCount; i++)
            {
                var firstName = faker.Name.FirstName();
                var lastName = faker.Name.LastName();
                var email = faker.Internet.Email(firstName, lastName, "alphabet-demo.local").ToLowerInvariant();

                var user = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    PhoneNumber = faker.Phone.PhoneNumber(),
                    FirstName = firstName,
                    LastName = lastName,
                    Bio = random.NextDouble() < 0.5 ? faker.Lorem.Sentence() : null,
                    Department = faker.PickRandom(Departments),
                    Location = faker.Address.City(),
                    AvatarUrl = random.NextDouble() < 0.6 ? faker.Internet.Avatar() : null,
                    CreatedAt = faker.Date.Past(2),
                    LastLoginAt = random.NextDouble() < 0.8 ? faker.Date.Recent(30) : null,
                    LockoutEnabled = true
                };

                var createResult = await _userManager.CreateAsync(user, options.DefaultPassword);
                if (!createResult.Succeeded)
                {
                    result.Errors.AddRange(createResult.Errors.Select(e => $"User '{email}': {e.Description}"));
                    continue;
                }

                // First seeded user gets Admin; rest get 1-2 random roles.
                var assignedRoles = i == 0
                    ? new List<string> { "Admin" }
                    : RoleNames.Where(r => r != "Admin").OrderBy(_ => random.Next()).Take(random.Next(1, 3)).ToList();

                var roleAssignResult = await _userManager.AddToRolesAsync(user, assignedRoles);
                if (!roleAssignResult.Succeeded)
                    result.Errors.AddRange(roleAssignResult.Errors.Select(e => $"User '{email}' roles: {e.Description}"));

                userIds.Add(user.Id);
            }
            result.Users = userIds.Count;
            result.UserIds = userIds;
            result.RoleIds = roleIds;

            // ---- Preferences ----
            var preferences = userIds.Select(userId => new UserPreferences
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Theme = faker.PickRandom("light", "dark", "system"),
                EmailNotifications = faker.Random.Bool(0.8f),
                PushNotifications = faker.Random.Bool(0.6f),
                Timezone = faker.PickRandom("UTC", "Asia/Qatar", "Europe/London", "America/New_York", "Asia/Kolkata")
            }).ToList();
            await _context.Set<UserPreferences>().AddRangeAsync(preferences, ct);
            await _context.SaveChangesAsync(ct);
            result.Preferences = preferences.Count;

            // ---- Sessions (1-3 per user, mostly active, some revoked) ----
            var sessions = new List<UserSession>();
            foreach (var userId in userIds)
            {
                var sessionCount = random.Next(1, 4);
                for (int s = 0; s < sessionCount; s++)
                {
                    var createdAt = faker.Date.Recent(30);
                    var isLast = s == sessionCount - 1;
                    sessions.Add(new UserSession
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        CreatedAt = createdAt,
                        ExpiresAt = createdAt.AddDays(30),
                        IpAddress = faker.Internet.Ip(),
                        UserAgent = faker.Internet.UserAgent(),
                        IsRevoked = !isLast && random.NextDouble() < 0.5
                    });
                }
            }
            await _context.Set<UserSession>().AddRangeAsync(sessions, ct);
            await _context.SaveChangesAsync(ct);
            result.Sessions = sessions.Count;

            // ---- Audit log (admin actions) ----
            var auditActions = new[] { "Created", "Locked", "Unlocked", "PasswordReset", "ForceLogout", "Login", "Logout" };
            var auditLogs = new List<UserAuditLog>();
            foreach (var userId in userIds)
            {
                var entryCount = random.Next(1, 5);
                for (int a = 0; a < entryCount; a++)
                {
                    auditLogs.Add(new UserAuditLog
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Action = a == 0 ? "Created" : faker.PickRandom(auditActions),
                        PerformedBy = a == 0 ? "seed" : faker.PickRandom("seed", "self", "admin"),
                        PerformedAt = faker.Date.Past(1),
                        IpAddress = faker.Internet.Ip(),
                        Details = new Dictionary<string, string?>()
                    });
                }
            }
            await _context.Set<UserAuditLog>().AddRangeAsync(auditLogs, ct);
            await _context.SaveChangesAsync(ct);
            result.AuditLogEntries = auditLogs.Count;

            return result;
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<UserAuditLog>().RemoveRange(_context.Set<UserAuditLog>());
            _context.Set<UserSession>().RemoveRange(_context.Set<UserSession>());
            _context.Set<UserPreferences>().RemoveRange(_context.Set<UserPreferences>());
            await _context.SaveChangesAsync(ct);

            // Delete only users/roles this seeder is responsible for (by convention,
            // seeded emails end with @alphabet-demo.local) rather than nuking every
            // user in the system.
            var seededUsers = await _userManager.Users
                .Where(u => u.Email != null && u.Email.EndsWith("@alphabet-demo.local"))
                .ToListAsync(ct);
            foreach (var user in seededUsers)
                await _userManager.DeleteAsync(user);

            // Roles are left in place intentionally (other seeded data may still
            // reference role names). Delete manually if you want a full reset.
        }
    }
}
