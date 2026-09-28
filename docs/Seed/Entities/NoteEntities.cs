// Entities inferred from CreateNoteRequest/UpdateNoteRequest,
// CreateNotebookRequest, and ShareNoteRequest in swagger.json.
//
// NoteVersion doesn't have a corresponding Create*Request (GET
// /notes/{noteId}/versions is read-only in the API — versions are presumably
// created automatically on each update), so I've added a simple version
// history entity for seeding purposes.

using System;
using System.Collections.Generic;

namespace Alphabet.Domain.Entities
{
    public class Notebook
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } // owner
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public Guid? ParentNotebookId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Note
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } // owner
        public Guid? NotebookId { get; set; }
        public string Title { get; set; } = default!;
        public string? Content { get; set; }
        public NoteFormat Format { get; set; }
        public string? Category { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? Color { get; set; }
        public bool IsPinned { get; set; }
        public bool IsFavorite { get; set; }
        public List<string> Collaborators { get; set; } = new(); // emails, set at creation
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class NoteShare
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid NoteId { get; set; }
        public string SharedWithEmail { get; set; } = default!;
        public string Permission { get; set; } = "View"; // "View" | "Edit", free text per swagger
        public DateTime SharedAt { get; set; }
    }

    public class NoteVersion
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid NoteId { get; set; }
        public int VersionNumber { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
