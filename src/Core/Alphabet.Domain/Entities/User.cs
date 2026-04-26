using Alphabet.Domain.ValueObjects;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents an application user.
/// </summary>
public sealed class User : BaseEntity
{
    private readonly List<string> _roles = [];

    private User()
    {
    }

    private User(string firstName, string lastName, Email email)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email;
    }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public Email Email { get; private set; } = Email.Create("unknown@example.com");

    public IReadOnlyCollection<string> Roles => _roles.AsReadOnly();

    public static User Register(string firstName, string lastName, Email email)
    {
        return new User(firstName, lastName, email);
    }

    public void GrantRole(string role)
    {
        if (!_roles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            _roles.Add(role);
            Touch();
        }
    }
}
