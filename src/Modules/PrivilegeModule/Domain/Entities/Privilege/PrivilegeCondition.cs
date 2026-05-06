namespace Alphabet.Domain.Entities.Privilege;

/// <summary>
/// Represents an optional attribute-based condition for a privilege.
/// </summary>
public sealed class PrivilegeCondition : BaseEntity
{
    public Guid PrivilegeId { get; set; }

    public string AttributeName { get; set; } = string.Empty;

    public string Operator { get; set; } = string.Empty;

    public string AttributeValue { get; set; } = string.Empty;
}
