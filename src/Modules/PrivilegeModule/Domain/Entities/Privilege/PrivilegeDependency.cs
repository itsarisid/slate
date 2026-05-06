namespace Alphabet.Domain.Entities.Privilege;

/// <summary>
/// Represents a dependency relationship between two privileges.
/// </summary>
public sealed class PrivilegeDependency
{
    public Guid PrivilegeId { get; set; }

    public Guid DependsOnPrivilegeId { get; set; }
}
