// NAMING NOTE: the entity here is called WorkTask (not Task) and this enum
// WorkTaskStatus (not TaskStatus), specifically to avoid colliding with
// System.Threading.Tasks.Task / System.Threading.Tasks.TaskStatus, which are
// used constantly in any async C# codebase. Rename back to Task/TaskStatus
// if you'd rather, just add a `using Task = Alphabet.Domain.Entities.WorkTask;`
// alias (or a fully-qualified `Threading.Tasks.Task` elsewhere) to keep the
// two apart.
//
// WorkTaskStatus is a bare 5-value integer enum (0-4) in swagger.json — the
// names below assume a kanban-style board, matching GET /tasks/board,
// zero-based to match. Priority is reused as-is from Module 9 (Todos) since
// it's the same shared enum across the Productivity module.

namespace Alphabet.Domain.Entities
{
    public enum WorkTaskStatus
    {
        Backlog = 0,
        ToDo = 1,
        InProgress = 2,
        InReview = 3,
        Done = 4
    }
}
