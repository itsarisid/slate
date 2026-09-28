// Requires: dotnet add package Bogus
//
// Seeds, in order: CommunicationConfiguration (singleton, upserted) ->
// CommunicationBatches -> CommunicationDeliveryResults.
//
// USER REFERENCES: RecipientUserId/SentByUserId are random Guids unless you
// pass real ones via ExistingUserIds.

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
    public class CommunicationSeedOptions
    {
        public int BatchCount { get; set; } = 60;
        public bool WipeExisting { get; set; } = false;
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
    }

    public class CommunicationSeedResult
    {
        public int Batches { get; set; }
        public int DeliveryResults { get; set; }
        public bool ConfigurationSeeded { get; set; }
    }

    public interface ICommunicationSeeder
    {
        Task<CommunicationSeedResult> SeedAsync(CommunicationSeedOptions options, CancellationToken ct = default);
    }

    public class CommunicationSeeder : ICommunicationSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        private static readonly (string Subject, string Body)[] MessagePool =
        {
            ("Your leave request was approved", "Your leave request for {{dates}} has been approved."),
            ("Asset maintenance due", "Asset {{assetTag}} is due for maintenance on {{date}}."),
            ("Welcome to Alphabet", "Your account has been created. Here's how to get started."),
            ("Password reset requested", "We received a request to reset your password."),
            ("New task assigned to you", "You've been assigned a new task: {{taskTitle}}."),
            ("Privilege access request pending", "You have a pending access request awaiting your decision."),
            ("Scheduler job failed", "Job '{{jobName}}' failed on its last run. Please review."),
            ("Meeting reminder", "Reminder: '{{eventTitle}}' starts in 15 minutes."),
            ("Weekly digest", "Here's your weekly summary of tasks, todos, and events."),
            ("Workflow approval needed", "A workflow instance is waiting on your approval."),
        };

        public CommunicationSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<CommunicationSeedResult> SeedAsync(CommunicationSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var faker = new Faker();

            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, 15).Select(_ => Guid.NewGuid()).ToList();

            // ---- Configuration (singleton) ----
            var existingConfig = await _context.Set<CommunicationConfiguration>().FirstOrDefaultAsync(ct);
            bool configSeeded = false;
            if (existingConfig == null)
            {
                await _context.Set<CommunicationConfiguration>().AddAsync(new CommunicationConfiguration
                {
                    Id = Guid.NewGuid(),
                    EnabledChannels = new List<string> { "Email", "Push", "SMS" },
                    DefaultChannel = "Email",
                    DetailedLoggingEnabled = true
                }, ct);
                await _context.SaveChangesAsync(ct);
                configSeeded = true;
            }

            // ---- Batches + delivery results ----
            var batches = new List<CommunicationBatch>();
            var results = new List<CommunicationDeliveryResult>();
            var allChannels = new[] { "Email", "SMS", "Push", "Webhook" };

            for (int i = 0; i < options.BatchCount; i++)
            {
                var (subject, body) = faker.PickRandom(MessagePool);
                var sentAt = faker.Date.Past(1);
                var recipientUserId = userIds[random.Next(userIds.Count)];
                var requestedChannels = allChannels.OrderBy(_ => random.Next()).Take(random.Next(1, 3)).ToList();

                var batch = new CommunicationBatch
                {
                    Id = Guid.NewGuid(),
                    Subject = subject,
                    Body = body,
                    IsHtml = random.NextDouble() < 0.4,
                    RecipientUserId = recipientUserId,
                    EmailAddress = requestedChannels.Contains("Email") ? faker.Internet.Email() : null,
                    PhoneNumber = requestedChannels.Contains("SMS") ? faker.Phone.PhoneNumber() : null,
                    PushToken = requestedChannels.Contains("Push") ? faker.Random.Guid().ToString() : null,
                    WebhookUrl = requestedChannels.Contains("Webhook") ? $"https://hooks.example.com/{faker.Random.AlphaNumeric(12)}" : null,
                    TotalChannelsRequested = requestedChannels.Count,
                    SentByUserId = random.NextDouble() < 0.3 ? userIds[random.Next(userIds.Count)] : null, // null = system-triggered
                    SentAt = sentAt
                };

                var successCount = 0;
                foreach (var channel in requestedChannels)
                {
                    var isSuccess = random.NextDouble() < 0.92; // most deliveries succeed
                    if (isSuccess) successCount++;

                    results.Add(new CommunicationDeliveryResult
                    {
                        Id = Guid.NewGuid(),
                        BatchId = batch.Id,
                        Channel = channel,
                        IsSuccess = isSuccess,
                        Message = isSuccess ? "Delivered." : faker.PickRandom(
                            "Provider timeout.", "Invalid recipient address.", "Rate limit exceeded.", "Unknown delivery error."),
                        ProcessedAt = sentAt.AddSeconds(random.Next(1, 30))
                    });
                }
                batch.SuccessfulChannels = successCount;
                batch.FailedChannels = requestedChannels.Count - successCount;

                batches.Add(batch);
            }

            await _context.Set<CommunicationBatch>().AddRangeAsync(batches, ct);
            await _context.SaveChangesAsync(ct);
            await _context.Set<CommunicationDeliveryResult>().AddRangeAsync(results, ct);
            await _context.SaveChangesAsync(ct);

            return new CommunicationSeedResult
            {
                Batches = batches.Count,
                DeliveryResults = results.Count,
                ConfigurationSeeded = configSeeded
            };
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<CommunicationDeliveryResult>().RemoveRange(_context.Set<CommunicationDeliveryResult>());
            _context.Set<CommunicationBatch>().RemoveRange(_context.Set<CommunicationBatch>());
            // Configuration is left alone on wipe — it's a singleton setting, not sample data.
            await _context.SaveChangesAsync(ct);
        }
    }
}
