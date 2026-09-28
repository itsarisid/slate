// Entity inferred from CreateTemplateRequest in swagger.json.
// `template` (the payload) has no defined shape (`"nullable": true` with no
// type) — genuinely freeform, same situation as SmartList.Criteria in
// Module 14. Modeled as a JSON string; the instantiate endpoint just hands
// this payload back for the client to prefill a creation form with, so
// there's no separate "instance" entity to seed.

using System;

namespace Alphabet.Domain.Entities
{
    public class Template
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? UserId { get; set; } // owner; null if you want to model a global/shared template
        public string Name { get; set; } = default!;
        public string EntityType { get; set; } = default!; // e.g. "Todo", "WorkTask", "Note", "Event"
        public string? Description { get; set; }
        public string? TemplateJson { get; set; } // freeform prefill payload, serialized
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
