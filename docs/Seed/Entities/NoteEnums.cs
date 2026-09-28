// NoteFormat is a bare 3-value integer enum (0,1,2) in swagger.json — names
// below are best-guess, zero-based to match. Match numeric values against
// yours if it already exists.

namespace Alphabet.Domain.Entities
{
    public enum NoteFormat
    {
        PlainText = 0,
        Markdown = 1,
        RichText = 2
    }
}
