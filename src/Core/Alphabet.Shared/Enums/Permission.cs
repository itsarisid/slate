namespace Alphabet.Shared.Enums;

[Flags]
public enum Permission
{
    None = 0,
    Read = 1,
    Create = 2,
    Update = 4,
    Delete = 8,
    Manage = Read | Create | Update | Delete
}
