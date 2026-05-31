using Alphabet.Domain.Enums;
using Alphabet.Domain.Exceptions;

namespace Alphabet.Domain.Entities.Privilege;

/// <summary>
/// Represents a reusable composite privilege policy.
/// </summary>
public sealed class PrivilegePolicy : BaseEntity
{
    private readonly HashSet<string> _privilegeNames = new(StringComparer.OrdinalIgnoreCase);

    private PrivilegePolicy()
    {
    }

    private PrivilegePolicy(string name, string? description, IEnumerable<string> privilegeNames, PrivilegePolicyCondition condition)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Privilege policy name is required.");
        }

        Name = name.Trim();
        Description = description?.Trim();
        Condition = condition;

        foreach (var privilegeName in privilegeNames.Where(static value => !string.IsNullOrWhiteSpace(value)))
        {
            _privilegeNames.Add(privilegeName.Trim().ToLowerInvariant());
        }
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public IReadOnlyCollection<string> PrivilegeNames => _privilegeNames.ToArray();

    public PrivilegePolicyCondition Condition { get; private set; }
    /// <summary>
    /// Create.
    /// </summary>

    public static PrivilegePolicy Create(
        string name,
        string? description,
        IEnumerable<string> privilegeNames,
        PrivilegePolicyCondition condition)
    {
        return new PrivilegePolicy(name, description, privilegeNames, condition);
    }
}
