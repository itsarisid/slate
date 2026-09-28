// Priority(1-4) and TodoStatus(0-4) are bare integer enums in swagger.json —
// names below are best-guess. Note TodoStatus is zero-based (0..4), unlike
// most other enums in this API which start at 1 — worth double-checking
// against your real enum if one exists. Priority is shared across the
// Productivity module (Todos, Tasks, Reminders, Events all reference it),
// so this same enum will be reused as-is in those later modules.

namespace Alphabet.Domain.Entities
{
    public enum Priority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Urgent = 4
    }

    public enum TodoStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3,
        Overdue = 4
    }
}
