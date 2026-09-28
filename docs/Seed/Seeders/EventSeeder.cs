// Requires: dotnet add package Bogus
//
// Seeds, in order: Events -> EventAttendees.
// USER REFERENCES: OrganizerUserId / attendee UserId are random Guids unless
// you pass real ones via ExistingUserIds; we also fabricate an email for
// each so the Attendee.Email field is always populated (as the API expects).

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
    public class EventSeedOptions
    {
        public int EventsPerUser { get; set; } = 6;
        public bool WipeExisting { get; set; } = false;
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
    }

    public class EventSeedResult
    {
        public int Events { get; set; }
        public int Attendees { get; set; }
    }

    public interface IEventSeeder
    {
        Task<EventSeedResult> SeedAsync(EventSeedOptions options, CancellationToken ct = default);
    }

    public class EventSeeder : IEventSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        private static readonly string[] TitlePool =
        {
            "Sprint Planning", "1:1 with Manager", "Vendor Kickoff Call", "All-Hands Meeting",
            "Budget Review", "Client Demo", "Design Review", "Interview: Backend Engineer",
            "Onboarding Session", "Quarterly Planning", "Team Standup", "Retro",
            "Offsite Planning", "Security Review", "Contract Renewal Discussion"
        };

        private static readonly string[] Locations =
        {
            "Conference Room A", "Conference Room B", "Main Office - 3rd Floor",
            "Zoom", "Microsoft Teams", "Client Office", null!
        };

        public EventSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<EventSeedResult> SeedAsync(EventSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var faker = new Faker();

            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, 15).Select(_ => Guid.NewGuid()).ToList();

            // Fabricated stand-in emails so EventAttendee.Email is always
            // populated; swap for real user emails if you have them handy.
            var userEmails = userIds.ToDictionary(id => id, _ => faker.Internet.Email());

            var events = new List<Event>();
            var attendees = new List<EventAttendee>();

            foreach (var organizerId in userIds)
            {
                for (int i = 0; i < options.EventsPerUser; i++)
                {
                    var isAllDay = random.NextDouble() < 0.1;
                    var start = faker.Date.Between(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(30));
                    var end = isAllDay ? start.Date.AddDays(1).AddSeconds(-1) : start.AddMinutes(faker.PickRandom(30, 60, 90, 120));
                    var isRecurring = random.NextDouble() < 0.2;

                    var evt = new Event
                    {
                        Id = Guid.NewGuid(),
                        OrganizerUserId = organizerId,
                        Title = faker.PickRandom(TitlePool),
                        Description = random.NextDouble() < 0.5 ? faker.Lorem.Sentence(8) : null,
                        Location = faker.PickRandom(Locations),
                        IsAllDay = isAllDay,
                        StartTime = start,
                        EndTime = end,
                        Timezone = faker.PickRandom("UTC", "Asia/Qatar", "Europe/London", "America/New_York"),
                        Recurrence = isRecurring ? new EventRecurrencePattern
                        {
                            Pattern = faker.PickRandom("Daily", "Weekly", "Monthly"),
                            Interval = 1,
                            DaysOfWeek = new List<string> { faker.PickRandom("Monday", "Tuesday", "Wednesday", "Thursday", "Friday") },
                            EndDate = DateTime.UtcNow.AddMonths(random.Next(1, 6)),
                            MaxOccurrences = null
                        } : null,
                        Visibility = faker.PickRandom<EventVisibility>(),
                        Color = random.NextDouble() < 0.5 ? faker.PickRandom("#4C8BF5", "#66BB6A", "#EF5350", "#FFCA28") : null,
                        ReminderMinutesBefore = faker.Random.Bool(0.7f)
                            ? new List<int> { faker.PickRandom(10, 15, 30, 60) }
                            : new List<int>(),
                        ConferenceLink = random.NextDouble() < 0.4 ? $"https://meet.example.com/{faker.Random.AlphaNumeric(10)}" : null,
                        CreatedAt = start.AddDays(-random.Next(1, 14))
                    };
                    events.Add(evt);

                    // organizer is implicitly attending; add 1-5 more attendees
                    var attendeeCount = random.Next(1, 6);
                    var candidateAttendees = userIds.Where(u => u != organizerId).OrderBy(_ => random.Next()).Take(attendeeCount).ToList();

                    foreach (var attendeeUserId in candidateAttendees)
                    {
                        var isPast = evt.StartTime < DateTime.UtcNow;
                        var response = isPast
                            ? faker.PickRandom("Accepted", "Accepted", "Declined", "Tentative") // mostly accepted for past events
                            : faker.PickRandom("Accepted", "Tentative", "NoResponse", "NoResponse");

                        attendees.Add(new EventAttendee
                        {
                            Id = Guid.NewGuid(),
                            EventId = evt.Id,
                            Email = userEmails[attendeeUserId],
                            UserId = attendeeUserId,
                            Response = response,
                            RespondedAt = response != "NoResponse" ? evt.CreatedAt.AddHours(random.Next(1, 72)) : null
                        });
                    }

                    // occasionally add an external (non-user) attendee
                    if (random.NextDouble() < 0.2)
                    {
                        attendees.Add(new EventAttendee
                        {
                            Id = Guid.NewGuid(),
                            EventId = evt.Id,
                            Email = faker.Internet.Email(),
                            UserId = null,
                            Response = "NoResponse",
                            RespondedAt = null
                        });
                    }
                }
            }

            await _context.Set<Event>().AddRangeAsync(events, ct);
            await _context.SaveChangesAsync(ct);
            await _context.Set<EventAttendee>().AddRangeAsync(attendees, ct);
            await _context.SaveChangesAsync(ct);

            return new EventSeedResult
            {
                Events = events.Count,
                Attendees = attendees.Count
            };
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<EventAttendee>().RemoveRange(_context.Set<EventAttendee>());
            _context.Set<Event>().RemoveRange(_context.Set<Event>());
            await _context.SaveChangesAsync(ct);
        }
    }
}
