// Entities inferred from SendCommunicationCommand, CommunicationBatchResponseDto,
// CommunicationDeliveryResultDto, and CommunicationConfigurationDto in
// swagger.json.
//
// CommunicationConfiguration is modeled as a singleton — one row describing
// which channels are enabled org-wide (matches GET /communications/configuration
// returning a single object, not a list).

using System;
using System.Collections.Generic;

namespace Alphabet.Domain.Entities
{
    public class CommunicationBatch
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public bool IsHtml { get; set; }
        public Guid? RecipientUserId { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PushToken { get; set; }
        public string? WebhookUrl { get; set; }
        public int TotalChannelsRequested { get; set; }
        public int SuccessfulChannels { get; set; }
        public int FailedChannels { get; set; }
        public Guid? SentByUserId { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }

    public class CommunicationDeliveryResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BatchId { get; set; }
        public string Channel { get; set; } = default!; // "Email", "SMS", "Push", "Webhook"
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    public class CommunicationConfiguration
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<string> EnabledChannels { get; set; } = new();
        public string? DefaultChannel { get; set; }
        public bool DetailedLoggingEnabled { get; set; }
    }
}
