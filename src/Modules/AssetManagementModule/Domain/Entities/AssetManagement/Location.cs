using Alphabet.Domain.Enums;
using Alphabet.Domain.Exceptions;
using Alphabet.Domain.ValueObjects;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a physical or logical asset location.
/// </summary>
public sealed class Location : BaseEntity
{
    private Location()
    {
    }

    private Location(
        string name,
        string code,
        AssetLocationType type,
        Address address,
        Guid? parentLocationId,
        bool isActive,
        Coordinates? coordinates,
        string? contactPerson,
        string? contactPhone)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Location name is required.");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Location code is required.");
        }

        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        Type = type;
        Address = address;
        ParentLocationId = parentLocationId;
        IsActive = isActive;
        Coordinates = coordinates;
        ContactPerson = string.IsNullOrWhiteSpace(contactPerson) ? null : contactPerson.Trim();
        ContactPhone = string.IsNullOrWhiteSpace(contactPhone) ? null : contactPhone.Trim();
    }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public AssetLocationType Type { get; private set; }

    public Address Address { get; private set; } = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    public Guid? ParentLocationId { get; private set; }

    public bool IsActive { get; private set; }

    public Coordinates? Coordinates { get; private set; }

    public string? ContactPerson { get; private set; }

    public string? ContactPhone { get; private set; }

    /// <summary>
    /// Creates a new location.
    /// </summary>
    public static Location Create(
        string name,
        string code,
        AssetLocationType type,
        Address address,
        Guid? parentLocationId,
        bool isActive,
        Coordinates? coordinates,
        string? contactPerson,
        string? contactPhone)
    {
        return new Location(name, code, type, address, parentLocationId, isActive, coordinates, contactPerson, contactPhone);
    }
}
