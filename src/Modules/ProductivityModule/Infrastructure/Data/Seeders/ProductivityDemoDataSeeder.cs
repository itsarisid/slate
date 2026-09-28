using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using ProductivityTaskStatus = Alphabet.Domain.Enums.TaskStatus;

namespace Alphabet.Infrastructure.Data.Seeders;

/// <summary>Seeds repeat-safe sample records for the productivity module.</summary>
public sealed class ProductivityDemoDataSeeder(AppDbContext dbContext) : IDemoDataSeeder
{
    private const string Marker = "[Demo]";

    public async Task<DemoDataSeedResult> SeedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var seededTodos = await dbContext.ProductivityTodos.AnyAsync(
            x => x.CreatedByUserId == userId && x.Title.StartsWith(Marker), cancellationToken);
        var seededTasks = await dbContext.ProductivityTasks.AnyAsync(
            x => x.OwnerUserId == userId && x.Title.StartsWith(Marker), cancellationToken);
        var seededNotes = await dbContext.ProductivityNotes.AnyAsync(
            x => x.OwnerUserId == userId && x.Title.StartsWith(Marker), cancellationToken);
        var seededEvents = await dbContext.ProductivityCalendarEvents.AnyAsync(
            x => x.OwnerUserId == userId && x.Title.StartsWith(Marker), cancellationToken);

        var todoCount = 0;
        var taskCount = 0;
        var noteCount = 0;
        var eventCount = 0;

        if (!seededTodos)
        {
            dbContext.ProductivityTodos.AddRange(
                Todo.Create(userId, userId, $"{Marker} Review project plan", "Check milestones and owners.", Priority.High, now.AddDays(2), "Work", 30, false, null),
                Todo.Create(userId, userId, $"{Marker} Prepare weekly update", "Summarize progress and blockers.", Priority.Medium, now.AddDays(4), "Work", null, false, null),
                Todo.Create(userId, userId, $"{Marker} Organize desk", "Clear and organize the workspace.", Priority.Low, now.AddDays(7), "Personal", null, false, null));
            todoCount = 3;
        }

        if (!seededTasks)
        {
            dbContext.ProductivityTasks.AddRange(
                ProductivityTask.Create(userId, $"{Marker} Launch checklist", "Complete the launch preparation work.", Priority.High, ProductivityTaskStatus.InProgress, now.AddDays(5), 6m, userId, null, null, null, null),
                ProductivityTask.Create(userId, $"{Marker} Review feedback", "Review the latest customer feedback.", Priority.Medium, ProductivityTaskStatus.NotStarted, now.AddDays(8), 3m, userId, null, null, null, null));
            taskCount = 2;
        }

        if (!seededNotes)
        {
            dbContext.ProductivityNotes.AddRange(
                Note.Create(userId, $"{Marker} Project notes", "Sample project notes for trying search and note views.", NoteFormat.Markdown, "Work", "#4F46E5", true, false, null, null),
                Note.Create(userId, $"{Marker} Ideas", "Capture and refine ideas here.", NoteFormat.Plain, "Personal", "#059669", false, true, null, null));
            noteCount = 2;
        }

        if (!seededEvents)
        {
            dbContext.ProductivityCalendarEvents.Add(
                CalendarEvent.Create(userId, $"{Marker} Planning session", "Discuss milestones and next steps.", "Meeting room", false, now.AddDays(1), now.AddDays(1).AddHours(1), "UTC", null, null, EventVisibility.Private, "#2563EB", [15], null));
            eventCount = 1;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new DemoDataSeedResult(todoCount, taskCount, noteCount, eventCount);
    }
}
