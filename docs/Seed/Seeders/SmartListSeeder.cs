// Requires: dotnet add package Bogus
//
// USER REFERENCES: UserId is a random Guid unless you pass real ones via
// ExistingUserIds.

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
    public class SmartListSeedOptions
    {
        public int ListsPerUser { get; set; } = 3;
        public bool WipeExisting { get; set; } = false;
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
    }

    public class SmartListSeedResult
    {
        public int SmartLists { get; set; }
    }

    public interface ISmartListSeeder
    {
        Task<SmartListSeedResult> SeedAsync(SmartListSeedOptions options, CancellationToken ct = default);
    }

    public class SmartListSeeder : ISmartListSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        // Realistic (name, entityType, criteria) presets rather than random
        // strings — a "smart list" only makes sense as a meaningful saved
        // filter, so these mirror the kind of views a real user would save.
        private static readonly (string Name, string EntityType, string Criteria)[] Presets =
        {
            ("Overdue Todos", "Todo", "{\"status\":\"Overdue\"}"),
            ("My Open Tasks", "WorkTask", "{\"status\":[\"ToDo\",\"InProgress\"],\"assigneeId\":\"$currentUser\"}"),
            ("High Priority Items", "Todo", "{\"priority\":\"High\"}"),
            ("Assets Under Maintenance", "Asset", "{\"status\":\"UnderMaintenance\"}"),
            ("Pending Leave Approvals", "LeaveRequest", "{\"status\":\"Pending\"}"),
            ("Unread Notifications", "Notification", "{\"isRead\":false}"),
            ("This Week's Events", "Event", "{\"startTime\":{\"gte\":\"$startOfWeek\",\"lte\":\"$endOfWeek\"}}"),
            ("Failed Scheduler Jobs", "SchedulerJob", "{\"lastExecutionStatus\":\"Failed\"}"),
            ("My Pinned Notes", "Note", "{\"isPinned\":true}"),
            ("Access Requests Awaiting Decision", "PrivilegeAccessRequest", "{\"status\":\"Pending\"}"),
        };

        public SmartListSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<SmartListSeedResult> SeedAsync(SmartListSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var faker = new Faker();

            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, 15).Select(_ => Guid.NewGuid()).ToList();

            var smartLists = new List<SmartList>();
            foreach (var userId in userIds)
            {
                var chosen = Presets.OrderBy(_ => random.Next()).Take(Math.Min(options.ListsPerUser, Presets.Length));
                foreach (var preset in chosen)
                {
                    smartLists.Add(new SmartList
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Name = preset.Name,
                        EntityType = preset.EntityType,
                        CriteriaJson = preset.Criteria,
                        IsShared = random.NextDouble() < 0.15,
                        CreatedAt = faker.Date.Past(1)
                    });
                }
            }

            await _context.Set<SmartList>().AddRangeAsync(smartLists, ct);
            await _context.SaveChangesAsync(ct);

            return new SmartListSeedResult { SmartLists = smartLists.Count };
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<SmartList>().RemoveRange(_context.Set<SmartList>());
            await _context.SaveChangesAsync(ct);
        }
    }
}
