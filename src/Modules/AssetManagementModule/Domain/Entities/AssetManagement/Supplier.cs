namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents an asset supplier or vendor.
/// </summary>
public sealed class Supplier : BaseEntity
{
    private Supplier()
    {
    }

    private Supplier(string name, string? contactEmail, string? contactPhone)
    {
        Name = name.Trim();
        ContactEmail = contactEmail?.Trim();
        ContactPhone = contactPhone?.Trim();
    }

    public string Name { get; private set; } = string.Empty;

    public string? ContactEmail { get; private set; }

    public string? ContactPhone { get; private set; }

    public static Supplier Create(string name, string? contactEmail, string? contactPhone)
        => new(name, contactEmail, contactPhone);
}
