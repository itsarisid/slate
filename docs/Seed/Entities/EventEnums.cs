// EventVisibility is a bare 3-value integer enum (0,1,2) in swagger.json —
// names below are best-guess, zero-based to match (consistent with the other
// Productivity module enums, NoteFormat and TodoStatus).

namespace Alphabet.Domain.Entities
{
    public enum EventVisibility
    {
        Private = 0,
        Public = 1,
        BusyOnly = 2
    }
}
