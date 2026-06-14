using Alphabet.Domain.Entities;

namespace Alphabet.Domain.Common;

/// <summary>
/// Explicit base type for aggregates that rely on audit metadata.
/// </summary>
public abstract class AuditableEntity : BaseEntity;
