// Requires: dotnet add package Bogus
//
// Seeds, in order: Notebooks -> Notes -> NoteShares -> NoteVersions.
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
    public class NoteSeedOptions
    {
        public int NotebooksPerUser { get; set; } = 2;
        public int NotesPerUser { get; set; } = 10;
        public bool WipeExisting { get; set; } = false;
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
    }

    public class NoteSeedResult
    {
        public int Notebooks { get; set; }
        public int Notes { get; set; }
        public int Shares { get; set; }
        public int Versions { get; set; }
    }

    public interface INoteSeeder
    {
        Task<NoteSeedResult> SeedAsync(NoteSeedOptions options, CancellationToken ct = default);
    }

    public class NoteSeeder : INoteSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        private static readonly string[] NotebookNames =
        {
            "Work", "Personal", "Meeting Notes", "Project Ideas", "Reference", "Archive"
        };

        private static readonly string[] NoteTitlePool =
        {
            "Sprint planning notes", "Vendor call summary", "Onboarding checklist draft",
            "Ideas for Q4 offsite", "Asset audit findings", "Leave policy questions",
            "Interview feedback", "Budget planning notes", "Client requirements",
            "Retro action items", "Architecture decision log", "Password rotation reminder",
            "Travel itinerary notes", "Book recommendations", "Weekly standup summary"
        };

        public NoteSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<NoteSeedResult> SeedAsync(NoteSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var faker = new Faker();

            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, 15).Select(_ => Guid.NewGuid()).ToList();

            // ---- Notebooks ----
            var notebooks = new List<Notebook>();
            foreach (var userId in userIds)
            {
                var names = NotebookNames.OrderBy(_ => random.Next()).Take(options.NotebooksPerUser).ToList();
                Notebook? previous = null;
                foreach (var name in names)
                {
                    var notebook = new Notebook
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Name = name,
                        Description = random.NextDouble() < 0.4 ? faker.Lorem.Sentence() : null,
                        Color = faker.PickRandom("#4C8BF5", "#66BB6A", "#EF5350", "#FFCA28", "#AB47BC"),
                        // occasionally nest under the previous notebook for this user
                        ParentNotebookId = previous != null && random.NextDouble() < 0.2 ? previous.Id : null,
                        CreatedAt = faker.Date.Past(1)
                    };
                    notebooks.Add(notebook);
                    previous = notebook;
                }
            }
            await _context.Set<Notebook>().AddRangeAsync(notebooks, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Notes ----
            var notes = new List<Note>();
            foreach (var userId in userIds)
            {
                var userNotebooks = notebooks.Where(n => n.UserId == userId).ToList();

                for (int i = 0; i < options.NotesPerUser; i++)
                {
                    var createdAt = faker.Date.Past(1);
                    var format = faker.PickRandom<NoteFormat>();

                    var note = new Note
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        NotebookId = userNotebooks.Count > 0 && random.NextDouble() < 0.8
                            ? faker.PickRandom(userNotebooks).Id
                            : null,
                        Title = faker.PickRandom(NoteTitlePool),
                        Content = format switch
                        {
                            NoteFormat.Markdown => $"## {faker.Lorem.Sentence(3)}\n\n{faker.Lorem.Paragraphs(2)}",
                            NoteFormat.RichText => $"<p>{faker.Lorem.Paragraph()}</p>",
                            _ => faker.Lorem.Paragraph()
                        },
                        Format = format,
                        Category = random.NextDouble() < 0.5 ? faker.PickRandom("Work", "Personal", "Ideas", "Reference") : null,
                        Tags = random.NextDouble() < 0.4
                            ? new List<string> { faker.Lorem.Word() }
                            : new List<string>(),
                        Color = random.NextDouble() < 0.5 ? faker.PickRandom("#FFF9C4", "#C8E6C9", "#BBDEFB", "#F8BBD0") : null,
                        IsPinned = random.NextDouble() < 0.15,
                        IsFavorite = random.NextDouble() < 0.2,
                        Collaborators = random.NextDouble() < 0.15
                            ? new List<string> { faker.Internet.Email() }
                            : new List<string>(),
                        CreatedAt = createdAt,
                        UpdatedAt = random.NextDouble() < 0.5 ? faker.Date.Between(createdAt, DateTime.UtcNow) : createdAt
                    };
                    notes.Add(note);
                }
            }
            await _context.Set<Note>().AddRangeAsync(notes, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Shares (for notes that already listed collaborators, plus a few extra) ----
            var shares = new List<NoteShare>();
            foreach (var note in notes)
            {
                foreach (var email in note.Collaborators)
                {
                    shares.Add(new NoteShare
                    {
                        Id = Guid.NewGuid(),
                        NoteId = note.Id,
                        SharedWithEmail = email,
                        Permission = faker.PickRandom("View", "Edit"),
                        SharedAt = note.CreatedAt.AddMinutes(random.Next(5, 600))
                    });
                }
                // occasionally share even without a pre-listed collaborator
                if (note.Collaborators.Count == 0 && random.NextDouble() < 0.1)
                {
                    shares.Add(new NoteShare
                    {
                        Id = Guid.NewGuid(),
                        NoteId = note.Id,
                        SharedWithEmail = faker.Internet.Email(),
                        Permission = faker.PickRandom("View", "Edit"),
                        SharedAt = note.UpdatedAt
                    });
                }
            }
            await _context.Set<NoteShare>().AddRangeAsync(shares, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Versions (1-4 per note, only for notes updated after creation) ----
            var versions = new List<NoteVersion>();
            foreach (var note in notes)
            {
                var versionCount = note.UpdatedAt > note.CreatedAt ? random.Next(1, 5) : 0;
                for (int v = 0; v < versionCount; v++)
                {
                    versions.Add(new NoteVersion
                    {
                        Id = Guid.NewGuid(),
                        NoteId = note.Id,
                        VersionNumber = v + 1,
                        Content = faker.Lorem.Paragraph(),
                        CreatedAt = note.CreatedAt.AddHours((v + 1) * random.Next(1, 48)),
                        CreatedByUserId = note.UserId
                    });
                }
            }
            await _context.Set<NoteVersion>().AddRangeAsync(versions, ct);
            await _context.SaveChangesAsync(ct);

            return new NoteSeedResult
            {
                Notebooks = notebooks.Count,
                Notes = notes.Count,
                Shares = shares.Count,
                Versions = versions.Count
            };
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<NoteVersion>().RemoveRange(_context.Set<NoteVersion>());
            _context.Set<NoteShare>().RemoveRange(_context.Set<NoteShare>());
            _context.Set<Note>().RemoveRange(_context.Set<Note>());
            _context.Set<Notebook>().RemoveRange(_context.Set<Notebook>());
            await _context.SaveChangesAsync(ct);
        }
    }
}
