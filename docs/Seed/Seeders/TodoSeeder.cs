// Requires: dotnet add package Bogus
//
// Seeds, in order: Todos -> TodoChecklistItems.
// USER REFERENCES: UserId/AssignedTo are random Guids unless you pass real
// ones via ExistingUserIds (see Module 7/8 for how to get real ones).

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
    public class TodoSeedOptions
    {
        public int TodosPerUser { get; set; } = 8;
        public bool WipeExisting { get; set; } = false;
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
    }

    public class TodoSeedResult
    {
        public int Todos { get; set; }
        public int ChecklistItems { get; set; }
    }

    public interface ITodoSeeder
    {
        Task<TodoSeedResult> SeedAsync(TodoSeedOptions options, CancellationToken ct = default);
    }

    public class TodoSeeder : ITodoSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        private static readonly string[] TitlePool =
        {
            "Follow up with vendor", "Review Q3 expense report", "Update asset inventory list",
            "Prepare onboarding docs", "Book meeting room for offsite", "Renew software license",
            "Draft policy update", "Schedule 1:1 with team", "Clean up shared drive",
            "Respond to audit request", "Order office supplies", "Submit timesheet",
            "Review pending leave requests", "Test backup restore process", "Update emergency contacts",
            "Sync with IT on laptop refresh", "Prepare board presentation", "Reconcile monthly invoices",
            "Plan team lunch", "Archive completed projects"
        };

        private static readonly string[] Categories = { "Work", "Personal", "Admin", "Finance", "IT", "HR" };

        public TodoSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<TodoSeedResult> SeedAsync(TodoSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var faker = new Faker();

            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, 15).Select(_ => Guid.NewGuid()).ToList();

            var todos = new List<Todo>();
            var checklistItems = new List<TodoChecklistItem>();

            foreach (var userId in userIds)
            {
                var count = options.TodosPerUser;
                for (int i = 0; i < count; i++)
                {
                    var createdAt = faker.Date.Past(1);
                    var hasDueDate = random.NextDouble() < 0.75;
                    DateTime? dueDate = hasDueDate ? faker.Date.Between(createdAt, DateTime.UtcNow.AddDays(30)) : null;

                    // Derive status from due date so the data is internally consistent.
                    TodoStatus status;
                    DateTime? completedAt = null;
                    var roll = random.NextDouble();
                    if (roll < 0.4)
                    {
                        status = TodoStatus.Completed;
                        completedAt = dueDate.HasValue
                            ? dueDate.Value.AddDays(-random.Next(0, 5))
                            : createdAt.AddDays(random.Next(1, 10));
                    }
                    else if (roll < 0.5)
                    {
                        status = TodoStatus.Cancelled;
                    }
                    else if (dueDate.HasValue && dueDate.Value < DateTime.UtcNow)
                    {
                        status = TodoStatus.Overdue;
                    }
                    else if (roll < 0.75)
                    {
                        status = TodoStatus.InProgress;
                    }
                    else
                    {
                        status = TodoStatus.NotStarted;
                    }

                    var isRecurring = random.NextDouble() < 0.15;

                    var todo = new Todo
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Title = faker.PickRandom(TitlePool),
                        Description = random.NextDouble() < 0.5 ? faker.Lorem.Sentence(10) : null,
                        Priority = faker.PickRandom<Priority>(),
                        Status = status,
                        DueDate = dueDate,
                        ReminderMinutesBefore = dueDate.HasValue && random.NextDouble() < 0.5
                            ? faker.PickRandom(15, 30, 60, 1440)
                            : null,
                        Category = faker.PickRandom(Categories),
                        Tags = random.NextDouble() < 0.4
                            ? new List<string> { faker.Lorem.Word() }
                            : new List<string>(),
                        IsRecurring = isRecurring,
                        RecurrencePattern = isRecurring ? new TodoRecurrencePattern
                        {
                            Pattern = faker.PickRandom("Daily", "Weekly", "Monthly"),
                            Interval = faker.PickRandom(1, 2),
                            DaysOfWeek = faker.PickRandom("Weekly") == "Weekly"
                                ? new List<string> { faker.PickRandom("Monday", "Wednesday", "Friday") }
                                : new List<string>(),
                            EndDate = random.NextDouble() < 0.5 ? DateTime.UtcNow.AddMonths(random.Next(1, 12)) : null,
                            MaxOccurrences = random.NextDouble() < 0.3 ? faker.Random.Int(5, 50) : null
                        } : null,
                        AssignedTo = random.NextDouble() < 0.2 ? userIds[random.Next(userIds.Count)] : null,
                        CompletedAt = completedAt,
                        CreatedAt = createdAt,
                        UpdatedAt = completedAt ?? createdAt
                    };
                    todos.Add(todo);

                    if (random.NextDouble() < 0.4)
                    {
                        var itemCount = random.Next(1, 6);
                        for (int c = 0; c < itemCount; c++)
                        {
                            checklistItems.Add(new TodoChecklistItem
                            {
                                Id = Guid.NewGuid(),
                                TodoId = todo.Id,
                                Text = faker.Lorem.Sentence(4),
                                Completed = status == TodoStatus.Completed || random.NextDouble() < 0.3,
                                Order = c
                            });
                        }
                    }
                }
            }

            await _context.Set<Todo>().AddRangeAsync(todos, ct);
            await _context.SaveChangesAsync(ct);
            await _context.Set<TodoChecklistItem>().AddRangeAsync(checklistItems, ct);
            await _context.SaveChangesAsync(ct);

            return new TodoSeedResult
            {
                Todos = todos.Count,
                ChecklistItems = checklistItems.Count
            };
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<TodoChecklistItem>().RemoveRange(_context.Set<TodoChecklistItem>());
            _context.Set<Todo>().RemoveRange(_context.Set<Todo>());
            await _context.SaveChangesAsync(ct);
        }
    }
}
