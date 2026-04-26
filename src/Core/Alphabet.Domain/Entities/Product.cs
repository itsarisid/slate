using Alphabet.Domain.Enums;
using Alphabet.Domain.Exceptions;
using Alphabet.Domain.ValueObjects;

namespace Alphabet.Domain.Entities;

/// <summary>
/// Represents a product aggregate root.
/// </summary>
public sealed class Product : BaseEntity
{
    private Product()
    {
    }

    private Product(string name, string description, Money price)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new DomainException("Product name is required.") : name.Trim();
        Description = description.Trim();
        Price = price;
        Status = ProductStatus.Draft;
    }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Money Price { get; private set; } = Money.Zero("USD");

    public ProductStatus Status { get; private set; }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    public static Product Create(string name, string description, Money price)
    {
        return new Product(name, description, price);
    }

    /// <summary>
    /// Updates the product details.
    /// </summary>
    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        Name = name.Trim();
        Description = description.Trim();
        Touch();
    }

    /// <summary>
    /// Changes the current product price.
    /// </summary>
    public void ChangePrice(Money price)
    {
        Price = price;
        Touch();
    }

    /// <summary>
    /// Activates the product for sale.
    /// </summary>
    public void Activate()
    {
        Status = ProductStatus.Active;
        Touch();
    }

    /// <summary>
    /// Deactivates the product.
    /// </summary>
    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
        Touch();
    }
}
