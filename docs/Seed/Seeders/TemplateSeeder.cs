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
    public class TemplateSeedOptions
    {
        public int TemplatesPerUser { get; set; } = 2;
        public bool WipeExisting { get; set; } = false;
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
    }

    public class TemplateSeedResult
    {
        public int Templates { get; set; }
    }

    public interface ITemplateSeeder
    {
        Task<TemplateSeedResult> SeedAsync(TemplateSeedOptions options, CancellationToken ct = default);
    }

    public class TemplateSeeder : ITemplateSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        // Realistic (name, entityType, description, payload) presets, same
        // reasoning as Module 14's SmartList presets — a template is only
        // meaningful as a coherent prefill, not a random string blob.
        private static readonly (string Name, string EntityType, string Description, string TemplateJson)[] Presets =
        {
            ("New Employee Onboarding Checklist", "Todo",
                "Standard checklist for onboarding a new hire.",
                "{\"title\":\"Onboard {{employeeName}}\",\"checklist\":[\"Set up laptop\",\"Add to Slack\",\"Schedule orientation\",\"Assign buddy\"]}"),
            ("Weekly Status Report", "Note",
                "Template for the weekly team status note.",
                "{\"title\":\"Weekly Status - Week of {{date}}\",\"content\":\"## Highlights\\n\\n## Blockers\\n\\n## Next Week\"}"),
            ("Client Kickoff Meeting", "Event",
                "Standard agenda for a new client kickoff call.",
                "{\"title\":\"Kickoff: {{clientName}}\",\"durationMinutes\":60,\"description\":\"Introductions, scope review, timeline, next steps.\"}"),
            ("Bug Report", "WorkTask",
                "Standard structure for logging a bug.",
                "{\"title\":\"[Bug] {{summary}}\",\"description\":\"**Steps to reproduce:**\\n\\n**Expected:**\\n\\n**Actual:**\",\"priority\":\"High\"}"),
            ("Asset Retirement Request", "Asset",
                "Prefill for requesting an asset be retired.",
                "{\"reason\":\"End of life\",\"status\":\"Retired\"}"),
            ("Leave Request - Annual", "LeaveRequest",
                "Common prefill for a standard annual leave request.",
                "{\"leaveTypeCode\":\"ANNUAL\",\"reason\":\"Personal time off\"}"),
        };

        public TemplateSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<TemplateSeedResult> SeedAsync(TemplateSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var faker = new Faker();

            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, 15).Select(_ => Guid.NewGuid()).ToList();

            var templates = new List<Template>();
            foreach (var userId in userIds)
            {
                var chosen = Presets.OrderBy(_ => random.Next()).Take(Math.Min(options.TemplatesPerUser, Presets.Length));
                foreach (var preset in chosen)
                {
                    templates.Add(new Template
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Name = preset.Name,
                        EntityType = preset.EntityType,
                        Description = preset.Description,
                        TemplateJson = preset.TemplateJson,
                        UsageCount = faker.Random.Int(0, 40),
                        CreatedAt = faker.Date.Past(1)
                    });
                }
            }

            await _context.Set<Template>().AddRangeAsync(templates, ct);
            await _context.SaveChangesAsync(ct);

            return new TemplateSeedResult { Templates = templates.Count };
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<Template>().RemoveRange(_context.Set<Template>());
            await _context.SaveChangesAsync(ct);
        }
    }
}
