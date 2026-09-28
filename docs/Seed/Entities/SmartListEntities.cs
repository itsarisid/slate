// This tag ("Productivity Module - Smart Views") covers three endpoints:
//   GET /api/v1/search           — ad-hoc search, nothing to seed (query-time only)
//   GET /api/v1/dashboard/today  — computed from other modules' data
//                                  (DashboardStatsDto is entirely derived from
//                                  SchedulerJob/JobExecution — see Module 5 —
//                                  there's nothing new to persist for it)
//   POST /api/v1/smart-lists     — the one real entity: a saved search/filter
//
// So this module only seeds SmartList.
//
// CreateSmartListRequest.criteria has no defined shape in swagger (just
// `"nullable": true` with no type) — it's genuinely freeform. Modeled here
// as a JSON string; swap for a JSON column type if your database supports
// one natively (e.g. Npgsql's jsonb, or SQL Server's nvarchar(max) is fine
// too — just don't try to give it a fixed shape).

using System;

namespace Alphabet.Domain.Entities
{
    public class SmartList
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } // owner
        public string Name { get; set; } = default!;
        public string EntityType { get; set; } = default!; // e.g. "Todo", "Task", "Asset", "LeaveRequest"
        public string? CriteriaJson { get; set; } // freeform filter criteria, serialized
        public bool IsShared { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
