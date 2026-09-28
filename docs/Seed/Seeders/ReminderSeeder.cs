// Requires: dotnet add package Bogus
//
// USER REFERENCES: UserId is a random Guid unless you pass real ones via
// ExistingUserIds. LINKED ENTITY REFERENCES: LinkedEntityId is a random Guid
// placeholder unless you pass real Todo/Event ids via ExistingTodoIds /
// ExistingEventIds (from Modules 9 and 11).

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
    public class ReminderSeedOptions
    {
        public int RemindersPerUser { get; set; } = 5;
        public bool WipeExisting { get; set; } = false;
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
        public IReadOnlyList<Guid> ExistingTodoIds { get; set; } = Array.Empty<Guid>();
        public IReadOnlyList<Guid> ExistingEventIds { get; set; } = Array.Empty<Guid>();
    }

    public class ReminderSeedResult
    {
        public int Reminders { get; set; }
    }

    public interface IReminderSeeder
    {
        Task<ReminderSeedResult> SeedAsync(ReminderSeedOptions options, CancellationToken ct = default);
    }

    public class ReminderSeeder : IReminderSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        private static readonly string[] TitlePool =
        {
            "Submit expense report", "Follow up with candidate", "Renew asset warranty",
            "Prepare for client call", "Review pending access requests", "Pay vendor invoice",
            "Send weekly status update", "Book flight for offsite", "Check backup completion",
            "Approve leave request", "Update project timeline", "Call IT about laptop issue"
        };

        public ReminderSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<ReminderSeedResult> SeedAsync(ReminderSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var faker = new Faker();

            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, 15).Select(_ => Guid.NewGuid()).ToList();

            var reminders = new List<Reminder>();

            foreach (var userId in userIds)
            {
                for (int i = 0; i < options.RemindersPerUser; i++)
                {
                    var reminderTime = faker.Date.Between(DateTime.UtcNow.AddDays(-14), DateTime.UtcNow.AddDays(21));
                    var isPast = reminderTime < DateTime.UtcNow;

                    ReminderStatus status;
                    DateTime? snoozedUntil = null;
                    DateTime? dismissedAt = null;

                    if (isPast)
                    {
                        var roll = random.NextDouble();
                        if (roll < 0.5) { status = ReminderStatus.Dismissed; dismissedAt = reminderTime.AddMinutes(random.Next(1, 120)); }
                        else if (roll < 0.7) { status = ReminderStatus.Triggered; }
                        else if (roll < 0.85)
                        {
                            status = ReminderStatus.Snoozed;
                            snoozedUntil = reminderTime.AddMinutes(faker.PickRandom(10, 15, 30, 60));
                        }
                        else { status = ReminderStatus.Cancelled; }
                    }
                    else
                    {
                        status = random.NextDouble() < 0.1 ? ReminderStatus.Cancelled : ReminderStatus.Pending;
                    }

                    var linkedType = faker.PickRandom<ReminderType>();
                    Guid? linkedEntityId = linkedType switch
                    {
                        ReminderType.Todo when options.ExistingTodoIds.Count > 0 => faker.PickRandom(options.ExistingTodoIds),
                        ReminderType.Event when options.ExistingEventIds.Count > 0 => faker.PickRandom(options.ExistingEventIds),
                        ReminderType.Todo or ReminderType.Event or ReminderType.Task or ReminderType.LeaveRequest => Guid.NewGuid(),
                        _ => null
                    };

                    var snoozeEnabled = random.NextDouble() < 0.8;

                    reminders.Add(new Reminder
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Title = faker.PickRandom(TitlePool),
                        Description = random.NextDouble() < 0.4 ? faker.Lorem.Sentence() : null,
                        ReminderTime = reminderTime,
                        ReminderType = linkedType,
                        Status = status,
                        RepeatInterval = random.NextDouble() < 0.2 ? faker.PickRandom(1, 7, 30) : null,
                        RepeatCount = random.NextDouble() < 0.2 ? faker.Random.Int(1, 10) : null,
                        EndDate = random.NextDouble() < 0.15 ? reminderTime.AddMonths(random.Next(1, 6)) : null,
                        SoundEnabled = faker.Random.Bool(0.8f),
                        VibrationEnabled = faker.Random.Bool(0.6f),
                        SnoozeEnabled = snoozeEnabled,
                        SnoozeMinutes = snoozeEnabled ? faker.PickRandom(5, 10, 15, 30) : null,
                        LinkedEntityType = linkedType == ReminderType.Standalone || linkedType == ReminderType.Custom ? null : linkedType.ToString(),
                        LinkedEntityId = linkedEntityId,
                        NotificationMethods = faker.Random.ListItems(new List<string> { "Push", "Email", "SMS" }, random.Next(1, 3)),
                        RecurrencePattern = null,
                        SnoozedUntil = snoozedUntil,
                        DismissedAt = dismissedAt,
                        CreatedAt = reminderTime.AddDays(-random.Next(0, 5))
                    });
                }
            }

            await _context.Set<Reminder>().AddRangeAsync(reminders, ct);
            await _context.SaveChangesAsync(ct);

            return new ReminderSeedResult { Reminders = reminders.Count };
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<Reminder>().RemoveRange(_context.Set<Reminder>());
            await _context.SaveChangesAsync(ct);
        }
    }
}
