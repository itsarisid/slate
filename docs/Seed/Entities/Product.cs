// NOTE: This is inferred from the CreateProductRequest / ProductResponseDto
// schemas in swagger.json. Replace with (or reconcile against) your real
// Product entity if one already exists in your project.
//
// Adjust namespace to match your project structure, e.g.:
//   Alphabet.Domain.Entities / Alphabet.Core.Entities / etc.

using System;

namespace Alphabet.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public double Price { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }   // e.g. "Active", "Draft", "Discontinued"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
